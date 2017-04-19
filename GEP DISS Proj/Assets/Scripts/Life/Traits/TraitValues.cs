using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraitValues
{
    //Generic Trait Const
    public const int TRAIT_COUNT = 21;

    //Gene Counts
    public static readonly int[] GENE_COUNT =
    {
        3,      //EYE_LEFT_GENES
        3,      //EYE_RIGHT_GENES
        1,      //EYE_STYLE_GENES       -- Determines whether eyes have matching or different colours
        3,      //HAIR_GENES
        3,      //SKIN_GENES
        4,      //GROWTH_RATE_GENES
        4,      //LIFE_EXPECTANCY_GENES
        2,       //REPRODUCTIVE_AGE_GENES
        2,       //GESTATION_PERIOD_GENES
        3,       //ENERGY_LEVEL_GENES
        3,       //ENERGY_CONS_GENES
        3,       //FOOD_LEVEL_GENES
        2,       //FOOD_CONS_GENES
        3,       //WATER_LEVEL_GENES
        2,       //WATER_CONS_GENES
        3,       //STRENGTH_GENES
        3,       //INTELLECT_GENES
        3,       //CONSTITUTION_GENES
        3,       //WISDOM_GENES
        3,       //CHARISMA_GENES
        3       //VANITY_GENES
    };

    public const int EYE_GENES = 3;
    public const int HAIR_GENES = 3;
    public const int SKIN_GENES = 3;
    public const int GROWTH_RATE_GENES = 4;
    public const int LIFE_EXPECTANCY_GENES = 4;
    public const int REPRODUCTIVE_AGE_GENES = 2;
    public const int GESTATION_PERIOD_GENES = 2;

    //Trait IDs
    public static readonly int[] TRAIT_IDS =
    {
        0,      //EYE_LEFT_ID
        1,      //EYE_RIGHT_ID
        2,      //EYE_STYLE_ID
        3,      //HAIR_ID
        4,      //SKIN_ID
        5,      //GROWTH_RATE_ID
        6,      //LIFE_EXPECTANCY_ID
        7,      //REPRODUCTIVE_AGE_ID
        8,      //GESTATION_PERIOD_ID
        9,      //ENERGY_LEVEL_ID
        10,     //ENERGY_CONS_ID
        11,     //FOOD_LEVEL_ID
        12,     //FOOD_CONS_ID
        13,     //WATER_LEVEL_ID
        14,     //WATER_CONS_ID
        15,     //STRENGTH_ID
        16,     //INTELLECT_ID
        17,     //CONSTITUTION_ID
        18,     //WISDOM_ID
        19,     //CHARISMA_ID
        20     //VANITY_ID
    };

    public const int EYE_ID = 0;
    public const int HAIR_ID = 1;
    public const int SKIN_ID = 2;
    public const int GROWTH_RATE_ID = 3;
    public const int LIFE_EXPECTANCY_ID = 4;
    public const int REPRODUCTIVE_AGE_ID = 5;
    public const int GESTATION_PERIOD_ID = 6;

    //Trait Abbreviations
    public static readonly string[] TRAIT_ABB =
    {
        "EL",       //EYE_LEFT_ABB
        "ER",       //EYE_RIGHT_ABB
        "ES",       //EYE_STYLE_ABB
        "HC",       //HAIR_ABB
        "SC",       //SKIN_ABB
        "GR",       //GROWTH_RATE_ABB
        "LE",       //LIFE_EXPECTANCY_ABB
        "RA",       //REPRODUCTIVE_AGE_ABB
        "GP",       //GESTATION_PERIOD_ABB
        "EN",       //ENERGY_LEVEL_ABB       -- EN = ENergy...
        "EC",       //ENERGY_CONS_ABB
        "FL",       //FOOD_LEVEL_ABB
        "FC",       //FOOD_CONS_ABB
        "WL",       //WATER_LEVEL_ABB
        "WC",       //WATER_CONS_ABB
        "STR",      //STRENGTH_ABB
        "INT",      //INTELELCT_ABB
        "CON",      //CONSTITUTION_ABB
        "WIS",      //WISDOM_ABB
        "CHA",      //CHARISMA_ABB
        "VAN"      //VANITY_ABB
    };

    public const string EYE_LEFT_ABB = "EL";
    public const string EYE_RIGHT_ABB = "ER";
    public const string EYE_STYLE_ABB = "ES";
    public const string HAIR_ABB = "HC";
    public const string SKIN_ABB = "SC";
    public const string GROWTH_RATE_ABB = "GR";
    public const string LIFE_EXPECTANCY_ABB = "LE";
    public const string REPRODUCTIVE_AGE_ABB = "RA";
    public const string GESTATION_PERIOD_ABB = "GP";

    //Trait Names
    public static readonly string[] TRAIT_NAME =
    {
        "Eye Left Colour",          //EYE_LEFT_NAME
        "Eye Right Colour",         //EYE_RIGHT_NAME
        "Eye Style",                //EYE_STYLE         -- Determines whether the individual has the same or different coloured eyes
        "Hair Colour",              //HAIR_NAME
        "Skin Colour",              //SKIN_NAME
        "Growth Rate",              //GROWTH_RATE_NAME
        "Life Expectancy",          //LIFE_EXPECTANCY_NAME
        "Reproductive Age",         //REPRODUCTIVE_AGE_NAME
        "Gestation Period",         //GESTATION_PERIOD_NAME
        "Energy Level",             //ENERGY_LEVEL_NAME
        "Energy Consumption",       //ENERGY_CONSUMPTION_NAME
        "Food Level",               //FOOD_LEVEL_NAME
        "Food Consumption",         //FOOD_CONSUMPTION_NAME
        "Water Level",              //WATER_LEVEL_NAME
        "Water Consumption",        //WATER_CONSUMPTION_NAME
        "Strength",                 //STRENGTH_NAME
        "Intellect",                //INTELLECT_NAME
        "Constitution",             //CONSTITUTION_NAME
        "Wisdom",                   //WISDOM_NAME
        "Charisma",                 //CHARISMA_NAME
        "Vanity"                    //VANITY_NAME
    };

    public const string EYE_LEFT_NAME = "Eye Left Colour";
    public const string EYE_RIGHT_NAME = "Eye Right Colour";
    public const string EYE_STYLE_NAME = "Eye Style";
    public const string HAIR_NAME = "Hair Colour";
    public const string SKIN_NAME = "Skin Colour";
    public const string GROWTH_RATE_NAME = "Growth Rate";
    public const string LIFE_EXPECTANCY_NAME = "Life Expectancy";
    public const string REPRODUCTIVE_AGE_NAME = "Reproductive Age";
    public const string GESTATION_PERIOD_NAME = "Gestation Period";

}
