using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Hunt : FSMState<AIBehaviour>
{
    public bool moveHome = false;
    public bool atTarget = false;
    public bool atHome = false;
    public bool attack = false;
    public bool moving = false;

    private float lastAttack = 0.0f;
    private int foodCarried = 0;

    private GameObject targetCreature = null;

    private AILerp ais3;

    public Hunt()
    {

    }

    public override void Enter(AIBehaviour entity)
    {
        //Debug.Log(entity.gameObject.name + ": Snoo Snoo");
        //Eventually replace with a graphical text bubble "Zzz"
        entity.GetComponent<AILerp>().enabled = true;
        SelectTarget(entity);
    }

    public override void Execute(AIBehaviour entity)
    {
        //To hunt, choose the nearest population (with lowest threat - IE check those not threats first, then if none then check closest)
        //  After, choose a random creature to hunt -- Would choose closest, but eventually if there are hundreds and checking all 100, could result in slow performance
        //                                          -- At same time, could result in going half way across the map (through "enemy territory" etc)
        //                                          -- Will need some sort of 'Target Fitness Eval' based on threat, distance, surrounding enemies etc

        //Check there is currently a target (IE hasn't been killed/deleted)
        if (targetCreature == null)
        {
            SelectTarget(entity);
            if(targetCreature == null || targetCreature.GetComponent<CreatureManager>() == null)
            {
                
                return;
            }
            if(targetCreature.GetComponent<CreatureManager>().inSetup)
            {
                targetCreature = null;
                return;
            }
        }
        else
        {
            if (!moveHome)
            {
                //If a mate is chosen, check they are ready to mate, if so then move to them, then move towards the Population Centre
                PerformCombat(entity);
            }
            else
            {
                entity.GetComponent<AILerp>().enabled = true;
                MoveHome(entity);
                //MoveToTarget(entity, entity.GetComponent<HumanManager>().population.transform);
            }
        }
    }

    public override void Exit(AIBehaviour entity)
    {
        //Debug.Log(entity.gameObject.name + " woke up!");
        //Eventually replace with a graphical text bubble "!"
    }

    private void SelectTarget(AIBehaviour entity)
    {

        List<int> threatPopsIndices = new List<int>();

        if (entity.GetComponent<CreatureManager>().population.threateningPops != null)
        {
            threatPopsIndices = entity.GetComponent<CreatureManager>().population.threateningPops;
        }

        List<Population> discoveredPops = new List<Population>();

        if (entity.GetComponent<CreatureManager>().population.discoveredPopulations != null)
        {
            discoveredPops = entity.GetComponent<CreatureManager>().population.discoveredPopulations;
        }

        float distance = Mathf.Infinity;
        GameObject popTarget = null;
        GameObject tempTarget = null;
        for (int i = 0; i < discoveredPops.Count; i++)
        {
            if(discoveredPops[i] == null)
            {
                Debug.Log("discoveredPops: " + entity.gameObject.name + " at index: " + i + " is NULL");
                continue;
            }

            float thisDist = Vector2.Distance(entity.transform.position, discoveredPops[i].transform.position);

            if (thisDist < 0)
            {
                thisDist *= -1;
            }

            if (thisDist < distance)
            {
                bool threat = false;
                distance = thisDist;
                for (int j = 0; j < threatPopsIndices.Count; j++)
                {
                    if (discoveredPops[i].populationIndex == threatPopsIndices[j])
                    {
                        threat = true;
                        tempTarget = discoveredPops[i].gameObject;
                        break;
                    }
                }
                if (!threat)
                {
                    popTarget = discoveredPops[i].gameObject;
                }
            }
        }

        if (popTarget == null)
        {
            if (tempTarget != null)
            {
                popTarget = tempTarget;
            }
        }

        if (popTarget == null)
        {
            
            return;
        }

        distance = Mathf.Infinity;
        for (int i = 0; i < popTarget.GetComponent<Population>().population.Count; i++)
        {
            float thisDist = Vector2.Distance(entity.transform.position, popTarget.GetComponent<Population>().population[i].transform.position);
            if (thisDist < 0)
            {
                thisDist *= -1;
            }

            if (thisDist < distance)
            {
                tempTarget = popTarget.GetComponent<Population>().population[i];
                distance = thisDist;
            }
        }

        targetCreature = tempTarget;
    }

    private void SelectMate(AIBehaviour entity)
    {
        if (entity.potentialMates.Count > 0)
        {
            float closestDist = Mathf.Infinity;
            GameObject closestMate = null;
            for (int i = 0; i < entity.potentialMates.Count; i++)
            {

                if (entity.potentialMates[i] == null)
                {
                    entity.potentialMates.RemoveAt(i);
                    continue;
                }

                GameObject thisMate = entity.potentialMates[i];
                float dist = Vector3.Distance(thisMate.transform.position, entity.transform.position) >= 0 ?
                             Vector3.Distance(thisMate.transform.position, entity.transform.position) :
                             (Vector3.Distance(thisMate.transform.position, entity.transform.position) * -1);

                if (dist < closestDist)
                {
                    //If there is a mate who was closer
                    if (closestMate != null)
                    {
                        //If this mate already has a mate assigned
                        if (closestMate.GetComponent<AIBehaviour>().targetMate != null)
                        {
                            //If the target mate used to be THIS mate then clear it (this mate "moved on") so they can mate with others, and 
                            //Assign the new closest mate details
                            if (closestMate.GetComponent<AIBehaviour>().targetMate.GetInstanceID() == entity.gameObject.GetInstanceID())
                            {
                                closestMate.GetComponent<AIBehaviour>().targetMate = null;
                                closestDist = dist;
                                closestMate = entity.potentialMates[i];
                                closestMate.GetComponent<AIBehaviour>().targetMate = entity.gameObject;
                                entity.targetMate = closestMate;
                            }
                            //Otherwise if the closestMate is pursuing a different creature remove them as a potential mate
                            else
                            {
                                entity.potentialMates.Remove(closestMate);
                            }
                        }
                        else
                        {
                            closestDist = dist;
                            closestMate = entity.potentialMates[i];
                            closestMate.GetComponent<AIBehaviour>().targetMate = entity.gameObject;
                            entity.targetMate = closestMate;
                        }
                    }
                    //If this is the first valid mate
                    else
                    { //If this mate already has a mate assigned
                        if (thisMate.GetComponent<AIBehaviour>().targetMate != null)
                        {
                            //If the target mate used to be THIS mate then clear it (this mate "moved on") so they can mate with others, and 
                            //Assign the new closest mate details
                            if (thisMate.GetComponent<AIBehaviour>().targetMate.GetInstanceID() == entity.gameObject.GetInstanceID())
                            {
                                closestDist = dist;
                                closestMate = thisMate;
                                closestMate.GetComponent<AIBehaviour>().targetMate = entity.gameObject;
                                entity.targetMate = closestMate;
                            }
                            //Otherwise if the closestMate is pursuing a different creature remove them as a potential mate
                            else
                            {
                                entity.potentialMates.Remove(closestMate);
                            }
                        }
                        else
                        {
                            closestDist = dist;
                            closestMate = thisMate;
                            closestMate.GetComponent<AIBehaviour>().targetMate = entity.gameObject;
                            entity.targetMate = closestMate;
                        }
                    }
                }
            }
        }
        //If there are no more potential mates, create a new list and try again
        else
        {
            entity.GenerateMateList();
        }
    }

    private void PerformCombat(AIBehaviour entity)
    {
        float rawDist = Vector3.Distance(entity.transform.position, targetCreature.transform.position);
        float dist = rawDist >= 0 ? rawDist : (rawDist * -1);

        if (!atTarget)
        {
            if (dist > 1.0f)
            {
                entity.GetComponent<AILerp>().enabled = true;
                MoveToTarget(entity, targetCreature.transform);
                atTarget = false;
                attack = false;
            }
            else
            {
                atTarget = true;
                targetCreature.GetComponent<AIBehaviour>().threatNearby = true;
                entity.GetComponent<Seeker>().path = null;
                entity.GetComponent<AILerp>().enabled = false;
                attack = true;
            }
        }
        else
        {
            if (attack)
            {
                targetCreature.GetComponent<AIBehaviour>().threatNearby = true;
                if (targetCreature.GetComponent<AIBehaviour>().targetCreature == null)
                {
                    targetCreature.GetComponent<AIBehaviour>().targetCreature = entity.gameObject;
                }
                //Force the target to enter Fight mode to fight back
                //Since everything moves at same speed (for now) no point adding flee in for this demo
                //Can set "Enemy Target", create a list, set the "ThreatNear" bool to true, etc...all of which the Attack.cs State can then use
                //Then Attack.cs would mostly consist of the below function
                AttackTarget(entity);
            }
        }
    }

    private void MoveHome(AIBehaviour entity)
    {

        float rawDist = Vector3.Distance(entity.transform.position, entity.GetComponent<CreatureManager>().population.transform.position);
        float dist = rawDist >= 0 ? rawDist : (rawDist * -1);

        if (!atHome)
        {
            if (dist > 1.0f)
            {
                entity.GetComponent<AILerp>().enabled = true;
                MoveToTarget(entity, entity.GetComponent<CreatureManager>().population.transform);
                atHome = false;
                attack = false;
            }
            else
            {
                atHome = true;
                entity.GetComponent<Seeker>().path = null;
                entity.GetComponent<AILerp>().enabled = false;
            }
        }
        else
        {
            StoreFood(entity);
        }
    }

    private void MoveToTarget(AIBehaviour entity, Transform target)
    {
        entity.GetComponent<AILerp>().target = target;
        ais3 = entity.GetComponent<AILerp>();
        ais3.SearchPath();
    }

    private void AttackTarget(AIBehaviour entity)
    {
        //Compare combat stats
        //Str + Const = HP
        //Str + int = damage
        //Const = Attack Speed

        float deltaT = Time.time - lastAttack;
        //Attack each second, but multiply damage output by DPS (att Speed)
        if (deltaT >= GlobalGEPSettings.ONE_SECOND)
        {
            lastAttack = Time.time;
            targetCreature.GetComponent<Stats>().currHP -= entity.GetComponent<Stats>().attDmg * entity.GetComponent<Stats>().attSpeed;
            if (targetCreature.GetComponent<Stats>().currHP <= 0)
            {
                //Could optionally force death immediately for accuracy (but checks once a second anyway)
                //Amount carried/obtained could be result of weight value if it was implemented in traits etc
                targetCreature.GetComponent<AIBehaviour>().CreatureMurdered(entity.GetComponent<CreatureManager>().population.populationIndex);
                foodCarried = 1;
                foodCarried *= 1 + (Mathf.RoundToInt(entity.attributes["intellect"]) * 2);
                moveHome = true;
                attack = false;
            }
        }

    }

    private void StoreFood(AIBehaviour entity)
    {
        entity.GetComponent<CreatureManager>().population.popFoodStores.SupplyResource(entity.gameObject, foodCarried * 10);
        entity.ChangeState(new Idle(), AIBehaviour.BEHAVIOUR_STATE.IDLE);
        entity.GetComponent<CreatureManager>().population.hunters--;
        entity.isHunter = false;
        moveHome = false;
        atTarget = false;
        atHome = false;
        attack = false;
        moving = false;
    }
}
