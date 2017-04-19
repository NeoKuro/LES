using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterConsumption {


    //Standard Public
    public float waterConsumptionVal;

    //Standard Private


    public WaterConsumption(List<Trait> waterConsumption)
    {
        EvaluateWaterConsumption(waterConsumption);
    }

    private void EvaluateWaterConsumption(List<Trait> waterConsumption)
    {
        float sum = 0.0f;
        for (int i = 0; i < waterConsumption.Count; i++)
        {
            sum += waterConsumption[i].numericValue;
        }
        float average = sum / waterConsumption.Count;
        float variance = 0f;
        List<float> differences = new List<float>();
        for (int i = 0; i < waterConsumption.Count; i++)
        {
            float var = waterConsumption[i].numericValue;
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
        float waterConsumptionPreEqn = Mathf.Sqrt(sd);

        float angle = Mathf.Deg2Rad * waterConsumptionPreEqn;

        float sin = Mathf.Sin(average);

        if (sin < 0)
        {
            sin *= -1;
        }

        float waterConsumptionEqn = (Mathf.Atan(angle) / 10) + (sin / 2);
        waterConsumptionVal = waterConsumptionEqn / 2;

        if (waterConsumptionVal < 0)
        {
            waterConsumptionVal *= -1;
        }

        if (waterConsumptionVal < 0.05)
        {
            if (waterConsumptionVal > 0.0075)
                waterConsumptionVal += 1;
            else
                waterConsumptionVal = 0;
        }

        //Debug.Log("Total Water Consumption: " + waterConsumptionVal + "     PreEqn: " + angle + "      sin: " + sin / 4);
        //if ((waterConsumptionVal >= 0.25f && waterConsumptionVal > 0.075f) && waterConsumptionVal != 0)
       //     Debug.Log("SUPER ENERGY: " + waterConsumptionVal);
    }
}
