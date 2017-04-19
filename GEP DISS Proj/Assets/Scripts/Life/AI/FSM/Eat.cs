using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Eat : FSMState<AIBehaviour>
{
    private bool startEating = false;
    private bool moving = false;
    private AILerp ais3;

    public Eat()
    {

    }

    public override void Enter(AIBehaviour entity)
    {
        //Debug.Log(entity.gameObject.name + ": Nom nom nom");
        //Eventually replace with a graphical text bubble "Zzz"
        entity.GetComponent<AILerp>().enabled = false;
    }

    public override void Execute(AIBehaviour entity)
    {
        //Will need to access the food stores from the POPULATION class (to be built) 
        //Consume food until reaching the minimum "happy" level (75%) or until food runs out
        //If food runs out either immediately enter Idle state, or force to do a priority 
        //Recover Food

        //Check the distance and move there if out of range
        float distance = Vector2.Distance(entity.transform.position, entity.GetComponent<CreatureManager>().population.transform.position);
        if (distance < 0)
        {
            distance *= -1;
        }

        if (distance > 2.0f)
        {
            startEating = false;
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
            startEating = true;
            moving = false;
        }

        if (startEating)
        {
            float currentLevel = entity.traits["Food Level"];
            float currentCons = entity.traits["Food Consumption"];

            //Increase by a number that resembles the POPULATION's food quality
            //Improved with higher intellect, and how long food has been there (on average)
            float foodVal = 0.0f;

            if (entity.GetComponent<CreatureManager>().population.dietType == Population.DIET_TYPE.HERBIVORE)
            {
                //If herbivore then just eat off ground over time
                //In future could implement fruit/veg gathering mechanics for herbivores etc
                //Faster recovery rate
                foodVal = 2 * ((1 * entity.GetComponent<Stats>().traits["Food Consumption"]) * 20) * (Time.deltaTime * 4);
            }
            else if (entity.GetComponent<CreatureManager>().population.popFoodStores.currentAmount > 0.0f)
            {
                foodVal = entity.GetComponent<CreatureManager>().population.popFoodStores.ConsumeResource(entity.gameObject);
            }

            currentLevel += foodVal;
            entity.traits["Food Level"] = currentLevel;

            if (currentLevel >= entity.capacityTraits["Food Level"])
            {
                entity.traits["Food Level"] = entity.capacityTraits["Food Level"];
                entity.ChangeState(new Idle(), AIBehaviour.BEHAVIOUR_STATE.IDLE);
            }
        }
    }

    public override void Exit(AIBehaviour entity)
    {
        //Debug.Log(entity.gameObject.name + " full up!");
        //Eventually replace with a graphical text bubble "!"
    }

    private void MoveToWaterSource(AIBehaviour entity)
    {
        entity.GetComponent<AILerp>().target = entity.GetComponent<CreatureManager>().population.transform;
        ais3 = entity.GetComponent<AILerp>();
        ais3.SearchPath();
        moving = true;
    }
}
