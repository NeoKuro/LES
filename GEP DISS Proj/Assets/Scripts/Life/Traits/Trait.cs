using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trait
{
    //Custom Public
    public Gene attachedToGene;                          //Gene responsible for this trait

    //Standard Public
    public int rawID = -1;
    public string traitName = "";
    public string traitID = "";                         //ID of trait shown to players (IE "GR01")
    public string stringValue = "";
    public float numericValue = -999;

    public Trait(int id, Gene gene, string abbreviation)
    {
        GetStringValues(abbreviation);
        traitName += id;
        traitID += id;

        attachedToGene = gene;
        /*gene.attachedTrait = this;
        gene.traitAttached = true;*/
    }

    private void GetStringValues(string abbreviation)
    {
        switch(abbreviation.ToLower())
        {
            case "el":
                traitName = TraitValues.EYE_LEFT_NAME;
                traitID = TraitValues.EYE_LEFT_ABB;
                break;
            case "er":
                traitName = TraitValues.EYE_RIGHT_NAME;
                traitID = TraitValues.EYE_RIGHT_ABB;
                break;
            case "es":
                traitName = TraitValues.EYE_STYLE_NAME;
                traitID = TraitValues.EYE_STYLE_ABB;
                break;
            case "hc":
                traitName = TraitValues.HAIR_NAME;
                traitID = TraitValues.HAIR_ABB;
                break;
            case "sc":
                traitName = TraitValues.SKIN_NAME;
                traitID = TraitValues.SKIN_ABB;
                break;
            case "gr":
                traitName = TraitValues.GROWTH_RATE_NAME;
                traitID = TraitValues.GROWTH_RATE_ABB;
                break;
            case "le":
                traitName = TraitValues.LIFE_EXPECTANCY_NAME;
                traitID = TraitValues.LIFE_EXPECTANCY_ABB;
                break;
        }
    }
}
