using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterLevel
{

    //Determines how much water an individual can store before they become dehydrated
    //      See FoodLevel for notes (will work the same/similar)
    //When hits 0, individual passes out
    //Default = ~10
    //Low: ~5, High: 23+
    //High values can also increase speed at which actions are completed (IE walk faster)
    //Consumption will be updated every FIXED UPDATE (0.02s) 
    //On 3 genes

    //Standard Public
    public float waterLevelVal;

    //Standard Private


    public WaterLevel(List<Trait> waterLevel)
    {
        EvaluateWaterLevels(waterLevel);
    }

    private void EvaluateWaterLevels(List<Trait> waterLevel)
    {
        float sum = 0.0f;
        for (int i = 0; i < waterLevel.Count; i++)
        {
            sum += waterLevel[i].numericValue;
        }
        float average = sum / waterLevel.Count;
        float variance = 0f;
        List<float> differences = new List<float>();
        for (int i = 0; i < waterLevel.Count; i++)
        {
            float var = waterLevel[i].numericValue;

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

        float waterLevelEqn = Mathf.Sqrt(average * 10) + (Mathf.Atan(angle));
        waterLevelVal = (waterLevelEqn * 2) + GlobalGEPSettings.MIN_WATER_LEVEL;

        //Debug.Log("Total Water: " + waterLevelVal + "     PreEqn: " + Mathf.Atan(angle) * 10 + "      avg: " + average * 2);
        //if (energyLevelVal >= 7)
        //     Debug.Log("SUPER ENERGY: " + energyLevelVal);
    }
}
