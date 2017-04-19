public class GenomeManager
{

    //Custom Public
    public CreatureManager manager;         //Will need to become a "generic" class to work with animals

    //Standard Public
    public Chromosome[] genome;    //Entire Genome consists of 23 chromosomes  -- Chromosome 23 ([22]) = Sex Chromosomes     

    //Standard Private


    // Use this for initialization
    public void GenerateNewGenome(CreatureManager thisManager)
    {
        genome = new Chromosome[GlobalGEPSettings.GENOME_LENGTH];
        manager = thisManager;

        if (thisManager.replicationMethod == REPLICATION_METHOD.REPRODUCE)
        {
            //If being born, the constructor will handle the genome
            Chromosome[] fatherGenome = thisManager.fatherCreature.genomeManager.genome;
            Chromosome[] motherGenome = thisManager.motherCreature.genomeManager.genome;

            BirthNewGenome(motherGenome, fatherGenome);

            return;
        }

        GenerateGenome();

        return;
    }

    public void BirthNewGenome(Chromosome[] motherGenome, Chromosome[] fatherGenome)
    {
        //Constructor - Used when a new 'human' is being 'born'. This way the DNA is not randomly generated, but rather is acquired from the parents
        for(int i = 0; i < GlobalGEPSettings.GENOME_LENGTH; i++)
        {
            genome[i] = new Chromosome(i, this, manager, motherGenome[i], fatherGenome[i]);
        }

    }

    private void GenerateGenome()
    {
        for (int i = 0; i < GlobalGEPSettings.GENOME_LENGTH; i++)
        {
            genome[i] = new Chromosome(i, this, manager);
        }
    }

    private void PrintFullGenome()
    {
        string genomeStr = "";

        for(int i = 0; i < genome.Length - 1; i++)
        {
            for(int j = 0; j < genome[i].genes.Length; j++)
            {
                genomeStr += genome[i].genes[j].gene;
            }
            genomeStr += "   ";
        }
        
    }

    private void Update()
    {
        //Debug.Log(genome[0].genes[0]);
    }    
}
