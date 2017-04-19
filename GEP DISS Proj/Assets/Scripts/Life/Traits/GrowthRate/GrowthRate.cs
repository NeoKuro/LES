using System.Collections.Generic;
using UnityEngine;

public class GrowthRate
{

    //Standard Public
    public float growthRate;


    public GrowthRate(List<Trait> growthRate)
    {
        EvaluateGrowthRate(growthRate);
    }

    private void EvaluateGrowthRate(List<Trait> growthRateTraits)
    {
        float sum = 0.0f;

        for(int i = 0; i < growthRateTraits.Count; i++)
        {
            sum += growthRateTraits[i].numericValue;
        }

        float average = sum / growthRateTraits.Count;
        float variance = 0f;
        List<float> differences = new List<float>();

        for(int i = 0; i < growthRateTraits.Count; i++)
        {
            float var = growthRateTraits[i].numericValue;
            var = var - average;
            var *= var;
            differences.Add(var);
        }

        for(int i = 0; i < differences.Count; i++)
        {
            variance += differences[i];
        }

        float standardDeviation = variance / differences.Count;     
        float growthRateEqnPre = Mathf.Sqrt(standardDeviation);
        float angle = Mathf.Deg2Rad * growthRateEqnPre;
        float growthRateEqn = average / 4 + (Mathf.Sin(angle) * 10);

        growthRate = growthRateEqn;
    }
}
