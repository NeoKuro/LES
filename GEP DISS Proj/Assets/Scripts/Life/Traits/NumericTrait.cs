using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumericTrait : Trait
{


    public NumericTrait(int id, Gene gene, CreatureManager hManager, string abbreviation, string thisName) : base(id, gene, abbreviation)
    {
        numericValue = EvaluateGene(gene, hManager);
        traitID = abbreviation + id;
        traitName = thisName;
    }

    public float EvaluateGene(Gene gene, CreatureManager hManager)
    {
        int chromosomeIndex = gene.parentChromosome.chromosomeNumber;
        int geneIndex = gene.geneNumber;
        float val = 0;
        if (GlobalGEPSettings.TRAITS_ON_GENES)
        {
            val = hManager.phenotypeNodesManager.chromosomePhenotypeGeneResults[chromosomeIndex][geneIndex];
        }
        else
        {
            val = hManager.phenotypeNodesManager.chromosomePhenotypeChromoResults[chromosomeIndex];
        }
        return val;
    }
}
