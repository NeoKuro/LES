using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    public float maxHP = 1.0f;
    public float currHP = 1.0f;
    public float attSpeed = 1.0f;       //In attacks per second
    public float attDmg = 1.0f;         //In HP per attack

    public float age = 0.0f;
    public int generation = 1;
    public int childrenCount = 0;
    public bool canMate = false;
    public bool fertile = true;
    public bool female = true;         //Used to determine if they carry children
    public bool pregnant = false;

    public Dictionary<string, float> attributes = new Dictionary<string, float>();
    public Dictionary<string, float> traits = new Dictionary<string, float>();
    public Dictionary<string, float> capacityTraits = new Dictionary<string, float>();
    public Dictionary<string, Color> colourTraits = new Dictionary<string, Color>();

    //Monobehaviour Public
    public List<GameObject> children = new List<GameObject>();          //Currently living (and born) children

    public void SetStats(CreatureManager hManager)
    {
        for (int i = 0; i < hManager.traitManager.attributesList.Count; i++)
        {
            attributes.Add(hManager.traitManager.attributesList[i].attributeName.ToLower(), hManager.traitManager.attributesList[i].attributeTraitVal);
        }

        TraitManager thisManager = hManager.traitManager;

        traits.Add("Growth Rate", thisManager.growthRate.growthRate);
        traits.Add("Life Expectancy", thisManager.lifeExpectancy.lifeExpectancyVal);

        if(thisManager.reproductiveAge.reprodAge < 0)
        {
            fertile = false;
            thisManager.reproductiveAge.reprodAge *= -1;
        }

        traits.Add("Reproductive Age", (thisManager.lifeExpectancy.lifeExpectancyVal * thisManager.reproductiveAge.reprodAge)); //Is a %age of total life expectancy
        traits.Add("Gestation Period", (thisManager.gestationPeriod.gestationPeriodVal));                                       //Used as a %age of 1 Period (GlobalSettings)
        traits.Add("Energy Level", thisManager.energyLevel.energyLevelVal);
        traits.Add("Energy Consumption", thisManager.energyConsumption.energyConsumptionVal);
        traits.Add("Food Level", thisManager.foodLevel.foodLevelVal);
        traits.Add("Food Consumption", thisManager.foodConsumption.foodConsumptionVal);
        traits.Add("Water Level", thisManager.waterLevel.waterLevelVal);
        traits.Add("Water Consumption", thisManager.waterConsumption.waterConsumptionVal);

        capacityTraits.Add("Energy Level", thisManager.energyLevel.energyLevelVal);
        capacityTraits.Add("Food Level", thisManager.foodLevel.foodLevelVal);
        capacityTraits.Add("Water Level", thisManager.waterLevel.waterLevelVal);

        colourTraits.Add("Eye Left", hManager.eyeColours[0]);
        if(hManager.eyeColours.Length == 2)
            colourTraits.Add("Eye Right", hManager.eyeColours[1]);
        colourTraits.Add("Hair Colour", hManager.hairColour);
        colourTraits.Add("Skin Colour", hManager.skinColour);
        
        float HP = 0.0f;
        //HP is result of Str + Const, minus percentage of HP based on age
        HP = (attributes["strength"] + attributes["constitution"]);
        maxHP = HP;
        currHP = maxHP;

        float speed = 0.0f;

        speed = (attributes["constitution"]);
        speed /= 10.0f;
        attSpeed = speed;

        GetGeneration();

        //The initial populations can by default breed. Regardless of Genes they are all fertile also
        if(hManager.initHumanPop || hManager.replicationMethod == REPLICATION_METHOD.RANDOMLY_GENERATE)
        {
            age = traits["Life Expectancy"] * GlobalGEPSettings.INIT_AGE_MULTIPLIER;
            fertile = true;
            canMate = true;
        }
    }

    public float GetStatValue(string statName)
    {
        bool isAttribute = CheckIsAttribute(statName);

        if (isAttribute)
        {
            return attributes[statName.ToLower()];
        }

        return traits[statName];
    }

    public Color GetColourStatValue(string statName)
    {
        return colourTraits[statName];
    }

    private bool CheckIsAttribute(string statName)
    {
        foreach (KeyValuePair<string, float> attribute in attributes)
        {
            if (attribute.Key.ToLower() == statName.ToLower())
            {
                return true;
            }
        }

        return false;
    }

    private void GetGeneration()
    {
        if(GetComponent<CreatureManager>().population.genIndices.ContainsKey(GetComponent<CreatureManager>().creatureIndex))
        {
            generation = GetComponent<CreatureManager>().population.genIndices[GetComponent<CreatureManager>().creatureIndex];
        }
    }
}
