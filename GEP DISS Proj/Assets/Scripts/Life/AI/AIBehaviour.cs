using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PRIORITY
{
    QUALITY,        //The default priority if all other priorities are satisfied
    SAFETY,         //Check how many deaths have occurred nearby
    WATER,          //Is there water nearby or stored?
    FOOD,           //What are food stores in the population like?
    SPACE           //Whats the density of the population centre like?
}

public class AIBehaviour : MonoBehaviour
{
    public enum BEHAVIOUR_STATE
    {
        NULL_STATE,     //Default state
        IDLE,           //Wander around, Stand still, etc
        SLEEP,          //Find a safe place and rest
        EAT,            //Find food to eat
        DRINK,          //Look for fresh water to drink
        REPRODUCE,      //Sexy time
        ENGAGED,        //Creature is busy....don't disturb
        ATTACK,         //In combat with someone/something
        HUNT,           //Go look for food to store at tribe
        FLEE,            //Danger near by, choose between Attack or Flee
        EXPLORE        //Look around the area for resources (Food) - Locations of food will be stored in the Population once they return
                       //      Will need a "Memory" of sorts, to unload all new stuff into Population memory, and take from Pop'n Memory etc
    }


    //Standard Public
    public bool updateStats = false;
    public bool reverseDirection = false;
    public bool rep_Cooldown = false;               //Reproduction cooldown toggle
    public bool threatNearby = false;              //Is there a hostile creature nearby (Carnivore/Omnivore/Other Tribe)
    public bool isHunter = false;                   //Is this creature a hunter?
    public bool isGatherer = false;                 //is this creature a gatherer?
    public float deltaT = 0.0f;

    //Trait/Attr Vars
    public Dictionary<string, float> attributes = new Dictionary<string, float>();
    public Dictionary<string, float> traits = new Dictionary<string, float>();
    public Dictionary<string, float> capacityTraits = new Dictionary<string, float>();
    //State/FSM Vars
    public BEHAVIOUR_STATE currentState = BEHAVIOUR_STATE.IDLE;
    public BEHAVIOUR_STATE previousState = BEHAVIOUR_STATE.NULL_STATE;
    public PRIORITY currentPriority = PRIORITY.QUALITY;
    public PRIORITY previousPriority = PRIORITY.QUALITY;

    //Monobehaviour Public
    public GameObject targetMate = null;
    public GameObject targetCreature = null;
    public GameObject targetWaterSource = null;
    public List<GameObject> offspring = new List<GameObject>();                 //List of the offspring waiting to be born
    public List<GameObject> potentialMates = new List<GameObject>();            //Search for potential mates once per period.

    //Custom Private
    private FiniteStateMachine<AIBehaviour> fsm;

    //Standard Private
    private bool alive = true;
    private List<string> deathCauses = new List<string>();
    private float lastUpdate = 0.0f;
    private float lastSecoundTime = 0.0f;         //Responsible for counting seconds
    [SerializeField]//DEBUG
    private int lastPregnancyTime = 0;       //Counter for pregnancy timer
    [SerializeField]//DEBUG
    private float pregPeriod = 0.0f;
    private int secondsCount = 0;           //Counts how many RL seconds have passed.
                                            //Used to determine age (Age is in "Periods" where 1 period is set in GlobalGEPSettings)

    //Monobheaviour Private
    private List<GameObject> waterSuppliesList = new List<GameObject>();         //List containing a local reference to all the known water supplies
                                                                                 //Will be added to the Population's list when found via Explore


    //Other stats to track for behaviour;
    //      - Safety, Food Supplies (in Population/Tribe), Water Availablility (Nearby - High Pop'n intellect may be able to store water), Pop'n Density (in area)
    //This is where the Quality Vs Survival traits come in. If all 4 above stats are satisfied, evolve Quality. Else look at strong, smart, resilient creatures


    public void Start()
    {
        CheckInitialStats();
        lastUpdate = Time.time;
        secondsCount = (int)System.Math.Round(GetComponent<Stats>().age * GlobalGEPSettings.ONE_TIME_PERIOD);

        pregPeriod = GetComponent<Stats>().traits["Gestation Period"];

        fsm = new FiniteStateMachine<AIBehaviour>();
        fsm.Configure(this, new Idle());

        GameObject waterRoot = GameObject.Find("WaterSourceContainer");

        for (int i = 0; i < waterRoot.transform.childCount; i++)
        {
            waterSuppliesList.Add(waterRoot.transform.GetChild(i).gameObject);
        }

    }

    public void Update()
    {
        if (GlobalGEPSettings.gameStatus != GlobalGEPSettings.GAME_STATE.RUNNING ||
            GetComponent<CreatureManager>().inSetup)
        {
            return;
        }

        if (!alive)
        {
            KillCreature();
        }
        //Have damage here too (IE ATTACK, HP etc)
    }

    public void FixedUpdate()
    {
        if (GlobalGEPSettings.gameStatus != GlobalGEPSettings.GAME_STATE.RUNNING ||
            GetComponent<CreatureManager>().inSetup)
        {
            lastUpdate = Time.time;
            return;
        }

        float secondDT = Time.time - lastSecoundTime;
        if (secondDT >= GlobalGEPSettings.ONE_SECOND)
        {
            secondsCount++;
            GetComponent<Stats>().age = (float)System.Math.Round(secondsCount / GlobalGEPSettings.ONE_TIME_PERIOD, 1);

            if (!GetComponent<Stats>().canMate)
            {
                if (traits["Reproductive Age"] > 0)
                {
                    if (GetComponent<Stats>().age >= traits["Reproductive Age"])
                    {
                        GetComponent<Stats>().canMate = true;
                    }
                }
                else
                {
                    if (GetComponent<Stats>().age >= traits["Life Expectancy"] * 0.5f)
                    {
                        GetComponent<Stats>().canMate = true;
                    }
                }
            }

            if (GetComponent<Stats>().pregnant)
            {
                lastPregnancyTime++;
            }

            if (currentState != BEHAVIOUR_STATE.ATTACK && currentState != BEHAVIOUR_STATE.HUNT)
            {
                GetComponent<Stats>().currHP += 1.0f * (attributes["constitution"] / 10);
            }

            lastSecoundTime = Time.time;
        }


        //Movement, Physics, Update stats etc
        deltaT = Time.time - lastUpdate;
        if (deltaT >= GlobalGEPSettings.STAT_UPDATE_PERIOD)
        {
            UpdateStats();
            CheckLifeState();
            updateStats = true;
        }

        //If whole number (no remainder)
        if (secondsCount % (GlobalGEPSettings.ONE_TIME_PERIOD * GlobalGEPSettings.MATING_PERIOD) == 0 && !GetComponent<Stats>().pregnant)
        {
            rep_Cooldown = false;

            //Refresh list once per period/Year
            if (GetComponent<Stats>().canMate)
            {
                GenerateMateList();
            }
        }

        //Once per year
        if (secondsCount % (GlobalGEPSettings.ONE_TIME_PERIOD) == 0)
        {
            //Check/Change priority once per period
            PRIORITY tempPriority = CheckPriority();
            if (tempPriority != currentPriority)
            {
                previousPriority = currentPriority;
                currentPriority = tempPriority;

                //On update priority, if previous one was one of the two below then remove a hunter/gatherer
                if (previousPriority == PRIORITY.FOOD)
                {
                    GetComponent<CreatureManager>().population.hunters--;
                }
                else if (previousPriority == PRIORITY.WATER)
                {
                    GetComponent<CreatureManager>().population.gatherers--;
                }
            }
        }


        if (GetComponent<Stats>().pregnant)
        {
            rep_Cooldown = true;
            if (lastPregnancyTime >= (GlobalGEPSettings.ONE_TIME_PERIOD * GetComponent<Stats>().traits["Gestation Period"]))
            {
                GiveBirth();
                lastPregnancyTime = 0;
                GetComponent<Stats>().pregnant = false;
            }
        }

        CheckBehaviourState();
        ExecuteBehaviour();
    }

    //Change FSM state (Behaviours)
    public void ChangeState(FSMState<AIBehaviour> state, BEHAVIOUR_STATE newState)
    {
        previousState = currentState;
        currentState = newState;
        fsm.ChangeState(state);
    }

    //Create a new list of potential mates
    public void GenerateMateList()
    {
        //Clear the current list, and create a new one each period (year)
        potentialMates.Clear();
        //Choose a list of 5 mates

        float vainCheck = GetComponent<CreatureManager>().population.averageVanity + (GetComponent<CreatureManager>().population.averageVanity * 0.25f);
        if (attributes["vanity"] >= vainCheck || currentPriority == PRIORITY.QUALITY)
        {
            for (int i = 0; i < GetComponent<CreatureManager>().population.population.Count; i++)
            {

                GameObject creature = GetComponent<CreatureManager>().population.population[i];
                if (potentialMates.Count >= 5)
                {
                    break;
                }
                //Only add a mate IF they;
                //      1. Are of age (CanMate),    2. Have completed setup,   3. Are not pregnant,   4. Give consent (Not in cooldown),   5. Creature already added
                //      6. Not already have mate,   7. Are not this creature
                if (!creature.GetComponent<Stats>().canMate ||
                    !creature.GetComponent<CreatureManager>().loadCreatureComplete ||
                    creature.GetComponent<Stats>().pregnant ||
                    creature.GetComponent<AIBehaviour>().rep_Cooldown ||
                    potentialMates.Contains(creature) ||
                    creature.GetComponent<AIBehaviour>().targetMate != null ||
                    creature.GetComponent<CreatureManager>().creatureIndex == GetComponent<CreatureManager>().creatureIndex)
                {
                    continue;
                }
                //If no suitable mate found - tough luck result in no reproduction
                if (creature.GetComponent<Stats>().attributes["charisma"] > GetComponent<CreatureManager>().population.averageCharisma)
                {
                    //Only add if is NOT this creature, is Single, or their Target mate is this creature already
                    if (creature.gameObject.GetComponent<CreatureManager>().creatureIndex != gameObject.GetComponent<CreatureManager>().creatureIndex
                        && (creature.GetComponent<AIBehaviour>().targetMate == null
                        || creature.GetComponent<AIBehaviour>().targetMate.GetComponent<CreatureManager>().creatureIndex == gameObject.GetComponent<CreatureManager>().creatureIndex))
                    {
                        potentialMates.Add(creature);
                    }
                }
            }
        }

        GameObject tempMate = null;
        float tempFit = Mathf.NegativeInfinity;

        for (int j = 0; j < 5; j++)
        {
            tempMate = null;
            tempFit = Mathf.NegativeInfinity;
            if (potentialMates.Count >= 5)
            {
                break;
            }
            //Cycle through each population member to compare their fitness wit heveryone elses 
            for (int i = 0; i < GetComponent<CreatureManager>().population.population.Count; i++)
            {
                GameObject creature = GetComponent<CreatureManager>().population.population[i];
                if (potentialMates.Count >= 5)
                {
                    break;
                }

                //Only add a mate IF they;
                //      1. Are of age (CanMate),    2. Have completed setup,   3. Are not pregnant,   4. Give consent (Not in cooldown),   5. Creature already added
                //      6. Not already have mate,   7. Are not this creature
                if (!creature.GetComponent<Stats>().canMate ||
                    !creature.GetComponent<CreatureManager>().loadCreatureComplete ||
                    creature.GetComponent<Stats>().pregnant ||
                    creature.GetComponent<AIBehaviour>().rep_Cooldown ||
                    potentialMates.Contains(creature) ||
                    creature.GetComponent<AIBehaviour>().targetMate != null ||
                    creature.GetComponent<CreatureManager>().creatureIndex == GetComponent<CreatureManager>().creatureIndex)
                {
                    continue;
                }

                float fitnessVal = 0.0f;
                //For each priority determine this mate's fitness
                //First check whether pop is migrating, then proceed as normal
                if (GetComponent<CreatureManager>().population.popState == POPULATION_STATE.MIGRATE)
                {
                    //Constitution
                    float con = creature.GetComponent<Stats>().attributes["constitution"];

                    fitnessVal = con;
                }
                //Physically fit individuals able to combat
                else if (currentPriority == PRIORITY.SAFETY)
                {
                    //Strength + constitution
                    float str = creature.GetComponent<Stats>().attributes["strength"];
                    float con = creature.GetComponent<Stats>().attributes["constitution"];

                    //If the strength value is lower than the constitution value,
                    //  Then halve the value.
                    //  Strength is more important, as such if it is lower than minor attribute, then
                    //      reduce strength in half so the weight has less/no effect.
                    if(str < con)
                    {
                        str *= 0.5f;
                    }

                    float strWeight = 2.0f;
                    float conWeight = 1.0f;

                    //Apply weightings to Str + const.
                    //      Weightings make one more important over the other
                    fitnessVal = (str * strWeight) + (con * conWeight);
                    fitnessVal /= 2;

                    if (fitnessVal > tempFit)
                    {
                        tempFit = fitnessVal;
                        tempMate = creature;
                    }
                }
                else if (currentPriority == PRIORITY.WATER)
                {
                    //Int
                    float intellect = creature.GetComponent<Stats>().attributes["intellect"];

                    fitnessVal = intellect;
                }
                else if (currentPriority == PRIORITY.FOOD)
                {
                    //Int + str
                    float intellect = creature.GetComponent<Stats>().attributes["intellect"];
                    float str = creature.GetComponent<Stats>().attributes["strength"];

                    if(intellect < str)
                    {
                        intellect *= 0.5f;
                    }

                    float intWeight = 2.0f;
                    float strWeight = 1.0f;

                    fitnessVal = (intellect * intWeight) + (str * strWeight);
                    fitnessVal /= 2;
                }

                if (fitnessVal > tempFit)
                {
                    tempFit = fitnessVal;
                    tempMate = creature;
                }
            }
        }
        if (potentialMates.Count < 5 && !potentialMates.Contains(tempMate) && tempMate != null)
        {
            //Add the most fit mate
            potentialMates.Add(tempMate);
        }
    }

    //Reproduce - Create new offspring if is possible
    public void Reproduce()
    {
        ChangeState(new Idle(), BEHAVIOUR_STATE.IDLE);
        rep_Cooldown = true;
        //If one or both mates are infertile, OR, this mate is male, OR, this mate is already pregnant
        if ((!GetComponent<Stats>().fertile || !targetMate.GetComponent<Stats>().fertile) || !GetComponent<Stats>().female || GetComponent<Stats>().pregnant)
        {
            return;
        }
        GetComponent<Stats>().pregnant = true;

        Population thisPopulation = GetComponent<CreatureManager>().population;
        GameObject newPop = Instantiate(thisPopulation.creatureObject);
        newPop.GetComponent<CreatureManager>().inSetup = true;
        newPop.GetComponent<CreatureManager>().population = thisPopulation;
        newPop.name = thisPopulation.speciesName.ToString() + " " + thisPopulation.population.Count + 1;
        newPop.transform.SetParent(thisPopulation.gameObject.transform);
        newPop.transform.localPosition = new Vector3(5000.0f, 5000.0f, 0);      //Hide away off screen
        newPop.GetComponent<CreatureManager>().replicationMethod = REPLICATION_METHOD.REPRODUCE;
        newPop.GetComponent<CreatureManager>().motherCreature = GetComponent<CreatureManager>();
        newPop.GetComponent<CreatureManager>().fatherCreature = targetMate.GetComponent<CreatureManager>();
        newPop.GetComponent<CreatureManager>().creatureIndex = thisPopulation.indexCount;

        int genIndex = thisPopulation.genIndices[GetComponent<CreatureManager>().creatureIndex] >= thisPopulation.genIndices[targetMate.GetComponent<CreatureManager>().creatureIndex]
                       ? GetComponent<CreatureManager>().creatureIndex : targetMate.GetComponent<CreatureManager>().creatureIndex;
        int generation = (thisPopulation.genIndices[genIndex]) + 1;

        thisPopulation.genIndices.Add(newPop.GetComponent<CreatureManager>().creatureIndex, generation);
        newPop.GetComponent<CreatureManager>().InitializeCreature();
        thisPopulation.population.Add(newPop);
        thisPopulation.indexCount++;
        offspring.Add(newPop);
    }

    //Check whether "In the mood" to reproduce (Safe, Fed, Watered, not in cooldown, not tired etc)
    public bool InTheMood()
    {
        bool inMood = traits["Energy Level"] >= (capacityTraits["Energy Level"] * 0.5f);
        if (!inMood)
        {
            return inMood;
        }
        inMood = traits["Food Level"] >= (capacityTraits["Food Level"] * 0.5f);
        if (!inMood)
        {
            return inMood;
        }

        inMood = traits["Water Level"] >= (capacityTraits["Water Level"] * 0.5f);

        inMood = !rep_Cooldown;      //Finally if has recently reproduced rep_cooldown will be false, otherwise it will be true

        return inMood;
    }

    //Called when creatures HP reaches 0. 
    //Only reaches 0 during combat (attacked)
    public void CreatureMurdered(int popIndex)
    {
        if (!GetComponent<CreatureManager>().population.popDeathCounter.ContainsKey(popIndex))
        {
            GetComponent<CreatureManager>().population.popDeathCounter.Add(popIndex, 1);       //First instance of this pop killing a tribe member
            GetComponent<CreatureManager>().population.popMurderTimer.Add(popIndex, 0);        //First instance, so add to Timer and start at 0
        }
        else
        {
            int deathCount = GetComponent<CreatureManager>().population.popDeathCounter[popIndex];
            deathCount++;
            GetComponent<CreatureManager>().population.popDeathCounter[popIndex] = deathCount; //Otherwise increment death by 1
            GetComponent<CreatureManager>().population.popMurderTimer[popIndex] = 0;           //Otherwise if not first, reset timer back to 0
        }

        alive = false;
        deathCauses.Add("Killed by creature from Population Index: " + popIndex);
    }

    public void FindWaterSources()
    {
        GameObject closestSource;
        float dist = Mathf.Infinity;
        for (int i = 0; i < waterSuppliesList.Count; i++)
        {
            float thisDist = Vector2.Distance(transform.position, waterSuppliesList[i].transform.position);
            if (thisDist < dist)
            {
                closestSource = waterSuppliesList[i];
                dist = thisDist;
                targetWaterSource = closestSource;
            }
        }
    }


    //Checks that the stats are valid (IE if negative growth, Kill creature etc)
    private void CheckInitialStats()
    {
        attributes = GetComponent<Stats>().attributes;
        traits = gameObject.GetComponent<Stats>().traits;
        capacityTraits = gameObject.GetComponent<Stats>().capacityTraits;

        foreach (KeyValuePair<string, float> trait in traits)
        {
            //To avoid this and make it more 'general' can add a "minimum value" field into Traits.cs 
            //  which the player can set when adding a trait, or can default to 0. Then compare min value to
            //  current value
            //This method would also allow to compare over time (IE if food dips to min level, kill creature) etc

            //Go through each trait and check whether they are fertile/alive etc
            for (int i = 0; i < TraitValues.TRAIT_NAME.Length; i++)
            {
                string traitName = TraitValues.TRAIT_NAME[i];

                if (trait.Key.ToLower() == traitName.ToLower())
                {
                    switch (i)
                    {
                        case 5:
                        case 6:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                            if (trait.Value <= 0)
                            {
                                alive = false;
                                deathCauses.Add(traitName + " less than 0");
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                }
            }
        }
    }

    //Check the creature's stats + priorities to see if they need to change
    private void CheckBehaviourState()
    {
        //Check stats + priorities
        //If priorities/Stats change, change FSM
        //Dont forget to change "currentState" either!
        //Priotity Order:
        //      1. SAFETY           2. WATER            3. FOOD             4. SPACE

        //First check if traits are ok

        BEHAVIOUR_STATE targetState = currentState;

        //If current "Engaged" (Actively reproducing etc) do not change state
        if (currentState == BEHAVIOUR_STATE.ENGAGED)
        {
            return;
        }

        if (GetComponent<Stats>().canMate && InTheMood())        //3 core stats below are at 75% or higher AND is of age
        {
                targetState = BEHAVIOUR_STATE.REPRODUCE;
        }
        else
        {
            float currentVal = traits["Energy Level"];
            if (currentVal <= capacityTraits["Energy Level"] * 0.15)
            {
                targetState = BEHAVIOUR_STATE.SLEEP;
            }

            currentVal = traits["Food Level"];
            if (currentVal <= capacityTraits["Food Level"] * 0.15)
            {
                targetState = BEHAVIOUR_STATE.EAT;
            }

            currentVal = traits["Water Level"];
            if (currentVal <= capacityTraits["Water Level"] * 0.25)
            {
                targetState = BEHAVIOUR_STATE.DRINK;
                //Only override IF is less than Water val and in immindent danger - prevent lfip flopping
                if (traits["Food Level"] < currentVal)
                {
                    if (traits["Food Level"] <= capacityTraits["Food Level"] * 0.15f)
                        targetState = BEHAVIOUR_STATE.EAT;
                }
            }
        }

        float energyVal = traits["Energy Level"];
        if (energyVal <= 0)
        {
            traits["Energy Level"] = 0.0f;
            targetState = BEHAVIOUR_STATE.SLEEP;
        }

        //Check make sure the water + food levels aren't at dangerous levels
        if (GetComponent<CreatureManager>().population.averageIntellect >= GlobalGEPSettings.MIN_WATER_STORE_INTELLECT &&
            GetComponent<CreatureManager>().population.popWaterStores.currentAmount <= 0.20f)
        {
            currentPriority = PRIORITY.WATER;
        }
        else if (GetComponent<CreatureManager>().population.popFoodStores.currentAmount <= 0.20f)
        {
            currentPriority = PRIORITY.FOOD;
        }

        //Then check the Priorities. Do space first...check POPULATION class for space avialability (Could ignore this for first prototype)

        //Do Food Storage, Water Storage and Space availability Checks here
        //PRIORITY tempPriority = CheckPriority();

        //For Fitness evaluation, keep the fitness in that state for 1 period (otherwise it wll be constantly changing)
        //      Possibly a setting for "sensitivity"?


        //This focuses on CONSTITUTION ATTRIBUTE for reproduction/Fitness/Attractiveness etc
        //This focus will be linked directly with the Population class that has a Population_Status variable, tracking whether the pop is colonizing
        //      or not. If colonizing then the population may only be in "SPACE" Priority 
        //Extremely Vain creatures will still take Charisma into account (IE if Vanity >= averageVanity + 25-50%, add Charisma to the fitness calculations)
        if (GetComponent<CreatureManager>().population.popState == POPULATION_STATE.MIGRATE)
        {
            //Explore for a new population location
            //      A random number of creatures from the population (between 25-50%) will split off and create a new population centre
            //      They will then choose a random location a minimum distance away from the original location (AILERP)
            //      They will not Hunt, or gather until a new location is found
            //          If not enough space will never find a new location and will eventually starve
            //          Alternatively, any other population becomes an enemy so may attack
            //              If this is the case, will need to make sure they don't attack original population (and vice versa) 
            //                  Possibly add a check of "immunity" for 1 period, or until far enough away
            //                      During immunity, either ALL species are non-threat (including food so will not hunt them)
            //                      OR can set it so only their own species is a non-threat (will have to program additional check if they can hunt yet etc)
            //                      But can be attacked and killed whilst searching elsewhere if this is the case.
            //                      This priority will be active and be linked with the population until the pop has settled.
            //                      
            //When inthis state, a creature is part of a "New Population" (colony) and is preparing to move out
            //Each pop will set their energy, food and water levels to max, and then head out.
            //Whilst in this state, they are moving, and if one stops to sleep, all stop to sleep. Same with eat + drink until food runs out.
            //Can still mate en-route to new location, and will look at the constitution attribute
            //Check if there is sufficient time to live (IE minimum of 1.5-2 periods)

            //If is in Space mode/Migrating, then make population.population[0] act as the leader/alpha, and have every other population follow them
            //      Swarm behaviour? (0<-1-<-2<-3<-4....) if x distance from 0, start a new column?
            //                      OR move randomly, but when gets certain distance from Alpha, make goal to move to their last position (AILERP)
            //                              then continue random movement etc


            //Still need to check hunger etc - but apply Swarm AI behaviours
            energyVal = traits["Energy Level"];
            if (energyVal <= 0)
            {
                traits["Energy Level"] = 0.0f;
                targetState = BEHAVIOUR_STATE.SLEEP;
            }
        }
        else
        {
            //This focuses on STRENGTH/CONSTITUTION ATTRIBUTES for reproduction/Fitness/Attractiveness etc
            //Extremely Vain creatures will still take Charisma into account (IE if Vanity >= averageVanity + 25-50%, add Charisma to the fitness calculations)
            if (currentPriority == PRIORITY.SAFETY)
            {
                //Determine Fight vs Flight
                //      Use Wisdom + intellect to compare their strength with the average strenght of population
                //          If one of the strongest members in the population, more likely to fight
                //          If one ofthe weaker members, more likely to flee
                //          If has low wisdom or intellect, AND one of weaker members, increased chance they will fight (despite risk of losing)
                //                  If wins...increase their Charisma???
                //          If has low wisdom/intellect, And one of stronger members, possibly increased chance will flee? (Wisdom/Intellect acting as 'courage' etc)

                //Could combine ATTACK/FLEE states into one substate ("COMBAT") which then divides into ATTACK/FLEE?
                //      Potentially more optimized
                //      Could perform these fight vs flight checks in there rather than in AI behaviour

                //Check if there is a threat near by (IE another pop within a certain distance AND is Omnivore/Carnivore)
                if (threatNearby)
                {
                    float wisdom = GetComponent<Stats>().attributes["wisdom"];
                    float intellect = GetComponent<Stats>().attributes["intellect"];
                    float strength = GetComponent<Stats>().attributes["strength"];
                    //If one of the strongest in the population then fight
                    //Otherwise, if Wisdom * Intellect (/10) added to strength is STILL less than average strength, Fight
                    //Otherwise flee
                    //          IE W=30, I=25, S=8. Avg = 15.
                    //          (3 * 2.5) + 8 = 15.5
                    //                  Results in Fleeing (smart enough to know not strong enough)
                    //      
                    //          IE W=10, I=25, S=8. Avg = 15
                    //          (1 * 2.5) + 8 = 10.5
                    //                  Results in fighting (Too irrational/not smart enough to realize weaker and less chance of winning etc)
                    bool fightFlight = strength >= GetComponent<CreatureManager>().population.averageStrength ? true :
                                        ((wisdom / 10) * (intellect / 10) + strength) <= GetComponent<CreatureManager>().population.averageStrength ? true : false;

                    if (fightFlight)
                    {
                        targetState = BEHAVIOUR_STATE.ATTACK;
                    }
                    else
                    {
                        //Temporarily set to Attack. Eventually will be Flee
                        //  This will need to have it so other pop members come to help if in range (which requires 'Update Priorities' for Safety to work
                        //      Regarding checking nearby threats (Detection radius) 
                        //  During fleeing, use 2x energy
                        targetState = BEHAVIOUR_STATE.ATTACK;
                    }
                }
                //Cause creature to pass out from exhaustion, regardless of the other stats
                //Difference between this and the first is the first instance of energy level is by the creature's choice
                //This will be forced on the entity regardless and cannot be overridden (aside from imminent danger - ADRENALINE!)
                //  Will only "pass out" if energy is at negative capacity (absolutely no energy that even adrenaline does nothing)
                //      Can result in being killed if still being pursued
                energyVal = traits["Energy Level"];
                if (energyVal <= 0 && energyVal >= -capacityTraits["Energy Level"])
                {
                    traits["Energy Level"] = 0.0f;
                    targetState = BEHAVIOUR_STATE.FLEE;
                }
                else if (energyVal < -capacityTraits["Energy Level"])
                {
                    traits["Energy Level"] = capacityTraits["Energy Level"];
                    targetState = BEHAVIOUR_STATE.SLEEP;
                }
            }
            //This focuses on INTELLECT ATTRIBUTE for reproduction/Fitness/Attractiveness etc
            //Extremely Vain creatures will still take Charisma into account (IE if Vanity >= averageVanity + 25-50%, add Charisma to the fitness calculations)
            else if (currentPriority == PRIORITY.WATER)
            {
                //Find fresh water and/or refille supplies
                //Drink FSM will use water sources but find it's own if null
                //This priority will update list of water sources before assigning this individual to gather water

                //Only assign a hunter if current water level is lower than 80%
                //  AND if there are less than 5 gatherers
                //  Otherwise allow them to do whatever they were doing before
                if (GetComponent<CreatureManager>().population.popWaterStores.currentAmount < 0.80f && GetComponent<CreatureManager>().population.gatherers < 2)
                {
                    if (targetState != BEHAVIOUR_STATE.EAT && targetState != BEHAVIOUR_STATE.DRINK)
                    {
                        //Change to GATHER - Gather = Gather up water 
                        FindWaterSources();
                        //targetState = BEHAVIOUR_STATE.HUNT;
                    }
                }

                energyVal = traits["Energy Level"];
                if (energyVal <= 0)
                {
                    traits["Energy Level"] = 0.0f;
                    targetState = BEHAVIOUR_STATE.SLEEP;
                }
            }

            //This focuses on INTELLECT/STRENGTH ATTRIBUTES for reproduction/Fitness/Attractiveness etc
            //Extremely Vain creatures will still take Charisma into account (IE if Vanity >= averageVanity + 25-50%, add Charisma to the fitness calculations)
            else if (currentPriority == PRIORITY.FOOD)
            {
                //Find Food sources/Hunt and/or gather more food
                //  If this creature is not already eating, and not too many hunters, assign this pop to a hunter
                //HERBIVORES DO NOT HUNT - no need to store food because can just eat from the ground
                //          Later on could add in Trees/fruits etc that they can gather

                //Hunting:  
                //          1. Find nearest population (ideally threatening/enemy)
                //          2. Find nearest creature from that population
                //          3. Move to that creature
                //          4. Start attacking creature
                //          5. If victorious return to pop centre with food

                //Only assign a hunter if current food level is lower than 80%
                //  AND if there are less than 5 hunters
                //  Otherwise allow them to do whatever they were doing before
                float val = GetComponent<CreatureManager>().population.hunters;
                float amt = GetComponent<CreatureManager>().population.popFoodStores.currentAmount;
                if (GetComponent<CreatureManager>().population.popFoodStores.currentAmount < 0.60f && GetComponent<CreatureManager>().population.hunters < 2)
                {
                    //Has minimum HP to right AND not pregnant
                    if (GetComponent<Stats>().currHP >= GetComponent<Stats>().maxHP * 0.25f && !GetComponent<Stats>().pregnant)
                    {
                        //Is at least of average strength
                        if (attributes["strength"] >= (GetComponent<CreatureManager>().population.averageStrength - (GetComponent<CreatureManager>().population.averageStrength * 0.25f)))
                        {
                            isHunter = true;
                            GetComponent<CreatureManager>().population.hunters++;
                        }
                    }

                }
                else if (GetComponent<CreatureManager>().population.popFoodStores.currentAmount >= 0.65f && isHunter)
                {
                    isHunter = false;
                    GetComponent<CreatureManager>().population.hunters--;
                }

                if (isHunter)
                {
                    if (targetState != BEHAVIOUR_STATE.EAT && targetState != BEHAVIOUR_STATE.DRINK)
                    {
                        targetState = BEHAVIOUR_STATE.HUNT;
                    }
                }

                energyVal = traits["Energy Level"];
                if (energyVal <= 0)
                {
                    traits["Energy Level"] = 0.0f;
                    targetState = BEHAVIOUR_STATE.SLEEP;
                }
            }

            //This focuses on Vanity attribute, ignoring all Survival attributes (even if REALLY good). The population is complacent and cares about 
            //      physical attractiveness of mates rather than practical/survival attractiveness of mates
            else if (currentPriority == PRIORITY.QUALITY)
            {
                //Do whatever - Don't change the FSM/Activity, just change the Fitness evaluation
                //Check that all needs are met (as with all other priorities where relevant)
            }

        }
        //Change the current state
        //Defaults to Idle

        if (currentState != targetState)
        {
            //Force an update to the creature window if its open
            updateStats = true;
            switch (targetState)
            {
                case BEHAVIOUR_STATE.SLEEP:
                    ChangeState(new Sleep(), BEHAVIOUR_STATE.SLEEP);
                    break;
                case BEHAVIOUR_STATE.EAT:
                    ChangeState(new Eat(), BEHAVIOUR_STATE.EAT);
                    break;
                case BEHAVIOUR_STATE.DRINK:
                    ChangeState(new Drink(), BEHAVIOUR_STATE.DRINK);
                    break;
                case BEHAVIOUR_STATE.REPRODUCE:
                    ChangeState(new Reproduce(), BEHAVIOUR_STATE.REPRODUCE);
                    break;
                case BEHAVIOUR_STATE.HUNT:
                    ChangeState(new Hunt(), BEHAVIOUR_STATE.HUNT);
                    break;
                case BEHAVIOUR_STATE.EXPLORE:
                    ChangeState(new Sleep(), BEHAVIOUR_STATE.EXPLORE);
                    break;
                case BEHAVIOUR_STATE.ATTACK:
                    ChangeState(new Attack(), BEHAVIOUR_STATE.ATTACK);
                    break;
                case BEHAVIOUR_STATE.FLEE:
                    ChangeState(new Sleep(), BEHAVIOUR_STATE.FLEE);
                    break;
                default:
                    ChangeState(new Idle(), BEHAVIOUR_STATE.IDLE);
                    break;
            }
        }
    }

    //Check whether creature is alive
    private void CheckLifeState()
    {
        foreach (KeyValuePair<string, float> trait in traits)
        {

            //To avoid this and make it more 'general' can add a "minimum value" field into Traits.cs 
            //  which the player can set when adding a trait, or can default to 0. Then compare min value to
            //  current value
            //This method would also allow to compare over time (IE if food dips to min level, kill creature) etc

            //Go through each trait and check whether they are fertile/alive etc
            for (int i = 0; i < TraitValues.TRAIT_NAME.Length; i++)
            {
                string traitName = TraitValues.TRAIT_NAME[i];

                if (trait.Key.ToLower() == traitName.ToLower())
                {
                    switch (i)
                    {
                        //Only food and water Kill                    
                        case 11:
                        case 13:
                            if (trait.Value <= 0)
                            {
                                alive = false;
                                deathCauses.Add(traitName + " less than 0");
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                }
            }
        }

        if (GetComponent<Stats>().currHP <= 0)
        {
            alive = false;
            deathCauses.Add("HP less than 0");
        }
    }

    //Kill this creature
    private void KillCreature()
    {
        if (GetComponent<Stats>().age < 0.1f)
        {
            Debug.Log("Died On Spawn/very young");
        }

        GetComponent<CreatureManager>().population.population.Remove(gameObject);

        if (isHunter)
            GetComponent<CreatureManager>().population.hunters--;

        if (isGatherer)
            GetComponent<CreatureManager>().population.gatherers--;

        if (GetComponent<CreatureManager>().population.genIndices.ContainsKey(GetInstanceID()))
            GetComponent<CreatureManager>().population.genIndices.Remove(GetInstanceID());

        if (GetComponent<CreatureManager>().motherCreature != null)
        {
            //if this creature still has a living mother, then decrement her child count (only represents living chilren)
            GetComponent<CreatureManager>().motherCreature.GetComponent<Stats>().children.Remove(gameObject);
            GetComponent<CreatureManager>().motherCreature.GetComponent<Stats>().childrenCount--;
        }

        Destroy(gameObject);
    }

    //Update stats (perform consumption)
    private void UpdateStats()
    {
        //Update Food, Water, Energy levels etc
        //Energy Levels

        float currentLevel = traits["Energy Level"];
        float currentCons = traits["Energy Consumption"];
        if (currentState != BEHAVIOUR_STATE.SLEEP)
        {
            currentLevel -= (currentCons * deltaT);
            traits["Energy Level"] = currentLevel;
        }

        currentLevel = traits["Food Level"];
        currentCons = traits["Food Consumption"];
        currentLevel -= (currentCons * deltaT);
        traits["Food Level"] = currentLevel;

        currentLevel = traits["Water Level"];
        currentCons = traits["Water Consumption"];
        currentLevel -= (currentCons * deltaT);
        traits["Water Level"] = currentLevel;

        if (IsOfAge())
        {
            GetComponent<Stats>().canMate = true;
        }




        float HP = 0.0f;
        //HP is result of Str + Const, minus percentage of HP based on age
        //      If Age = life expectancy then HP will be 0 (Cos(0) = 1, HP * 1 = HP. HP-HP = 0 = death)
        HP = (attributes["strength"] + attributes["constitution"]);
        HP -= (HP * Mathf.Cos(traits["Life Expectancy"] - GetComponent<Stats>().age));
        GetComponent<Stats>().maxHP = HP;


        if (GetComponent<Stats>().currHP > HP)
        {
            GetComponent<Stats>().currHP = HP;
        }

        gameObject.GetComponent<Stats>().traits = traits;
        lastUpdate = Time.time;
    }

    //Get the priority of the Creature (Food, Water, Safety, Space, Quality etc)
    //Originally was going to be population wide, but that would not work (if set to Safety for everyone even though only 1 is in danger, then 
    //      could cause entire population to flee despite danger being 1000m away etc
    private PRIORITY CheckPriority()
    {
        Population thisPopulation = GetComponent<CreatureManager>().population;

        //Safety
        //Ultimately perform the actual check for nearby threats here
        //      Could classify different populations as threats
        //      Create list in the population for "ThreateningPopulations"
        //
        //      Then if lots of deaths happening due to them, add that population to the list (possibly add in a Populationindex similar to creature index)
        //              See GlobalGEPSettings.Pop_Threat_THreshold for ideas on how to further develop this!
        //
        //      Otherwise if few deaths, declassify as a threat and ignore them
        //      This will allow the system to be more malleable for instance if Herbivores evolve ability to fight back
        //      If they do, after previously being a threat, and start killing populations, they can be readded automatically and populations react to them

        //Update the list of Threats
        foreach (KeyValuePair<int, int> hostilePop in thisPopulation.popDeathCounter)
        {
            //See GlobalGEPSettings.Pop_Threat_THreshold for ideas on how to further develop this!
            //if the number of kills is more than or equal to the threshold
            if (hostilePop.Value > GlobalGEPSettings.POP_THREAT_THRESHOLD)
            {
                float threatPeriod = thisPopulation.popMurderTimer[hostilePop.Key];
                int basePeriod = (int)threatPeriod;
                threatPeriod -= (threatPeriod * 0.25f);

                //And if the time since the last encounter/murder is less than the threatPeriod - 25%
                //      This makes it so that tensions are there, but no conflict (Cold war type thing)
                //      But if there is one attack they are instantly added back to the list)
                //          Keep them on the list anyway, but don't flee from them or attack on sight??????
                if (basePeriod <= (int)threatPeriod)
                {
                    thisPopulation.threateningPops.Add(hostilePop.Key);
                }
                else
                {
                    //Otherwise if this key exists, then remove it from the listof active threats
                    if (thisPopulation.threateningPops.Contains(hostilePop.Key))
                    {
                        thisPopulation.threateningPops.Remove(hostilePop.Key);
                    }
                }
            }
        }

        //Determine if threat nearby - Use a trigger circle set to range 5 etc (later set to be detection radius via genetics)
        //Issue is that this trigger will trigger on UIInput.cs as well.

        if (threatNearby)
        {
            return PRIORITY.SAFETY;
        }
        //For safety: Check how many deaths have happened in the population due to carnivores
        //              Then check if there are any carnivores nearby (possibly have a second trigger circle to detect carnivores/omnivores)
        //              If lots of deaths, but no Carnies nearby, still set safety concern              
        //
        //              If lots of deaths, but none near by, Moderate safety    -   Will reproduce
        //              If few deaths and none nearby, total safety             -   Will reproduce
        //              If lots of deaths and nearby carnies, UNSAFE            -   Won't eat, drink, sleep, reproduce, Explore, or Gather
        //              if few deaths, but carnies nearby, Moderate safety      -   Won't reproduce

        //Water
        if (thisPopulation.averageIntellect >= GlobalGEPSettings.MIN_WATER_STORE_INTELLECT)
        {
            if (thisPopulation.popWaterStores.currentAmount <= 0.15 && thisPopulation.gatherers < 5)
            {
                thisPopulation.gatherers++;
                return PRIORITY.WATER;
            }
        }

        //Food
        if (thisPopulation.dietType != Population.DIET_TYPE.HERBIVORE)
        {
            if (thisPopulation.popFoodStores.currentAmount <= 0.15 && thisPopulation.hunters < 5)
            {
                return PRIORITY.FOOD;
            }
        }
        //Space
        //      Space will not be determined here, but rather by the Population as a whole
        //          in the Population class. Otherwise each member of the population will create a new migrating population
        //          rather than just 1 (decimating entire populations)

        return PRIORITY.QUALITY;
    }

    //Execute the current FSM state
    private void ExecuteBehaviour()
    {
        fsm.Update();
    }

    //Check whether creature is of age
    private bool IsOfAge()
    {
        bool ofAge = false;
        if (!GetComponent<Stats>().fertile)
        {
            ofAge = GetComponent<Stats>().age >= (traits["Life Expectancy"] * 0.5f) ? true : false;
        }
        else
        {
            ofAge = GetComponent<Stats>().age >= (traits["Reproductive Age"]) ? true : false;
        }
        return ofAge;
    }

    private void GiveBirth()
    {
        for (int i = 0; i < offspring.Count; i++)
        {
            if (offspring[i] == null)
            {
                Debug.Log("Error: Offspring not found");
                continue;
            }
            //Add to list of born children, and increment total living child count
            GetComponent<Stats>().children.Add(offspring[i]);
            GetComponent<Stats>().childrenCount++;
            //Set the new location, allow to perform normally

            offspring[i].transform.localPosition = gameObject.transform.localPosition;
            offspring[i].GetComponent<CreatureManager>().inSetup = false;
        }
        //Remove all offspring from list. All have been born now
        //Don't remove one by one otherwise will affect the for loop (i++ whilst removing from list, decreasing offspring.Count value)
        offspring.Clear();
    }
}