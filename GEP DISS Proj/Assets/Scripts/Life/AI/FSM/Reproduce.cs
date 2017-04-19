using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Reproduce : FSMState<AIBehaviour>
{
    public bool moveToCentre = true;
    public bool ready = false;
    public bool moving = false;
    private AILerp ais3;
    private Idle waitForPartner = new Idle();

    public Reproduce()
    {

    }

    public override void Enter(AIBehaviour entity)
    {
        //Debug.Log(entity.gameObject.name + ": Snoo Snoo");
        //Eventually replace with a graphical text bubble "Zzz"
        entity.GetComponent<AILerp>().enabled = true;
    }

    public override void Execute(AIBehaviour entity)
    {
        //Steps to Reproduce:
        //1. Select the nearest mate
        //2. Check if mate is already taken (is "targetMate" null?) if not set this creature as their target mate (and vice versa)
        //3. On entering "Reproduce" state, check if targetMate is null. if so go to Step 1. Otherwise proceed to step 4
        //4. Both creatures move within their Pop'n centre radius, and towards each other.
        //5. Sexy Time (share DNA - Create offspring off screen. DO NOT USE START() FUNCTION IN HUMANMANAGER. 
        //                                                      Will need to change this so that the correct function has to be called so DNA is combined
        //                                                      Rather than randomly created as is the case with the initial pop.

        //Try to select a mate by going through the list of potential mates.
        //If none are found, force the AIBehaviour to make a new list then try again next execution
        if (entity.targetMate == null)
        {
            SelectMate(entity);
        }
        else
        {
            if (!ready)
            {
                //If a mate is chosen, check they are ready to mate, if so then move to them, then move towards the Population Centre
                PerformMovements(entity);
            }
            else
            {
                entity.Reproduce();
                ready = false;
                moveToCentre = true;
                moving = false;

                //Reproduce here
                //Set a "Cooldown" timer here also
                /*
                entity.currentState = AIBehaviour.BEHAVIOUR_STATE.IDLE;
                entity.targetMate.GetComponent<AIBehaviour>().currentState = AIBehaviour.BEHAVIOUR_STATE.IDLE;
                */
            }
        }
    }

    public override void Exit(AIBehaviour entity)
    {
        //Debug.Log(entity.gameObject.name + " woke up!");
        //Eventually replace with a graphical text bubble "!"
    }

    private void SelectMate(AIBehaviour entity)
    {
        //Pick the best mate
        //Index 0 will be the best available
        for(int i = 0; i < entity.potentialMates.Count; i++)
        {
            //If empty (mate died, is taken, etc) then pass over
            if(entity.potentialMates[i] == null)
            {
                continue;
            }

            //If this mate is still available add them
            if(entity.potentialMates[i].GetComponent<AIBehaviour>().targetMate == null)
            {
                entity.targetMate = entity.potentialMates[i];
                entity.potentialMates[i].GetComponent<AIBehaviour>().targetMate = entity.gameObject;
                return;
            }
        }

        //If unable to find a suitable mate (IE all taken) then regenerate the list and try again
        if(entity.targetMate == null)
        {
            entity.potentialMates.Clear();
            entity.GenerateMateList();
        }

        //If no suitable mates, then come out of Reproduce state
        if (entity.potentialMates.Count == 0)
        {
            entity.ChangeState(new Idle(), AIBehaviour.BEHAVIOUR_STATE.IDLE);
            entity.rep_Cooldown = true;
        }

        /*
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
        }*/
    }

    private void PerformMovements(AIBehaviour entity)
    {
        if (entity.targetMate == null || entity.targetMate.GetComponent<AIBehaviour>() == null)
        {
            entity.GenerateMateList();
            return;
        }

        if (entity.targetMate.GetComponent<AIBehaviour>().targetMate != null &&
            entity.targetMate.GetComponent<AIBehaviour>().targetMate.GetInstanceID() != entity.gameObject.GetInstanceID())
        {
            entity.ChangeState(new Idle(), AIBehaviour.BEHAVIOUR_STATE.IDLE);
            entity.targetMate = null;
            entity.GenerateMateList();
            return;
        }

        /*
        if (entity.targetMate.GetComponent<AIBehaviour>().currentState != AIBehaviour.BEHAVIOUR_STATE.REPRODUCE &&
            entity.targetMate.GetComponent<AIBehaviour>().currentState != AIBehaviour.BEHAVIOUR_STATE.ENGAGED)
        {
            entity.ChangeState(new Idle(), AIBehaviour.BEHAVIOUR_STATE.IDLE);
            return;
        }
        */

        if (!entity.targetMate.GetComponent<AIBehaviour>().InTheMood())
        {
            entity.ChangeState(new Idle(), AIBehaviour.BEHAVIOUR_STATE.IDLE);
            return;
        }

        float rawDist = Vector3.Distance(entity.targetMate.transform.position, entity.transform.position);
        float dist = rawDist >= 0 ? rawDist : (rawDist * -1);

        if (!moveToCentre)
        {
            if (dist > 1.0f)
            {
                //if (entity.currentState != AIBehaviour.BEHAVIOUR_STATE.ENGAGED)
                MoveToTarget(entity, entity.targetMate.transform);
                entity.GetComponent<AILerp>().enabled = true;
                moveToCentre = false;
                ready = false;
            }
            else
            {
                moveToCentre = true;
                entity.GetComponent<Seeker>().path = null;
                entity.GetComponent<AILerp>().enabled = false;
                //MoveToTarget(entity, null);
            }
        }
        else if (moveToCentre)
        {
            rawDist = Vector3.Distance(entity.GetComponent<CreatureManager>().population.transform.position, entity.transform.position);
            dist = rawDist >= 0 ? rawDist : (rawDist * -1);

            if (dist > (entity.GetComponent<CreatureManager>().population.currentRadius * 0.5f))
            {
                if (!moving)
                {
                    ready = false;
                    entity.GetComponent<AILerp>().enabled = true;
                    MoveToTarget(entity, entity.GetComponent<CreatureManager>().population.transform);
                    moving = true;
                }
            }
            else
            {
                ready = true;
                //Is now in position, remove the target + proceed
                entity.GetComponent<Seeker>().path = null;
                entity.GetComponent<AILerp>().enabled = false;
                //MoveToTarget(entity, null);
            }
        }
    }

    private void MoveToTarget(AIBehaviour entity, Transform target)
    {
        entity.GetComponent<AILerp>().target = target;

        ais3 = entity.GetComponent<AILerp>();
        ais3.SearchPath();
        entity.currentState = AIBehaviour.BEHAVIOUR_STATE.ENGAGED;
    }

}
