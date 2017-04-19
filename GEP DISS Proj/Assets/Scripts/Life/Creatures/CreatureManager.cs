using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum REPLICATION_METHOD
{
    RANDOMLY_GENERATE,          //Randomly generate a new pop/genome
    REPRODUCE,                  //Reproduce with partner creature
    REPLICATE                   //Make a clone/copy of self
}

public class CreatureManager : MonoBehaviour
{
    //Custom Class Public
    //Have a class that handles 'state' - include a state for "reproduction"
    //-- State should be publicly visible, so that when Genome is loaded (new birth), then the genome script can determine if 'reproducing' or 'init generation'
    //--if reproducing will need to execute a function that merges parent DNA
    //--if 'initGen' will randomly generate the data
    public Population population;                   //Stores a reference to the parent population (the "Tribe" as it were)
    public GenomeManager genomeManager;             //Stores/represents the genotype of individual
    public NodeManager phenotypeNodesManager;       //Stores/represents the Phenotype of individual
    public TraitManager traitManager;               //Stores all the traits for an individual
    public Stats creatureStats;
    public AIBehaviour behaviourManager;            //Responsible for handling all actions/behaviours of the creatures (including aging, eating, drinking etc)
    public CreatureManager fatherCreature;            //When reproducing with another creature, this stores a reference to the other parent
    public CreatureManager motherCreature;            //When reproducing with another creature, this stores a reference to the other parent

    //Monobehaviour Public
    public Color[] eyeColours;
    public Color hairColour;
    public Color skinColour;
    public GameObject popColourObject;

    //Standard Public
    public System.Random generator;
    public int creatureIndex = -1;
    public bool inSetup = false;
    public bool initHumanPop = false;
    public REPLICATION_METHOD replicationMethod = REPLICATION_METHOD.RANDOMLY_GENERATE;
    public string speciesName = "Human";

    //Custom Private
    [SerializeField]
    private GeneColour eyeColourTrait;
    [SerializeField]
    private GeneColour hairColourTrait;
    [SerializeField]
    private GeneColour skinColourTrait;

    //Monobehaviour Private
    private GameObject hairObject;
    private GameObject skinObject;
    private GameObject[] eyeObjects;

    //Standard Private
    private Dictionary<string, int[][]> traitIndices;

    //Threaded Vars
    private Job loadCreatureJob;
    public bool loadCreatureComplete = false;


    // Use this for initialization
    public void InitializeCreature()
    {
        StartCoroutine(SetupCreature());
    }

    private IEnumerator SetupCreature()
    {
        //Find each object in the main Creature object
        //Such as Hair, skin and eye objects
        //Cycles through each child looking at tags
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            string childTag = gameObject.transform.GetChild(i).tag;

            //get hair object 
            if (childTag.ToLower().Equals("hair"))
            {
                hairObject = gameObject.transform.GetChild(i).gameObject;
            }
            else if (childTag.ToLower().Equals("skin"))
            {
                skinObject = gameObject.transform.GetChild(i).gameObject;
            }
            else if (childTag.ToLower().Equals("eyes"))
            {
                GameObject eyesObject = gameObject.transform.GetChild(i).gameObject;
                eyeObjects = new GameObject[eyesObject.transform.childCount];

                for (int j = 0; j < eyesObject.transform.childCount; j++)
                {
                    eyeObjects[j] = eyesObject.transform.GetChild(j).gameObject;
                }
            }
            else if(childTag.ToLower().Equals("background"))
            {
                popColourObject = gameObject.transform.GetChild(i).gameObject;
            }
        }

        //Initialize key components
        generator = new System.Random((int)System.DateTime.Now.Ticks);
        genomeManager = new GenomeManager();
        traitManager = new TraitManager();
        phenotypeNodesManager = new NodeManager();
        creatureStats = gameObject.AddComponent<Stats>();

        //Start a new threaded job to run concurrently with the Unity program.
        //This handles all the loading of data (such as generating new genome etc)
        //For Reproduction, a different function will be needed that does not randomly generate the genome
        loadCreatureJob = new Job();
        loadCreatureJob.hManager = this;
        loadCreatureJob.Start();

        if (loadCreatureJob == null)
        {
            Debug.Log("ERROR: loadCreatureJob does not exist, cannot continue as no data will be available");
            yield return null;
        }
        //Halt setup until loading of creature genome/phenotype has been complete
        yield return StartCoroutine(loadCreatureJob.WaitFor());

        traitIndices = traitManager.traitIndices;

        //Set hair, Eye and skin Colour
        hairColour = traitManager.hairColour.color;
        skinColour = traitManager.skinColour.color;
        eyeColours = new Color[eyeObjects.Length];

        //For each eye object found (either 1 or 2 eyes) assign the colours
        for(int i = 0; i < eyeColours.Length; i++)
        {
            eyeColours[i] = traitManager.eyeColours[i].color;
            eyeObjects[i].GetComponent<SpriteRenderer>().color = eyeColours[i];
        }
        
        //If skin object found assign skin colour (some creatures only have hair/fur)
        if(skinObject != null)
        {
            skinObject.GetComponent<SpriteRenderer>().color = skinColour;
        }

        if(popColourObject != null)
        {
            popColourObject.GetComponent<SpriteRenderer>().color = population.populationColour;
        }

        //Assign hair/fur colour
        hairObject.GetComponent<SpriteRenderer>().color = hairColour;

        creatureStats.SetStats(this);

        behaviourManager = gameObject.AddComponent<AIBehaviour>();
        //AI FSM Notes        https://forum.unity3d.com/threads/state-machine-coroutine-vs-update.103110/

        yield return null;
    }
}
