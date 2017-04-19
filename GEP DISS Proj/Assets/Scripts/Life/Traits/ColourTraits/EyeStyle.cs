using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeStyle
{

    //Standard Public
    public bool eyeMatching = true;


    public EyeStyle(List<Trait> eyeStyleTraits)
    {
        EvaluateEyeStyle(eyeStyleTraits);
    }

    private void EvaluateEyeStyle(List<Trait> eyeStyleTraits)
    {
        float sum = 0.0f;
        for (int i = 0; i < eyeStyleTraits.Count; i++)
        {
            sum += eyeStyleTraits[i].numericValue;
        }

        sum = Mathf.Sin(sum);

        if(sum > 0.95f)
        {
            eyeMatching = false;
        }
    }
}
