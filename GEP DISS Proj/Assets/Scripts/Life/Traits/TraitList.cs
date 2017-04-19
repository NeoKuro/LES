using System.Collections;
using System.Linq;
using System.Collections.Generic;
//using UnityEngine;


public class TraitList //: MonoBehaviour
{
    private struct TraitData
    {
        public List<TraitIndices> indices;
    }

    private struct TraitIndices
    {
        public int chromosomeIndex;
        public int geneIndex;
        public int listIndex;
    }
    //Custom Public
    public Chromosome[] genome;

    //Custom Private
    private TraitManager manager;

    public TraitList(Chromosome[] thisGenome, TraitManager thisManager, CreatureManager hManager, string speciesName)
    {
        manager = thisManager;
        manager.traitIndices = new Dictionary<string, int[][]>();
        genome = thisGenome;

        //Add row for this Species if it does not already exist
        if (!GlobalGEPSettings.speciesTraitLayouts.ContainsKey(speciesName))
        {
            GlobalGEPSettings.speciesTraitLayouts.Add(speciesName, new Dictionary<string, int[][]>());
        }

        GenerateTraitsList(hManager, speciesName);

        if (!GlobalGEPSettings.RANDOMIZED_TRAITS)
            SetupTraits(speciesName, hManager);
    }

    public void SetupTraits(string speciesName, CreatureManager hManager)
    {
        Dictionary<string, int[][]> indices = GlobalGEPSettings.speciesTraitLayouts[speciesName];
        int index = 0;

        foreach (KeyValuePair<string, int[][]> traitType in indices)
        {
            int[][] geneData = traitType.Value;
            index = 0;
            for (int i = 0; i < geneData.Length; i++)
            {
                for (int j = 1; j < geneData[i].Length; j++)
                {
                    int chromosomeIndex = geneData[i][0];
                    int geneIndex = geneData[i][j];
                    Gene thisGene = hManager.genomeManager.genome[chromosomeIndex].genes[geneIndex];
                    hManager.genomeManager.genome[chromosomeIndex].genes[geneIndex].traitAttached = true;
                    AttachTrait(traitType.Key, index, thisGene, hManager, GetAbbreviation(traitType.Key));
                    index++;
                }
            }
        }
    }

    //Generate all traits, randomly placing them on Chromosomes (for now)
    private void GenerateTraitsList(CreatureManager hManager, string speciesName)
    {
        for (int i = 0; i < TraitValues.TRAIT_IDS.Length; i++)
        {
            int[][] traitIndices = new int[TraitValues.TRAIT_IDS.Length][];

            if (GlobalGEPSettings.RANDOMIZED_TRAITS || !GlobalGEPSettings.speciesTraitLayouts[speciesName].ContainsKey(TraitValues.TRAIT_NAME[i]))
                traitIndices = GenerateTraits(TraitValues.GENE_COUNT[i], TraitValues.TRAIT_NAME[i], hManager, TraitValues.TRAIT_ABB[i]);

            if (GlobalGEPSettings.RANDOMIZED_TRAITS)
            {
                //Store the local arrays in the dictionaries
                manager.traitIndices.Add(TraitValues.TRAIT_NAME[i], traitIndices);
            }
            else if (!GlobalGEPSettings.speciesTraitLayouts[speciesName].ContainsKey(TraitValues.TRAIT_NAME[i]))
            {
                GlobalGEPSettings.speciesTraitLayouts[speciesName][TraitValues.TRAIT_NAME[i]] = traitIndices;
            }
        }
    }

    //Generates a list of traits and returns the indices of the traits chromosomes + Genes
    private int[][] GenerateTraits(int genesCount, string traitName, CreatureManager hManager, string abbreviation)
    {
        //List of used chromosome indices for each trait (used to check if already used, rather than adding a new row to the int[][])
        List<int> chromosomeIndices = new List<int>();
        TraitData tempTraitIndices = new TraitData();
        tempTraitIndices.indices = new List<TraitIndices>();
        TraitIndices traitData = new TraitIndices();
        int[][] traitIndices;
        int listIndex = 0;
        bool uniqueIndex;

        genome = hManager.genomeManager.genome;

        for (int i = 0; i < genesCount; i++)
        {
            uniqueIndex = true;
            //int chromosomeNumber = Random.Range(0, GenomeManager.GENOME_LENGTH);
            //int geneNumber = Random.Range(0, genome[chromosomeNumber].genes.Length);
            int chromosomeNumber = hManager.generator.Next(0, GlobalGEPSettings.GENOME_LENGTH);
            int geneNumber = hManager.generator.Next(0, genome[chromosomeNumber].genes.Length);

            while (chromosomeNumber == 22 || (genome[chromosomeNumber].traits >= GlobalGEPSettings.CHROMOSOME_LENGTH))
            {
                //Disallow trait on Sex Chromsoomes
                //Have to do something special for sex chromosome (as it is 2 different 'types' XY or XX)
                chromosomeNumber = hManager.generator.Next(0, GlobalGEPSettings.GENOME_LENGTH);
            }



            while (genome[chromosomeNumber].genes[geneNumber].traitAttached == true)
            {
                //If there is a trait already attached then find a new gene
                geneNumber = hManager.generator.Next(0, genome[chromosomeNumber].genes.Length);
            }

            if (genome[chromosomeNumber].genes[geneNumber].traitAttached == false)
            {
                for (int j = 0; j < chromosomeIndices.Count; j++)
                {
                    if (chromosomeIndices[j] == chromosomeNumber)
                    {
                        //If chromosome number is not unique (chromosome previously chosen for this trait) 
                        //  then store a reference to that index (j should equal int[j][] below)
                        uniqueIndex = false;
                        listIndex = j;
                        break;
                    }

                    //If is unique, then add to the list and store the new reference to 1 higher than current j value
                    //  j + 1 because it is a new insertation
                    listIndex = j + 1;
                }

                if (uniqueIndex)
                {
                    chromosomeIndices.Add(chromosomeNumber);
                }

                //Set the index data for the trait
                //  this is used to setup the jagged array in TraitManager.Dictionary<int, int[][]>
                traitData.chromosomeIndex = chromosomeNumber;
                traitData.geneIndex = geneNumber;
                traitData.listIndex = listIndex;
                tempTraitIndices.indices.Add(traitData);

                //Signal a trait has been asigned, but don't attach anything yet
                genome[chromosomeNumber].genes[geneNumber].traitAttached = true;
                genome[chromosomeNumber].traits++;

                if (GlobalGEPSettings.RANDOMIZED_TRAITS)
                {
                    //Change trait on gene to true.
                    AttachTrait(traitName, i, genome[chromosomeNumber].genes[geneNumber], hManager, abbreviation);
                }
            }
        }

        traitIndices = new int[chromosomeIndices.Count][];

        for (int i = 0; i < traitIndices.Length; i++)
        {
            traitIndices[i] = new int[GlobalGEPSettings.MAX_GENES_PER_TRAIT];
            for (int j = 0; j < traitIndices[i].Length; j++)
            {
                traitIndices[i][j] = -1;
            }
        }

        int traitIndicesValueCount = 1;

        for (int i = 0; i < tempTraitIndices.indices.Count; i++)
        {
            //Check to make sure traitIndices at this listIndex (row) is not empty
            if (traitIndices[tempTraitIndices.indices[i].listIndex] != null)
            {
                int count = 0;
                for (int j = 0; j < traitIndices[tempTraitIndices.indices[i].listIndex].Length; j++)
                {
                    if (traitIndices[tempTraitIndices.indices[i].listIndex][j] != -1)
                    {
                        count++;
                    }
                }
                traitIndicesValueCount = count;
            }

            if (traitIndicesValueCount == 0)
            {
                //Index [x][0] will ALWAYS be the chromosome index
                traitIndices[tempTraitIndices.indices[i].listIndex][0] = tempTraitIndices.indices[i].chromosomeIndex;
                traitIndicesValueCount = 1;
            }

            traitIndices[tempTraitIndices.indices[i].listIndex][traitIndicesValueCount] = tempTraitIndices.indices[i].geneIndex;

        }

        for (int i = 0; i < traitIndices.Length; i++)
        {
            traitIndices[i] = ValidateIndices(traitIndices[i]);
        }

        return traitIndices;
    }



    private int[] ValidateIndices(IEnumerable<int> indices)
    {
        return indices.Where(i => i != -1).ToArray();
    }

    private void AttachTrait(string traitName, int traitID, Gene gene, CreatureManager hManager, string abbreviation)
    {
        switch (abbreviation.ToLower())
        {
            case "el":
            case "er":
            case "hc":
            case "sc":
                gene.attachedTrait = new RGBColourTrait(traitName, traitID, gene, abbreviation, hManager);
                break;
            default:
                gene.attachedTrait = new NumericTrait(traitID, gene, hManager, abbreviation, traitName);
                break;
        }
    }

    private string GetAbbreviation(string traitName)
    {
        for (int i = 0; i < TraitValues.TRAIT_NAME.Length; i++)
        {
            if (TraitValues.TRAIT_NAME[i] == traitName)
            {
                return TraitValues.TRAIT_ABB[i];
            }
        }

        UnityEngine.Debug.Log("ERROR: Trait not found: " + traitName);
        return "GR";    //Return Growth Rate as default (most traits will be of numerical value, this will prevent crashes/null errors)
    }
}
