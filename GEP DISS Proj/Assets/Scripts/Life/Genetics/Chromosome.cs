
public class Chromosome //: MonoBehaviour
{

    //Custom Public
    public GenomeManager genome;
    public Gene[] genes;              //Genes attached to this Chromosome

    //Standard Public
    public int chromosomeNumber = 999;  //Invalid number initially
    public int traits = 0;              //Number of traits on this chromosome
    public float chromosomePhenotypicValue = -999;

    public Chromosome(int chromosome, GenomeManager thisGenome, CreatureManager hManager)
    {
        genes = new Gene[GlobalGEPSettings.CHROMOSOME_LENGTH];
        genome = thisGenome;
        chromosomeNumber = chromosome;
        GenerateChromosome(hManager.generator);
    }

    public Chromosome(int chromosome, GenomeManager thisGenome, CreatureManager hManager, Chromosome motherChromosome, Chromosome fatherChromosome)
    {
        genes = new Gene[GlobalGEPSettings.CHROMOSOME_LENGTH];
        genome = thisGenome;
        chromosomeNumber = chromosome;

        for (int i = 0; i < GlobalGEPSettings.CHROMOSOME_LENGTH; i++)
        {
            if(chromosomeNumber != 22)
            {
                genes[i] = new Gene(this, i, hManager.generator, motherChromosome.genes[i], fatherChromosome.genes[i]);
            }
        }
    }

    private void GenerateChromosome(System.Random generator)
    {

        for (int i = 0; i < GlobalGEPSettings.CHROMOSOME_LENGTH; i++)
        {
            if (chromosomeNumber != 22)
            {
                genes[i] = new Gene(this, i, generator);
            }
        }
    }
}
