using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneColour
{
    //Monobehaviour Public
    public Color color;

    //Standard Private
    private float[] rawColours = new float[3]
        {
            0.0f,
            0.0f,
            0.0f
        };
    private float[] evalColours = new float[3]
        {
            0.0f,
            0.0f,
            0.0f
        };

    public GeneColour(List<Trait> traitList)
    {
        for(int i = 0; i < traitList.Count; i++)
        {
            rawColours[traitList[i].rawID] += traitList[i].numericValue;
        }

        EvaluateColour();
        SetColour();
    }

    private void EvaluateColour()
    {
        for (int i = 0; i < rawColours.Length; i++)
        {
            evalColours[i] = Mathf.Sin(rawColours[i]);

            if (evalColours[i] < 0)
            {
                evalColours[i] *= -1;
            }
        }
    }

    private void SetColour()
    {
        color = new Color(evalColours[0], evalColours[1], evalColours[2]);
    }
}


