using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceStores
{
    //Standard Public
    public int resourceAmount = 0;          //How much food this population has - Int not float, makes it more finite, multiplied based on their intellect etc
    public int resourceCapacity = 10;       //Maximum amount of food population can store
    public float resourceQuality = 1.0f;    //Quality of the food. Higher quality = provides more food level when consumed
    public float currentAmount = 0.0f;      //Percentage of the filled capacity

    //Standard Private
    private string storeType = "";

    //Custom Private
    private Population thisPopulation;

    public void InitialSetup(Population newPop, string attribute)
    {
        thisPopulation = newPop;
        if (attribute.ToLower().Contains("food"))
        {
            float tAmount = GlobalGEPSettings.STARTING_FOOD_AMOUNT;
            resourceQuality = GlobalGEPSettings.START_FOOD_QUALITY;
            resourceCapacity = thisPopulation.population.Count * GlobalGEPSettings.RESOURCE_CAPACITY_MULT;      //eventually go based off of 'Radius' or size of population
                                                                                                            //Eventually radius will increase with pop'n/exploration etc
            resourceAmount = (int)(resourceCapacity * tAmount);
        }

        currentAmount = (resourceAmount / (float)resourceCapacity);
        storeType = attribute;
    }

    //Remove resource from storage
    public float ConsumeResource(GameObject creature)
    {
        if (resourceAmount > 0)
        {
            resourceAmount--;
            //Food to be added to the creature's food stores.
            //Result of 1 food amount (foodAmount--) multiplied by;
            //          FoodQuality multiplied by the consumption rate of the creature
            //          Multiplied by 10.
            //Means creatures are not waiting around for too long to feed themselves.
            //Will have to prioritize hunger (IE if starving, and low food, will eat. If above 50% and low food won't eat)
            currentAmount = (resourceAmount / (float)resourceCapacity);
            return 1 * ((resourceQuality * creature.GetComponent<Stats>().traits[storeType + " Consumption"]) * 20);
        }

        return 0.0f;
    }

    //Add resource to stores
    public void SupplyResource(GameObject creature, int resourceToAdd)
    {
        //Even if the resource is at capacity, modify quality anyway
        CalculateQuality(creature);
        resourceAmount += resourceToAdd;

        if (resourceAmount > resourceCapacity)
        {
            resourceAmount = resourceCapacity;
        }
        currentAmount = (resourceAmount / (float)resourceCapacity);
    }

    //Add resource to stores
    public void SupplyResource(int resourceToAdd)
    {
        //Even if the resource is at capacity, modify quality anyway
        resourceAmount += resourceToAdd;

        if (resourceAmount > resourceCapacity)
        {
            resourceAmount = resourceCapacity;
        }
        currentAmount = (resourceAmount / (float)resourceCapacity);
    }



    //Check the current food levels, vs how hungry creature is
    public bool CheckPriority(GameObject creature)
    {
        //If food levels are "Low"
        if (currentAmount <= 0.5f)
        {
            float currentResourceLevel = creature.GetComponent<Stats>().traits[storeType + " Levels"];
            float resourcePriorityMargin = creature.GetComponent<AIBehaviour>().capacityTraits[storeType + " Levels"] * GlobalGEPSettings.PRIOTIY_MARGIN;
            //If creature food levels are below priority threshold then signal they are a priority
            if (currentResourceLevel <= resourcePriorityMargin)
            {
                return true;
            }
            //Otherwise they are not a priority
            return false;
        }
        //Otherwise if food levels are not low, let them eat anyway
        return true;
    }

    private void CalculateQuality(GameObject creature)
    {
        float intellectDifference = creature.GetComponent<Stats>().attributes["intellect"] - thisPopulation.averageIntellect;

        resourceQuality += intellectDifference / 100;   //Increases/Decreases based on how better/worse a creature is, as a percentage        
    }

}
