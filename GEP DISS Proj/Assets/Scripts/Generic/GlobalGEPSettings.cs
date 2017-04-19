
#define DEBUG_MODE       //Debug mode
//#undef DEBUG_MODE      //Not debug mode

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GlobalGEPSettings : MonoBehaviour
{
    //////////////////////////////////////////
    ////////////    GAME SETTINGS   //////////
    //////////////////////////////////////////
    public enum GAME_STATE
    {
        START,
        RUNNING,
        PAUSED,
        EXIT
    }
    //Current game state
    public static GAME_STATE gameStatus = GAME_STATE.RUNNING;
    public static float MOVE_SENSITIVITY = 25.0f;            //How quickly the camera will pan over the map 
    

    //////////////////////////////////////////
    /////////    Creature SETTINGS   /////////
    //////////////////////////////////////////
    public static float INIT_AGE_MULTIPLIER = 0.5f; //%age of life expectancy to age the individuals (IE to prevent long waits for reproduction)
    public static float THREAT_DETECTION_RANGE = 5.0f;  //Range at which threats are detected by creatures in a population  
                                                        //  Can later be developed to be determined by genetics (IE Sight, Hearing, Smell etc)


    //////////////////////////////////////////
    ////////    Population SETTINGS   ////////
    //////////////////////////////////////////
    public static bool POP_SPECIFIC_CONSUMPTION = false;    //Food Quality linked to entire population or just creature (for use with intellect etc)
    public static float STARTING_FOOD_AMOUNT = 0.5f;    //How much food a population will start with on spawn. Higher numbers = easy, lower = harder...Percentage
    public static int RESOURCE_CAPACITY_MULT = 10;       //Current Pop * Multiplier to determine max amount of food a pop can store
    public static float START_FOOD_QUALITY = 1.0f;    //What quality of food will the populations start with on spawn
    public static float PRIOTIY_MARGIN = 0.30f;     //At which point does the creatures in a population consider their hunger etc stats a priority vs not a priority

    public static float MIN_WATER_STORE_INTELLECT = 50;     //Minimum average intellect for a population before they can start storing water (instead of going to water each time)
    public static float MIN_FARMING_INTELLECT = 20;         //Minimum average intellect before population starts farming (generating food) - OMNIVORES ONLY 
    public static int POP_THREATENING_PERIOD = 5;           //Time it takes before a population is no longer considered a threat
    public static int POP_THREAT_THRESHOLD = 5;             //How many pop members a pop needs to kill before considered a threat
                                                            //Could eventually base this off of population's relative strengths also
                                                            //      IE much stronger (or larger?) pop attacking = instant threat
                                                            //      Likewise, if that smaller pop attacked instead, would take much more
                                                            //          Threat-Fitness evaluation?

    //////////////////////////////////////////
    ////////////    TIME SETTINGS   //////////
    //////////////////////////////////////////
    //Game - Time Settings
    public const float ONE_SECOND = 1.0f;           //Will always be 1 second (real seconds). Different from STAT_UPDATE_PERIOD as that can change
    public static float ONE_TIME_PERIOD = 100.0f;   //How many RL seconds in 1 period
                                                    //100 seconds = 1 period (where 1 period is 1 "year" off the hoomanses lifetime)
    public static float TIME_SPEED_MULT = 1.0f;     //Speed/slow down time (0.0 = pause, 0.25, 0.5, 1.0, 1.5, 2.0, 4.0)
    public static float STAT_UPDATE_PERIOD = 1.0f;  //How many seconds between updating stats
    public static float MATING_PERIOD = 0.25f;      //Fraction of ONE_TIME_PERIOD before next mating cycle begins




    //////////////////////////////////////////
    ////////////    GEP SETTINGS   ///////////
    //////////////////////////////////////////

    //GEP - Genetic Operator Settings
    public static float GEN_OP_RATE = 0.25f;    //Likelihood a genetic operator algorithm is applied
    public static float[] GO_CHANCES = new float[3] //Array for the chance of each operator that may happen
    {   
        1.0f,       //Mutation Operator Chance
        1.0f,       //Inversion Operator Chance
        1.0f        //Transposition Operator Chance
    };
    public static int MAX_MUT_ITERATIONS = 3;       //Maximum number of times the Mutation operator can be applied to 1 gene
    public static int MIN_INVERSION_LENGTH = 2;     //Minimum number of characters to be inverted - if n = 1, then no change will happen
    public static int MAX_INVERSION_LENGTH = 4;     //Maximum number of characters to be inverted
    public static int MIN_TRANSPOSE_LENGTH = 2;     //Minimum number of characters to be inserted during a transposition 
    public static int MAX_TRANSPOSE_LENGTH = 4;     //Maximum number of characters to be inserted during a transposition

    public static float MIN_ENERGY_LEVEL = 2.5f;    //The minimum level that the energy level trait may be
    public static float MIN_FOOD_LEVEL   = 2.5f;    //The minimum level that the food level trait may be
    public static float MIN_WATER_LEVEL  = 2.5f;    //The minimum level that the water level trait may be


    //GEP - Genome Settings
    public const int GENOME_LENGTH = 23;
    public const int CHROMOSOME_LENGTH = 6;
    public static int MAX_GENES_PER_TRAIT = 10; //Maximum number of genes per trait (can be increased or decrease
                                                //Used to arbitrarily set the size of the jaggedArrays in TraitList.cs "traitIndices" then remove "null" elements
    public static float A_VAL = 1.0f;           //The value of the A Const/variable in expressionTrees
    public static float B_VAL = 2.0f;           //The value of the B const/variable in expressionTrees
    public static bool FIXED_GENE_LENGTH = true;    //Is the length of genes fixed - Used in GeneticOperators() - Transposition
    public static bool TRAITS_ON_GENES = false;  //Are the traits on the genes or chromosomes
    public static bool RANDOMIZED_TRAITS = false;   //Randomized = Traits will have different layouts between individuals
                                                    //Non-Random = Traits will have the same (or similar - ignoring mutations) layouts


    //GEP - Trait List - Preset layouts For when "RANDOMIZED_TRAITS" is false
    public static Dictionary<string, Dictionary<string, int[][]>> speciesTraitLayouts = new Dictionary<string, Dictionary<string, int[][]>>();
}
