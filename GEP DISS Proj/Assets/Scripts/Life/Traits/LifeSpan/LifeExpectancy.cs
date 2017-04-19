using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeExpectancy
{
    //Life Expectancy determines the max age of an individual
    //On 4 genes

    //Standard Public
    public float lifeExpectancyVal;

    //Standard Private


    public LifeExpectancy(List<Trait> lifeExpectancy)
    {
        EvaluateGrowthRate(lifeExpectancy);
    }

    private void EvaluateGrowthRate(List<Trait> lifeExectancy)
    {
        float sum = 0.0f;
        for (int i = 0; i < lifeExectancy.Count; i++)
        {
            sum += lifeExectancy[i].numericValue;
        }
        float average = sum / lifeExectancy.Count;
        float variance = 0f;
        List<float> differences = new List<float>();
        for (int i = 0; i < lifeExectancy.Count; i++)
        {
            float var = lifeExectancy[i].numericValue;

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

        if(average < 0)
        {
            average *= -1;
        }

        float lifeExpectancyEqn = average * 2 + (Mathf.Atan(angle) * 10);
        lifeExpectancyVal = lifeExpectancyEqn;

        //Debug.Log("Total Life: " + lifeExpectancyVal + "     PreEqn: " + Mathf.Atan(angle) * 10 + "      avg: " + average * 2);
        //if (growthRate <= 0)
            //Debug.Log("Dead: " + growthRate);
    }
}
