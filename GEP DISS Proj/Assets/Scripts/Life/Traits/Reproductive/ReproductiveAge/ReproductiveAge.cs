using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReproductiveAge
{
    //Reproductive age deterimines the minimum age for an individual to start breeding
    //On 4 genes
    //multiply the RepAge value with the Lifespan age. 
    //      The resulting value can then either be;
    //          A) Subtracted from the lifespan (IE 20 Life span * 0.3 Rep age = 6. Actual Rep Age = 20 - 6 = 14) where higher values are better (pos correlation)
    //          B) Left as is, where the resulting value = age at which they become fertile etc
    // If 0 or negative numbers obtained, then considered infertile?

    //Standard Public
    public float reprodAge;

    //Standard Private


    public ReproductiveAge(List<Trait> reproductiveAge)
    {
        EvaluateReproductiveAge(reproductiveAge);
    }

    private void EvaluateReproductiveAge(List<Trait> reproductiveAge)
    {
        float sum = 0.0f;
        for (int i = 0; i < reproductiveAge.Count; i++)
        {
            sum += reproductiveAge[i].numericValue;
        }
        float average = sum / reproductiveAge.Count;
        float variance = 0f;
        List<float> differences = new List<float>();
        for (int i = 0; i < reproductiveAge.Count; i++)
        {
            float var = reproductiveAge[i].numericValue;

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


        float reprodAgeEqn = (Mathf.Sin(average)) * (Mathf.Sin(angle));

        if(reprodAgeEqn > 1)
        {
            Debug.Log("> 1 : " + reprodAgeEqn);
        }

        reprodAge = reprodAgeEqn;

        //Debug.Log("Total Rep Age: " + reprodAge + "     PreEqn: " + Mathf.Atan(angle) * 10 + "      avg: " + average * 2);
        //if (growthRate <= 0)
        //Debug.Log("Dead: " + growthRate);
    }
}
