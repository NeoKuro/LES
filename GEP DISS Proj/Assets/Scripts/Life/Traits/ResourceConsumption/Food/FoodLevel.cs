using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodLevel {

    //Determines how much food an individual can "store" before they get hungry again
    //When hits 0 individual starts taking damage
    //      Alternatively have a ((-food) * 0.5) as the min limit. If hits this number then the individual dies
    //When hits 0, individual passes out
    //Default = ~10
    //Consumption will be updated every FIXED UPDATE (0.02s) 
    //On 3 genes

    //Standard Public
    public float foodLevelVal;

    //Standard Private


    public FoodLevel(List<Trait> foodLevel)
    {
        EvaluateFoodLevels(foodLevel);
    }

    private void EvaluateFoodLevels(List<Trait> foodLevel)
    {
        float sum = 0.0f;
        for (int i = 0; i < foodLevel.Count; i++)
        {
            sum += foodLevel[i].numericValue;
        }
        float average = sum / foodLevel.Count;
        float variance = 0f;
        List<float> differences = new List<float>();
        for (int i = 0; i < foodLevel.Count; i++)
        {
            float var = foodLevel[i].numericValue;

            var = var - average;
            var *= var;
            differences.Add(var);
        }

        for (int i = 0; i < differences.Count; i++)
        {
            variance += differences[i];
        }

        float sd = variance / differences.Count;

        //Change this equation for different results
        float standardDeviation = Mathf.Sqrt(sd);

        float angle = standardDeviation;

        if (average < 0)
        {
            average *= -1;
        }

        float foodLevelEqn = Mathf.Sqrt(average * 10) + (Mathf.Atan(angle));
        foodLevelVal = (foodLevelEqn * 2) + GlobalGEPSettings.MIN_FOOD_LEVEL;

        //Debug.Log("Total Food: " + foodLevelVal + "     PreEqn: " + Mathf.Atan(angle) * 10 + "      avg: " + average * 2);
        //if (energyLevelVal >= 7)
        //     Debug.Log("SUPER ENERGY: " + energyLevelVal);
    }
}
