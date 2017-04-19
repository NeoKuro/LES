using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodConsumption
{


    //Standard Public
    public float foodConsumptionVal;

    //Standard Private


    public FoodConsumption(List<Trait> foodConsumption)
    {
        EvaluateFoodConsumption(foodConsumption);
    }

    private void EvaluateFoodConsumption(List<Trait> foodConsumption)
    {
        float sum = 0.0f;
        for (int i = 0; i < foodConsumption.Count; i++)
        {
            sum += foodConsumption[i].numericValue;
        }
        float average = sum / foodConsumption.Count;
        float variance = 0f;
        List<float> differences = new List<float>();
        for (int i = 0; i < foodConsumption.Count; i++)
        {
            float var = foodConsumption[i].numericValue;
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
        float energyConsumptionPreEqn = Mathf.Sqrt(sd);

        float angle = Mathf.Deg2Rad * energyConsumptionPreEqn;

        float sin = Mathf.Sin(average);

        if (sin < 0)
        {
            sin *= -1;
        }

        float foodConsumptionEqn = (Mathf.Atan(angle) / 10) + (sin / 2);
        foodConsumptionVal = foodConsumptionEqn / 2;

        if (foodConsumptionVal < 0)
        {
            foodConsumptionVal *= -1;
        }

        if (foodConsumptionVal < 0.05)
        {
            if (foodConsumptionVal > 0.0075)
                foodConsumptionVal += 1;
            else
                foodConsumptionVal = 0;
        }

        //Debug.Log("Total Food Consumption: " + foodConsumptionEqn + "     PreEqn: " + angle + "      sin: " + sin / 4);
        //if ((foodConsumptionVal >= 0.25f && foodConsumptionVal > 0.075f) && foodConsumptionVal != 0)
            //Debug.Log("SUPER ENERGY: " + foodConsumptionVal);
    }
}
