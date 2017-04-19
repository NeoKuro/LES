using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyConsumption
{

    //Standard Public
    public float energyConsumptionVal;

    //Standard Private


    public EnergyConsumption(List<Trait> energyConsumption)
    {
        EvaluateEnergyConsumption(energyConsumption);
    }

    private void EvaluateEnergyConsumption(List<Trait> energyConsumption)
    {
        float sum = 0.0f;
        for (int i = 0; i < energyConsumption.Count; i++)
        {
            sum += energyConsumption[i].numericValue;
        }
        float average = sum / energyConsumption.Count;
        float variance = 0f;
        List<float> differences = new List<float>();
        for (int i = 0; i < energyConsumption.Count; i++)
        {
            float var = energyConsumption[i].numericValue;
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

        float energyConsumptionEqn = (Mathf.Atan(angle) / 10) + (sin / 2);
        energyConsumptionVal = energyConsumptionEqn / 2;

        if (energyConsumptionVal < 0)
        {
            energyConsumptionVal *= -1;
        }

        if (energyConsumptionVal < 0.05)
        {
            if (energyConsumptionVal > 0.0075)
                energyConsumptionVal += 1;
            //For extremely low values, kill them
            else
                energyConsumptionVal = 0;
        }

        //Debug.Log("Total Energy Consumption: " + energyConsumptionVal + "     PreEqn: " + angle + "      sin: " + sin / 4);
        //if((energyConsumptionVal >= 0.25f && energyConsumptionVal > 0.075f )&& energyConsumptionVal != 0)
        //    Debug.Log("SUPER ENERGY: " + energyConsumptionVal);
    }
}
