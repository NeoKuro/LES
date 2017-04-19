using System.Collections;
using System.Collections.Generic;
//using UnityEngine;

public class Gene
{
    private enum GENETIC_OPERATOR
    {
        MUTATION = 0,
        INVERSION = 1,
        TRANSPOSITION = 2
    }

    //Standard CONST
    public const int GENE_HEAD_LENGTH = 5;

    //Custom Public
    public Trait attachedTrait;                             //The trait attached to this gene (if any)
    public Chromosome parentChromosome;                     //The chromosome this gene is attached to

    //Standard Public
    public int geneNumber = 999;
    public string gene;
    private string[] geneDomains = new string[2] { "", "" };
    public bool traitAttached = false;                      //Is a trait already attached to this gene?

    //Standard Private
    private int geneTailLength;
    private char[] elements;                              //Contains the 2 valid elements used in gene generation
    private char[] operators;                             //Contains the 5 valid operators used in gene generation

    public Gene(Chromosome chromosome, int geneNo, System.Random generator)
    {
        parentChromosome = chromosome;
        Initialize();
        GenerateGene(generator);
        geneNumber = geneNo;
        UnityEngine.Debug.Log("Gene Done");
    }

    public Gene(Chromosome chromosome, int geneNo, System.Random generator, Gene motherGenome, Gene fatherGenome)
    {
        parentChromosome = chromosome;
        Initialize();
        Recombination(motherGenome, fatherGenome, generator);
        geneNumber = geneNo;
        UnityEngine.Debug.Log("B_Gene Done");
    }

    private void Initialize()
    {
        elements = new char[2]
        {
           'A', 'B'
        };

        operators = new char[5]
        {
           '*', '/', '+', '-', '#'
        };

        geneTailLength = CalculateTailLength();
    }

    private int CalculateTailLength()
    {
        int t;
        int nMax = 1;

        for (int i = 0; i < operators.Length; i++)
        {
            if (operators[i].Equals('*'))
            {
                nMax = 2;
                break;
            }
            if (operators[i].Equals('/'))
            {
                nMax = 2;
                break;
            }
            if (operators[i].Equals('+'))
            {
                nMax = 2;
                break;
            }
            if (operators[i].Equals('-'))
            {
                nMax = 2;
                break;
            }
        }

        t = GENE_HEAD_LENGTH * (nMax - 1) + 1;

        return t;
    }

    private string GenerateGene(System.Random generator)
    {
        gene = "";                                  //Each gene length is determined by Head Length + Tail Length (usually HeadLength*2 + 1)
        //Head
        for (int i = 0; i < GENE_HEAD_LENGTH; i++)
        {
            int elementOrOperator = generator.Next(0, 2);

            //EoO == Element
            if (elementOrOperator == 0)
            {
                int elementIndex = generator.Next(0, elements.Length);
                gene += elements[elementIndex];
            }
            else
            {
                int operatorIndex = generator.Next(0, operators.Length);
                gene += operators[operatorIndex];
            }
        }

        geneDomains[0] = gene;      //Since only the head has been generated so far, set equal to the gene

        //Tail
        for (int i = 0; i < geneTailLength; i++)
        {
            int elementIndex = generator.Next(0, elements.Length);
            gene += elements[elementIndex];
            geneDomains[1] += elements[elementIndex];       //Add chosen element to the tail
        }
        return gene;
    }

    private string Recombination(Gene motherGene, Gene fatherGene, System.Random generator)
    {
        gene = "";

        //Because the genome uses Chromosomes, this basic combination method represents the genetic operator "CROSSOVER"
        //Where DNA from 2 parents/contributors is combined
        //To be more precise it resembles "Multi-point Crossover" as this recombination takes place over multiple chromosomes across the entire genome
        //      Instead of just applying to 1 chromosome and 1 gene
        //  This is also known as "Recombination" hence the method name

        Gene[] parentDNA = new Gene[2]
        {
            fatherGene,
            motherGene
        };

        //Randomly choose whether to use the mother or father gene
        //In theory it is possible to have an exact copy of one of them but oddsa re very slim
        Gene inheritedGene = parentDNA[generator.Next(parentDNA.Length)];      //Generate random number either 0 or 1. If 1, then use mother DNA. Otherwise use father DNA

        geneDomains = inheritedGene.geneDomains;

        //Assign the inherited gene string to the offspring
        gene = inheritedGene.gene;

        //Apply genetic operators here
        GeneticOperators(generator);

        return gene;
    }

    private void GeneticOperators(System.Random generator)
    {
        //Operators:
        //      Mutation (Permutation)  - 2 values in a string are swapped around
        //      Selection               - Already done at the mate Targetting processes
        //      Inversion               - A length of values within the string have their order reversed - Only applied to 1 "Domain" at a time (Head or Tail)
        //      Transposition           - A sequence of new values is inserted into the HEAD domain - As data is added can pose problems with gene length. 
        //                              -       2 Options:
        //                                              1. Maintain a constant head size by removing the excess values from the end of the HEAD
        //                                              2. Recalculate the required TailLength, then transpose new data into the tail to equal this new length
        //                              -       Can offer this as a chocie to the user (FixedGeneLength vs Varying length)

        if (generator.NextDouble() > GlobalGEPSettings.GEN_OP_RATE)
        {
            //If is higher than the rate then there will be no operator/mutator applied
            return;
        }

        List<float> geneticOperatorList = new List<float>();
        float percentage = 1.00f;   //100%


        for (int i = 0; i < GlobalGEPSettings.GO_CHANCES.Length; i++)
        {
            //The minimum threshold. To have this particular GO occur, value must be higher than this but lower than any other
            percentage -= (GlobalGEPSettings.GO_CHANCES[i] / GlobalGEPSettings.GO_CHANCES.Length);
            geneticOperatorList.Add(percentage);

            if (i == GlobalGEPSettings.GO_CHANCES.Length - 1)
            {
                geneticOperatorList[i] = 0; //By default the last value will be 0
            }
        }

        float rand = (float)generator.NextDouble();
        int operatorIndex = 0;

        for (int i = geneticOperatorList.Count - 1; i >= 0; i--)
        {
            if (rand >= geneticOperatorList[i])
            {
                operatorIndex = i;
            }
        }

        switch (operatorIndex)
        {
            case 0:
                Mutation_OP(generator);
                break;
            case 1:
                Inversion_OP(generator);
                break;
            case 2:
                Transposition_OP(generator);
                break;
            default:
                break;
        }

        //Replace the old gene string with the newly mutated genes
        gene = geneDomains[0] + geneDomains[1];
    }

    private void Mutation_OP(System.Random generator)
    {
        //Number returned is 0 or 1
        int domainIndex = generator.Next(2);
        string domainStr = geneDomains[domainIndex];
        char[] domainChars = domainStr.ToCharArray();

        int mutationCount = generator.Next(1, GlobalGEPSettings.MAX_MUT_ITERATIONS);

        for (int i = 0; i < mutationCount; i++)
        {
            
            int mutChar = generator.Next(domainStr.Length - 1);

            if(domainIndex == 0)
            {
                //head Domain. Generate any element
                domainChars[mutChar] = GenerateRandomHeadElement(generator);
            }
            else if (domainIndex == 1)
            {
                //Tail Domain. Generate Terminals only
                int elementIndex = generator.Next(0, elements.Length);
                gene += elements[elementIndex];
            }

            /*
            int secondChar = generator.Next(domainStr.Length - 1);
            int retries = 0;

            //Make sure not choosing same character to mutate, and to prevent infinite.long loops limit to 5 retries
            while (secondChar == firstChar && retries < 5)
            {
                secondChar = generator.Next(domainStr.Length - 1);
                retries++;
            }

            char storedChar = domainChars[secondChar];
            domainChars[secondChar] = domainChars[firstChar];
            domainChars[firstChar] = storedChar;*/
        }

        domainStr = new string(domainChars);
        geneDomains[domainIndex] = domainStr;
    }

    private void Inversion_OP(System.Random generator)
    {
        //Number returned is 0 or 1
        int domainIndex = generator.Next(2);
        string domainStr = geneDomains[domainIndex];
        char[] domainChars = domainStr.ToCharArray();

        int startRangeInclusive = generator.Next(domainStr.Length - 1);
        int endRangeExclusive = generator.Next(domainStr.Length - 1);



        //If the same number (diff = 0) or diff is less than the min length, regen number
        while (IsValidRange(startRangeInclusive, endRangeExclusive))
        {
            endRangeExclusive = generator.Next(domainStr.Length - 1);
        }

        if (startRangeInclusive > endRangeExclusive)
        {
            int temp = endRangeExclusive;
            endRangeExclusive = startRangeInclusive;
            startRangeInclusive = temp;
        }

        //Get the range
        int inversionRange = (endRangeExclusive - startRangeInclusive);

        string unchangedStart = domainStr.Substring(0, startRangeInclusive);
        string inversionStr = domainStr.Substring(startRangeInclusive, inversionRange);
        string unchangedEnd = domainStr.Substring(endRangeExclusive);
        char[] inversionChars = inversionStr.ToCharArray();

        System.Array.Reverse(inversionChars);
        inversionStr = new string(inversionChars);

        geneDomains[domainIndex] = unchangedStart + inversionStr + unchangedEnd;

    }

    private void Transposition_OP(System.Random generator)
    {
        //Must happen in head
        string domainStr = geneDomains[0];
        //Determine the number of characters to be inserted --- Adds one so that the number in the Settings is accurate - Random.Next(x, y); where y is non-inclusive
        int transposeLength = generator.Next(1, GlobalGEPSettings.MAX_TRANSPOSE_LENGTH + 1);
        int transposeStart = generator.Next((domainStr.Length - transposeLength));
        int currentIndex = transposeStart;
        char[] headChars = geneDomains[0].ToCharArray();

        for (int i = 0; i < transposeLength; i++)
        {
            headChars[currentIndex] = GenerateRandomHeadElement(generator);
            currentIndex++;
        }

        geneDomains[0] = new string(headChars);
    }

    private bool IsValidRange(int startRangeInclusive, int endRangeExclusive)
    {
        int range = endRangeExclusive - startRangeInclusive;
        if(range < 0)
        {
            range = startRangeInclusive - endRangeExclusive;
        }

        if(range < GlobalGEPSettings.MIN_INVERSION_LENGTH)
        {
            return true;
        }

        if(range > GlobalGEPSettings.MAX_INVERSION_LENGTH)
        {
            return true;
        }

        return false;
    }

    private char GenerateRandomHeadElement(System.Random generator)
    {
        int elementOrOperator = generator.Next(0, 2);
        //EoO == Element
        if (elementOrOperator == 0)
        {
            int elementIndex = generator.Next(0, elements.Length);
            return elements[elementIndex];
        }
        else
        {
            int operatorIndex = generator.Next(0, operators.Length);
            return operators[operatorIndex];
        }
    }

}
