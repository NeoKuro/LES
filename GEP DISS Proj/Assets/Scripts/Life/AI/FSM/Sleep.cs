using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Sleep : FSMState<AIBehaviour>
{
    public Sleep()
    {

    }
    
    public override void Enter(AIBehaviour entity)
    {
        //Debug.Log(entity.gameObject.name + ": Zzz");
        //Eventually replace with a graphical text bubble "Zzz"
        entity.GetComponent<AILerp>().enabled = false;
    }

    public override void Execute(AIBehaviour entity)
    {
        //Awareness of surroundings only includes sounds very close by
        //As such any danger will not be aware unless danger is close by, and will sleep through it
        //Recover energy
        float currentLevel = entity.traits["Energy Level"];
        float currentCons = entity.traits["Energy Consumption"];

        //Increase by POPULATION's Safety + Intellect (better shelter/beds)
        //Must be in the radius of a bed/shelter unless passed out - but this will be handled in the AI Behaviour
        currentLevel += ((currentCons * 10) * Time.deltaTime);
        entity.traits["Energy Level"] = currentLevel;

        //Finished replenishing Energy. So wake up automatically
        if (currentLevel >= entity.capacityTraits["Energy Level"])
        {
            entity.traits["Energy Level"] = entity.capacityTraits["Energy Level"];
            entity.ChangeState(new Idle(), AIBehaviour.BEHAVIOUR_STATE.IDLE);
        }
    }

    public override void Exit(AIBehaviour entity)
    {
        //Debug.Log(entity.gameObject.name + " woke up!");
        //Eventually replace with a graphical text bubble "!"
    }
}
