using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyLevel {

    //Determines how much an individual has. Will be depleted by "EnergyConsumption" and when depleted must rest/sleep. 
    //If hungry/Thirsty or in dangerous area and falls asleep (passes out etc) then can be at risk of death
    //When hits 0, individual passes out
    //Default = ~10
    //Low: ~5, High: 23+
    //High values can also increase speed at which actions are completed (IE walk faster)
    //Consumption will be updated every FIXED UPDATE (0.02s) 
    //On 3 genes

    //Standard Public
    public float energyLevelVal;

    //Standard Private


    public EnergyLevel(List<Trait> energyLevel)
    {
        EvaluateEnergyLevels(energyLevel);
    }

    private void EvaluateEnergyLevels(List<Trait> energyLevel)
    {
        float sum = 0.0f;
        for (int i = 0; i < energyLevel.Count; i++)
        {
            sum += energyLevel[i].numericValue;
        }
        float average = sum / energyLevel.Count;
        float variance = 0f;
        List<float> differences = new List<float>();
        for (int i = 0; i < energyLevel.Count; i++)
        {
            float var = energyLevel[i].numericValue;

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

        float energyLevelEqn = Mathf.Sqrt(average * 10) +  (Mathf.Atan(angle));
        energyLevelVal = (energyLevelEqn * 2) + GlobalGEPSettings.MIN_ENERGY_LEVEL;

        //Debug.Log("Total Energy: " + energyLevelVal + "     PreEqn: " + Mathf.Atan(angle) * 10 + "      avg: " + average * 2);
        //if (energyLevelVal >= 7)
       //     Debug.Log("SUPER ENERGY: " + energyLevelVal);
    }
}
