using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestationPeriod {
    //Gestation period determines how long a new individual will take to be 'born' (or hatch)
    //Number represents how many "Years" it will take (1.0 = 1 years, 0.5 = 0.5 years etc)
    //<0 = Infertile (never have offspring)
    //Extremely high numbers = harder to have offspring
    //Currently ~10% population infertile
    //On average values range 0.6 - 0.9
    //Lowest < 0
    //Highest 1.6...
    //On 2 genes

    //Standard Public
    public float gestationPeriodVal;

    //Standard Private


    public GestationPeriod(List<Trait> gestationPeriodTraits)
    {
        EvaluateGestationPeriod(gestationPeriodTraits);
    }

    private void EvaluateGestationPeriod(List<Trait> gestationPeriodTraits)
    {
        float sum = 0.0f;
        for (int i = 0; i < gestationPeriodTraits.Count; i++)
        {
            sum += gestationPeriodTraits[i].numericValue;
        }
        float average = sum / gestationPeriodTraits.Count;
        float variance = 0f;
        List<float> differences = new List<float>();
        for (int i = 0; i < gestationPeriodTraits.Count; i++)
        {
            float var = gestationPeriodTraits[i].numericValue;

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

        float gestationPeriodEqn = average / 4 + (Mathf.Sin(angle));

        if(gestationPeriodEqn < 0)
        {
            gestationPeriodEqn *= -1;
            gestationPeriodEqn += 1;        //make sure is positive, but give penalty if was negative
        }

        gestationPeriodVal = gestationPeriodEqn;

        //Debug.Log("Total Gestation Period: " + gestationPeriodVal + "     PreEqn: " + standardDeviation + "      avg: " + average * 2);
    }
}
