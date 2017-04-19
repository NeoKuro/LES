using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum POPULATION_STATE    //Used instead of a bool so it can be later expanded on (IE other states -unhappy, expanding etc)
{
    MIGRATE,        //Is this population looking for a new location?
    STABLE          //Is this population happy where they are?
}

public class Population : MonoBehaviour
{
    public enum DIET_TYPE
    {
        CARNIVORE,
        HERBIVORE,
        OMNIVORE
    }

    //Standard Public
    public string speciesName = "Human";
    public bool initHumanPop = false;
    public bool setupComplete = false;
    public int populationIndex = 0;
    public int initPopulation = 10;          //on start create a pop-up asking to input Name + Pop. Also used to keep track of indices
    public int indexCount = 0;
    public int currentGeneration = 1;       //Current generation - New generation when a member of the previous generation has offspring
    public int hunters = 0;                 //Counter used when assigning hunters - this is so that not every member in a pop will go gather food
    public int gatherers = 0;               //Same as hunters, but for water.
    public float averageStrength = 0.0f;
    public float averageIntellect = 0.0f;
    public float averageVanity = 0.0f;
    public float averageCharisma = 0.0f;
    public float currentRadius = 10.0f;
    public Dictionary<int, int> genIndices = new Dictionary<int, int>();    //<int, int> = GetInstanceID, generation


    //Enum Public
    public POPULATION_STATE popState = POPULATION_STATE.STABLE;
    public DIET_TYPE dietType;

    //Monobehaviour Public
    public Color populationColour;
    public GameObject creatureObject;
    public GameObject initialPromptObject;
    public GameObject modalCanvas;
    public List<GameObject> population = new List<GameObject>();
    public List<Population> discoveredPopulations = new List<Population>();
    public List<int> threateningPops = new List<int>();   //List of all the indexes of the populations this Population deems is a threat
                                                                                //      IE many population members been killed by them
    public Dictionary<int, int> popDeathCounter = new Dictionary<int, int>();   //Dictionary tracking the number of deaths (,int>) caused by each population index (<int,)
    public Dictionary<int, int> popMurderTimer = new Dictionary<int, int>();    //Dictionary used to track how long its been since a pop member was murdered by each pop
    public List<GameObject> carnivoreObjects = new List<GameObject>();
    public List<GameObject> herbivoreObjects = new List<GameObject>();
    public List<GameObject> omnivoreObjects = new List<GameObject>();

    //Custom Public
    public ResourceStores popFoodStores = new ResourceStores();
    public ResourceStores popWaterStores = new ResourceStores();

    //Standard Private
    private float initRadius = 10;
    private float lastTime = 0.0f;
    private float deltaT = 0.0f;
    private int secondsCount = 0;
    private int lastSecCount = 0;

    //Monobehaviour Private
    private GameObject initialPrompt;

    //Monobehaviour Private


    public void Start()
    {
        if (popState != POPULATION_STATE.MIGRATE)
        {
            if (!initHumanPop)
            {
                return;
            }
            StartCoroutine(SetupPopulation());
        }
    }

    public void Update()
    {
        deltaT = Time.time - lastTime;
        if (deltaT >= GlobalGEPSettings.ONE_SECOND)
        {

            secondsCount++;
            lastTime = Time.time;
        }

        if(secondsCount >= GlobalGEPSettings.ONE_TIME_PERIOD * 0.5f)
        {
            if (averageIntellect >= GlobalGEPSettings.MIN_FARMING_INTELLECT && dietType == DIET_TYPE.OMNIVORE)
            {
                float foodMultiplier = averageIntellect / 100;
                if(foodMultiplier > 0.75f)
                {
                    foodMultiplier = 0.75f;
                }
                //Every half year fill the food supplies up by at minimum 25% capacity, at Maximum 75%
                //Simulates Farming, only applies to omnivores though 
                popFoodStores.SupplyResource(Mathf.RoundToInt(popFoodStores.resourceCapacity * foodMultiplier));
            }
        }

        //Once per year/Period
        if (secondsCount >= GlobalGEPSettings.ONE_TIME_PERIOD)
        {
            averageStrength = averageIntellect = averageVanity = averageCharisma = 0;
            for (int i = 0; i < population.Count; i++)
            {
                if (population[i].GetComponent<Stats>().attributes.Count > 0)
                {
                    averageStrength += population[i].GetComponent<Stats>().attributes["strength"];
                    averageIntellect += population[i].GetComponent<Stats>().attributes["intellect"];
                    averageVanity += population[i].GetComponent<Stats>().attributes["vanity"];
                    averageCharisma += population[i].GetComponent<Stats>().attributes["charisma"];
                }
            }
            //Update average attributes
            //Could put this into a Dictionary instead
            averageStrength = (averageStrength / population.Count);
            averageIntellect = (averageIntellect / population.Count);
            averageVanity = (averageVanity / population.Count);
            averageCharisma = (averageCharisma / population.Count);

            //Update the list of threats/hostile pops
            CheckThreats();

            //Analyse the population density once a year.
            //If it is too high, then create a new population group
            CheckPopulationDensity();
            secondsCount = 0;
        }

        if(popState == POPULATION_STATE.MIGRATE)
        {
            transform.position = population[0].transform.position;  //Whilst migrating, make the creature in 'first' be the "Alpha" and move the pop with them
                                                                    //      Basic implementation of "Swarm behaviour"???
        }
    }

    public void InitializeNewPopulation()
    {
        //Create a popup window prompting user to state name + init pop
        //Can also set things like diet (Herbie = food for others, Carnie = food for others + hunts)

        //If there is no creatureObject detected, then terminate. Without creature objects this cannot 

        if (initialPromptObject != null)
        {
            //Spawn popup window
            if (initialPromptObject != null)
            {
                GameObject newModalWindow = Instantiate(modalCanvas);

                //Setup Secondary Canvas
                initialPrompt = Instantiate(initialPromptObject);
                initialPrompt.transform.SetParent(newModalWindow.transform);
                initialPrompt.GetComponent<RectTransform>().localPosition = Vector3.zero;
                initialPrompt.GetComponent<InitPopSetupWindow>().InitializeWindow(this);
            }
        }
        //Start spawning initial populations
        StartCoroutine(SetupPopulation());
    }

    public IEnumerator SetupPopulation()
    {
        while (!setupComplete)
        {
            yield return new WaitForEndOfFrame();
        }

        if (creatureObject == null)
        {
            switch (dietType)
            {
                case DIET_TYPE.CARNIVORE:
                    int cIndex = Random.Range(0, carnivoreObjects.Count - 1);
                    creatureObject = carnivoreObjects[cIndex];
                    break;
                case DIET_TYPE.HERBIVORE:
                    int hIndex = Random.Range(0, carnivoreObjects.Count - 1);
                    creatureObject = herbivoreObjects[hIndex];
                    break;
                default:
                    int oIndex = Random.Range(0, carnivoreObjects.Count - 1);
                    creatureObject = omnivoreObjects[oIndex];
                    break;
            }

            if (creatureObject == null)
            {
                Debug.Log("CreatureObject is Null. Has it been assigned in the inspector?");
            }
        }

        populationColour = GetComponent<SpriteRenderer>().color;

        if (speciesName != "Human" || !setupComplete)
        {
            float r, g, b;
            r = Random.Range(0.0f, 1.0f);
            g = Random.Range(0.0f, 1.0f);
            b = Random.Range(0.0f, 1.0f);

            populationColour = new Color(r, g, b);

            gameObject.GetComponent<SpriteRenderer>().color = populationColour;
        }

        //Spawn population
        for (int i = 0; i < initPopulation; i++)
        {
            GameObject newPop = Instantiate(creatureObject);
            newPop.GetComponent<CreatureManager>().inSetup = true;
            newPop.GetComponent<CreatureManager>().population = this;
            newPop.name = speciesName.ToString() + " " + i;
            newPop.transform.SetParent(gameObject.transform);
            Vector3 newPos = RandomCircleLocation();
            newPos = new Vector3(newPos.x, newPos.y, -0.5f);
            newPop.transform.position = newPos;
            yield return new WaitForFixedUpdate();  //Wait until the next fixed update (physics check) has occurred to check if in invalid position
            newPop.GetComponent<CreatureManager>().inSetup = false;
            newPop.GetComponent<CreatureManager>().replicationMethod = REPLICATION_METHOD.RANDOMLY_GENERATE;
            newPop.GetComponent<CreatureManager>().InitializeCreature();
            newPop.GetComponent<CreatureManager>().creatureIndex = indexCount;
            if (initHumanPop)
            {
                newPop.GetComponent<CreatureManager>().initHumanPop = true;
            }
            genIndices.Add(newPop.GetComponent<CreatureManager>().creatureIndex, 1);
            population.Add(newPop);
            indexCount++;
        }

        popFoodStores.InitialSetup(this, "Food");
        popWaterStores.InitialSetup(this, "Water");
        yield return null;
    }

    private void CheckThreats()
    {
        List<int> hostilePopKeys = new List<int>(popMurderTimer.Keys);

        for(int i = 0; i < hostilePopKeys.Count; i++)
        {
            int key = hostilePopKeys[i];
            //if last recorded death by this pop was more than 5 Periods ago, remove them as a threat
            //      Could add other attributes such as "Grudges" as in how long a pop will hold grudges
            //          could be done on average, OR could eventually add in 'governments' where individuals 
            //          make more decisions and so would go based off their traits/attributes etc
            if (popMurderTimer[key] >= GlobalGEPSettings.POP_THREATENING_PERIOD)
            {
                if (popDeathCounter.ContainsKey(key))
                    popDeathCounter.Remove(key);

                if (threateningPops.Contains(key))
                    threateningPops.Remove(key);

                if (popMurderTimer.ContainsKey(key))
                    popMurderTimer.Remove(key);

                continue;
            }

            //Otherwise increment the counter by 1 period
            popMurderTimer[key] += 1;
        }
    }

    private void CheckPopulationDensity()
    {
        //If the population density is morethan 15People/point
        if(population.Count / currentRadius >= 15)
        {
            MigrateNewPopulation();
        }
    }

    private void MigrateNewPopulation()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
        Population migratingPopulation = new Population();
        int newPopCount = Random.Range(2, (population.Count / 2));
        migratingPopulation.populationColour = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f));
        List<int> newIndexes = new List<int>();
        for (int i = 0; i < newPopCount; i++)
        {
            int popIndex = Random.Range(0, population.Count);

            migratingPopulation.population.Add(population[popIndex]);
            migratingPopulation.population[i].GetComponent<Stats>().generation = 1;
            migratingPopulation.population[i].GetComponent<Stats>().traits["Food Levels"] = migratingPopulation.population[i].GetComponent<Stats>().capacityTraits["Food Levels"];
            migratingPopulation.population[i].GetComponent<Stats>().traits["Water Levels"] = migratingPopulation.population[i].GetComponent<Stats>().capacityTraits["Water Levels"];
            migratingPopulation.population[i].GetComponent<Stats>().traits["Energy Levels"] = migratingPopulation.population[i].GetComponent<Stats>().capacityTraits["Energy Levels"];
            migratingPopulation.population[i].GetComponent<CreatureManager>().popColourObject.GetComponent<SpriteRenderer>().color = migratingPopulation.populationColour;
            migratingPopulation.population[i].GetComponent<CreatureManager>().creatureIndex = i;

            if (migratingPopulation.population[i].GetComponent<Stats>().pregnant)
            {
                for (int j = 0; j < migratingPopulation.population[i].GetComponent<AIBehaviour>().offspring.Count; j++)
                {
                    migratingPopulation.population[i].GetComponent<AIBehaviour>().offspring[j].GetComponent<Stats>().generation = 2;
                    migratingPopulation.population[i].GetComponent<AIBehaviour>().offspring[j].
                        GetComponent<CreatureManager>().popColourObject.GetComponent<SpriteRenderer>().color = migratingPopulation.populationColour;
                }
            }

            population.RemoveAt(popIndex);
        }

        migratingPopulation.popState = POPULATION_STATE.MIGRATE;
        migratingPopulation.popFoodStores.currentAmount = migratingPopulation.population.Count;
        if (averageIntellect >= GlobalGEPSettings.MIN_WATER_STORE_INTELLECT)
        {
            migratingPopulation.popWaterStores.currentAmount = migratingPopulation.population.Count;
        }

        migratingPopulation.dietType = dietType;
        migratingPopulation.setupComplete = true;
        migratingPopulation.modalCanvas = modalCanvas;
        migratingPopulation.creatureObject = creatureObject;
        migratingPopulation.initialPrompt = initialPrompt;
        migratingPopulation.initialPromptObject = initialPromptObject;
    }

    private Vector3 RandomCircleLocation()
    {
        float radius = Random.Range(0, initRadius);
        Vector3 center = gameObject.transform.position;
        float ang = Random.value * 360;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.z = center.z;
        return pos;
    }
}
