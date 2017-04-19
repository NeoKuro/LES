using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Drink : FSMState<AIBehaviour>
{
    private bool moveToWater = false;
    private bool moving = false;

    private GameObject waterHole = null;
    private AILerp ais3;

    public Drink()
    {

    }

    public override void Enter(AIBehaviour entity)
    {
        //Debug.Log(entity.gameObject.name + ": Glub glub glub");
        //Eventually replace with a graphical text bubble "Zzz"
        entity.GetComponent<AILerp>().enabled = false;

        if (entity.targetWaterSource == null)
        {
            entity.FindWaterSources();
        }

        waterHole = entity.targetWaterSource;
    }

    public override void Execute(AIBehaviour entity)
    {
        //If not will go to nearest water source and begin drinking
        //Replenishing supplies (Food + water) will happen in another state either "Explore" or perhaps "Gather"
        //Drink water

        //Check if there is any water stored in the Population centre
        //If there is then set the target water hole to be the population centre
        //If not carry on with the closest water source
        if (entity.GetComponent<CreatureManager>().population.popWaterStores.resourceAmount > 0)
        {
            waterHole = entity.GetComponent<CreatureManager>().population.gameObject;
        }

        //Check the distance and move there if out of range
        float distance = Vector2.Distance(entity.transform.position, waterHole.transform.position);
        if (distance < 0)
        {
            distance *= -1;
        }

        if (distance > 1.0f)
        {
            moveToWater = true;
            entity.GetComponent<AILerp>().enabled = true;
            entity.GetComponent<AILerp>().speed = 5;
            if (!moving)
            {
                MoveToWaterSource(entity);
            }
        }
        else
        {
            entity.GetComponent<AILerp>().enabled = false;
            entity.GetComponent<AILerp>().speed = 4;
            moveToWater = false;
            moving = false;
        }


        //If in range consume water (eitehr from population or source)
        if (!moveToWater)
        {
            float currentLevel = entity.traits["Water Level"];
            float currentCons = entity.traits["Water Consumption"];

            //Increase by POPULATIONS current water quality level. Determined by Intellect + distance to water sources
            //Does not take Time.deltaTime into account - will (probably) completely fill up individual(s) 
            //      This is because it takes 1 resource rather than a fraction/float

            //Default value (incase no water stored in population - default uses lower quality (0.5f) and time.deltaTime
            float drinkVal = 0.25f * (entity.traits["Water Consumption"] * 10);
            if (entity.GetComponent<CreatureManager>().population.popWaterStores.currentAmount > 0.0f)
            {
                drinkVal = entity.GetComponent<CreatureManager>().population.popWaterStores.ConsumeResource(entity.gameObject);
            }
            currentLevel += drinkVal;
            entity.traits["Water Level"] = currentLevel;

            if (currentLevel >= entity.capacityTraits["Water Level"])
            {
                entity.traits["Water Level"] = entity.capacityTraits["Water Level"];
                entity.ChangeState(new Idle(), AIBehaviour.BEHAVIOUR_STATE.IDLE);
            }
        }
    }

    public override void Exit(AIBehaviour entity)
    {
        //Debug.Log(entity.gameObject.name + ": Not Thirsty!");
        //Eventually replace with a graphical text bubble "!"
    }

    private void MoveToWaterSource(AIBehaviour entity)
    {
        entity.GetComponent<AILerp>().target = waterHole.transform;
        ais3 = entity.GetComponent<AILerp>();
        ais3.SearchPath();
        moving = true;
    }
}
