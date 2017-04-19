using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributeTrait
{

    //Standard Public
    public string attributeName = "";
    public float attributeTraitVal;

    //Standard Private


    public AttributeTrait(List<Trait> attributeTrait)
    {
        attributeName = attributeTrait[0].traitName;
        EvaluateAttributeTrait(attributeTrait);
    }

    private void EvaluateAttributeTrait(List<Trait> attributeTrait)
    {
        float sum = 0.0f;
        for (int i = 0; i < attributeTrait.Count; i++)
        {
            sum += attributeTrait[i].numericValue;
        }
        float average = sum / attributeTrait.Count;
        float variance = 0f;
        List<float> differences = new List<float>();
        for (int i = 0; i < attributeTrait.Count; i++)
        {
            float var = attributeTrait[i].numericValue;
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
        float attributeTraitPreEqn = Mathf.Sqrt(sd);

        float angle = Mathf.Deg2Rad * attributeTraitPreEqn;

        float attributeTraitEqn = (Mathf.Sin(angle) * 10);

        if (attributeTraitVal < 0)
        {
            attributeTraitVal *= -1;
        }

        attributeTraitVal = (attributeTraitEqn - (Mathf.Atan(angle) / 4)) * 20;
        

        //Debug.Log("Total " + attributeName + ": " + attributeTraitVal + "     PreEqn: " + Mathf.Tan(angle));
        //if ((attributeTraitVal >= 0.25f && attributeTraitVal > 0.075f) && attributeTraitVal != 0)
        //    Debug.Log("SUPER ENERGY: " + attributeTraitVal);
    }
}
