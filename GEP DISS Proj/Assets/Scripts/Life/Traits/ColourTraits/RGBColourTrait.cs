using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RGBColourTrait : Trait
{
    //Holds Eye, Hair and Skin colours (all 3 RGB values)
    //Difference is held in the traitName/ID

    //Standard Public
    public string rgb = "";

    public RGBColourTrait(string trait, int id, Gene gene, string abbreviation, CreatureManager hManager) : base(id, gene, abbreviation)
    {

        traitName = trait + " " + id;

        switch (id)
        {
            case 0:
                traitName += " Red";
                rgb = "red";
                break;
            case 1:
                traitName += " Green";
                rgb = "green";
                break;
            case 2:
                traitName += " Blue";
                rgb = "blue";
                break;
        }

        traitID = abbreviation + id;
        rawID = id;
        EvaluateGene(gene, hManager);
    }


    public void EvaluateGene(Gene gene, CreatureManager hManager)
    {
        int chromosomeIndex = gene.parentChromosome.chromosomeNumber;
        int geneIndex = gene.geneNumber;
        if (GlobalGEPSettings.TRAITS_ON_GENES)
        {
            numericValue = hManager.phenotypeNodesManager.chromosomePhenotypeGeneResults[chromosomeIndex][geneIndex];
        }
        else
        {
            numericValue = hManager.phenotypeNodesManager.chromosomePhenotypeChromoResults[chromosomeIndex];
        }
    }
}
