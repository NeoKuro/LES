using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Attack : FSMState<AIBehaviour>
{
    public bool atTarget = false;
    public bool attack = false;
    public bool moving = false;

    private float lastAttack = 0.0f;
    private int foodCarried = 0;

    private GameObject targetCreature = null;

    private AILerp ais3;

    public Attack()
    {

    }

    public override void Enter(AIBehaviour entity)
    {
        //Debug.Log(entity.gameObject.name + ": Snoo Snoo");
        //Eventually replace with a graphical text bubble "Zzz"
        entity.GetComponent<AILerp>().enabled = true;
        SelectTarget(entity.targetCreature);
    }

    public override void Execute(AIBehaviour entity)
    {
        //Check there is currently a target (IE hasn't been killed/deleted)
        if (targetCreature == null)
        {
            SelectTarget(entity.targetCreature);
            if(targetCreature == null)
            {
                entity.ChangeState(new Idle(), AIBehaviour.BEHAVIOUR_STATE.IDLE);
            }
        }
        else
        {
                //If a mate is chosen, check they are ready to mate, if so then move to them, then move towards the Population Centre
                PerformCombat(entity);
        }
    }

    public override void Exit(AIBehaviour entity)
    {
        //Debug.Log(entity.gameObject.name + " woke up!");
        //Eventually replace with a graphical text bubble "!"
    }

    private void SelectTarget(GameObject newTarget)
    {
        //This one will just select the target based on who is attacking

        if(targetCreature == null)
        {
            //assign new target
            targetCreature = newTarget;
            return;
        }

        if(targetCreature.GetComponent<Stats>().currHP <= 0)
        {
            targetCreature = null;  //If killed remove target
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
                entity.GetComponent<Seeker>().path = null;
                entity.GetComponent<AILerp>().enabled = false;
                attack = true;
            }
        }
        else
        {
            if (attack)
            {
                //Force the target to enter Fight mode to fight back
                //Since everything moves at same speed (for now) no point adding flee in for this demo
                //Can set "Enemy Target", create a list, set the "ThreatNear" bool to true, etc...all of which the Attack.cs State can then use
                //Then Attack.cs would mostly consist of the below function
                AttackTarget(entity);
            }
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
        if (deltaT >= GlobalGEPSettings.ONE_SECOND)
        {
            lastAttack = Time.time;
            targetCreature.GetComponent<Stats>().currHP -= entity.GetComponent<Stats>().attDmg * entity.GetComponent<Stats>().attSpeed;
            if (targetCreature.GetComponent<Stats>().currHP <= 0)
            {
                //Could optionally force death immediately for accuracy (but checks once a second anyway)
                //Could also optionally allow creatures defending themselves to take any foodback with them
                //But if they are under attack from multiple creatures would have to handle that
                attack = false;
                targetCreature.GetComponent<AIBehaviour>().CreatureMurdered(entity.GetComponent<CreatureManager>().population.populationIndex);
                entity.ChangeState(new Idle(), AIBehaviour.BEHAVIOUR_STATE.IDLE);
            }
        }
    }
}
