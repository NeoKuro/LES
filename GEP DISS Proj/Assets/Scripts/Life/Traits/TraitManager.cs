using System.Collections;
using System.Collections.Generic;
//using UnityEngine;

public class TraitManager //: MonoBehaviour
{
    //Used to store the results of all the traits that have multiple components (IE colour)

    //Custom Public
    public Chromosome[] genome;
    public GeneColour hairColour;
    public GeneColour[] eyeColours = new GeneColour[2];
    public EyeStyle eyeStyle;
    public GeneColour skinColour;
    public GrowthRate growthRate;
    public LifeExpectancy lifeExpectancy;
    public ReproductiveAge reproductiveAge;
    public GestationPeriod gestationPeriod;
    public EnergyLevel energyLevel;
    public EnergyConsumption energyConsumption;
    public FoodLevel foodLevel;
    public FoodConsumption foodConsumption;
    public WaterLevel waterLevel;
    public WaterConsumption waterConsumption;
    public List<AttributeTrait> attributesList = new List<AttributeTrait>();


    //Standard Public
    public Dictionary<string, int[][]> traitIndices;       //List of all chromosomes for each trait, indexed by trait names;
                                                           //Dictionary<TRAIT_NAME, Chromosomes_Attached>
                                                           //public Dictionary<string, Gene[]> traitGenes;                   //List of all genes for each trait, indexed by trait names
                                                           //Dictionary<TRAIT_NAME, Genes_Attached>

    //Custom Private
    private TraitList traitList;


    public void GenerateTraitsList(CreatureManager hManager, string speciesName)
    {
        //Generate the list of traits. This is because all traits will be on the same layout
        //              This MIGHT change through mutation (swap with other traits etc?)
        //genome = gameObject.GetComponent<HumanManager>().genomeManager.genome;
        genome = hManager.genomeManager.genome;
        traitList = new TraitList(genome, this, hManager, speciesName);
        //This has been moved to the Thread
        //EvaluateTraits();
    }

    public void EvaluateTraits(CreatureManager hManager, string speciesName)
    {
        List<Gene> genes = new List<Gene>();
        if (!GlobalGEPSettings.RANDOMIZED_TRAITS || traitIndices.Count == 0)
        {
            traitIndices = GlobalGEPSettings.speciesTraitLayouts[speciesName];
        }

        foreach (KeyValuePair<string, int[][]> thisTrait in traitIndices)
        {
            string key = thisTrait.Key;
            //For every chromosome a trait is linked to...
            //i = chromosome index (when a trait is on multiple chromosomes)
            for (int i = 0; i < thisTrait.Value.Length; i++)
            {
                //Index [i][0] will ALWAYS be the chromosome index
                int chromosomeIndex = thisTrait.Value[i][0];
                //For every gene this trait is linked to (on this chromosome)...
                //j = gene index for this chromosome
                for (int j = 1; j < thisTrait.Value[i].Length; j++)
                {
                    //Get the gene index
                    int geneIndex = thisTrait.Value[i][j];
                    //Add the gene at the chromosomeIndex and geneIndex to a list to be evaluated
                    //Accesses the genome rather than making a copy ensuring the genes accessed later on match exactly the current genome
                    genes.Add(genome[chromosomeIndex].genes[geneIndex]);
                }
            }

            List<Trait> thisTraitList = AccessTraits(genes);

            //For each trait (keyValuePair) evaluate the genes
            //Could put the results into a dictionary - this way would be more polymorphic as would search for (or add) a row for each trait and search by name
            //  instead of individual variables, or an array which requires the developer to remember which index is which
            switch (key.ToLower())
            {
                case "eye left colour":
                    eyeColours[0] = new GeneColour(thisTraitList);
                    if (eyeColours[1] == null && eyeStyle != null)
                    {
                        if (eyeStyle.eyeMatching)
                        {
                            eyeColours[1] = eyeColours[0];
                            UnityEngine.Debug.Log("Eye Matching, Assigned 1 = 0");
                        }
                    }
                    break;
                case "eye right colour":
                    eyeColours[1] = new GeneColour(thisTraitList);
                    if (eyeColours[0] == null && eyeStyle != null)
                    {
                        if (eyeStyle.eyeMatching)
                        {
                            eyeColours[0] = eyeColours[1];
                            UnityEngine.Debug.Log("Eye Matching, Assigned 0 = 1");
                        }
                    }
                    break;
                case "eye style":
                    eyeStyle = new EyeStyle(thisTraitList);
                    if (eyeStyle.eyeMatching)
                    {
                        if (eyeColours[0] != null)
                        {
                            eyeColours[1] = eyeColours[0];
                        }
                        else if (eyeColours[1] != null)
                        {
                            eyeColours[0] = eyeColours[1];
                        }
                        else
                        {
                            UnityEngine.Debug.Log("Error: Both eye colours are currently NULL");
                        }
                    }
                    break;
                case "hair colour":
                    hairColour = new GeneColour(thisTraitList);
                    break;
                case "skin colour":
                    skinColour = new GeneColour(thisTraitList);
                    break;
                case "growth rate":
                    growthRate = new GrowthRate(thisTraitList);
                    break;
                case "life expectancy":
                    lifeExpectancy = new LifeExpectancy(thisTraitList);
                    break;
                case "reproductive age":
                    reproductiveAge = new ReproductiveAge(thisTraitList);
                    break;
                case "gestation period":
                    gestationPeriod = new GestationPeriod(thisTraitList);
                    break;
                case "energy level":
                    energyLevel = new EnergyLevel(thisTraitList);
                    break;
                case "energy consumption":
                    energyConsumption = new EnergyConsumption(thisTraitList);
                    break;
                case "food level":
                    foodLevel = new FoodLevel(thisTraitList);
                    break;
                case "food consumption":
                    foodConsumption = new FoodConsumption(thisTraitList);
                    break;
                case "water level":
                    waterLevel = new WaterLevel(thisTraitList);
                    break;
                case "water consumption":
                    waterConsumption = new WaterConsumption(thisTraitList);
                    break;
                case "strength":
                case "intellect":
                case "constitution":
                case "wisdom":
                case "charisma":
                case "vanity":
                    attributesList.Add(new AttributeTrait(thisTraitList));
                    break;
            }

            genes.Clear();
        }
    }

    public TraitList GetTraitList()
    {
        return traitList;
    }

    private void EvaluateColour(List<Trait> traits, CreatureManager hManager)
    {


        float r = 0.0f, g = 0.0f, b = 0.0f;
        for (int i = 0; i < traits.Count; i++)
        {
            RGBColourTrait trait = (RGBColourTrait)traits[i];
            trait.EvaluateGene(trait.attachedToGene, hManager);


            switch (trait.rgb.ToLower())
            {
                case "red":
                    r = traits[i].numericValue;
                    break;
                case "green":
                    g = traits[i].numericValue;
                    break;
                case "blue":
                    b = traits[i].numericValue;
                    break;
            }
        }
    }

    private List<Trait> AccessTraits(List<Gene> genes)
    {
        List<Trait> traits = new List<Trait>();
        for (int i = 0; i < genes.Count; i++)
        {
            traits.Add(genes[i].attachedTrait);
        }

        return traits;
    }
}
