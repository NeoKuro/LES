using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitPopSetupWindow : PopupWindow
{
    public Text speciesNameVal;
    public Text speciesDietVal;
    public Text speciesInitPopVal;
    

    //Standard Private
    private string speciesName = "";
    private int speciesInitPop = 1;

    //Monobehaviour Private
    private Population newPopulation;

    public void InitializeWindow(Population population)
    {
        newPopulation = population;
        InputHandler.camControlsEnabled = false;
        GlobalGEPSettings.gameStatus = GlobalGEPSettings.GAME_STATE.PAUSED;
    }

    public void OnNameChanged(InputField input)
    {
        speciesName = input.text;
        speciesNameVal.text = speciesName;
    }

    public void OnDietBtnPressed(GameObject btn)
    {
        if (btn.name.ToLower().Contains("carnivore"))
        {
            speciesDietVal.text = "Carnivore";
        }
        else if(btn.name.ToLower().Contains("herbivore"))
        {
            speciesDietVal.text = "Herbivore";
        }
        else if (btn.name.ToLower().Contains("omnivore"))
        {
            speciesDietVal.text = "Omnivore";
        }
    }

    public void OnInitPopSliderChanged(Slider popSlider)
    {
        speciesInitPop = (int)popSlider.value;
        speciesInitPopVal.text = speciesInitPop.ToString();
    }

    public void CancelNewPop()
    {
        Destroy(newPopulation.gameObject);
        base.CloseWindow();
    }

    public void ConfirmPopulation()
    {
        //Create a new Population class.
        //Two options, a button can bring up this window first, and on confirm, allows the player to place the population centre
        //      Option 2 is to bring up this window after population centre is chosen (pressing the UI button for new pop = 
        //              place centre. Then the "Population" class can open this window)

        if(newPopulation != null)
        {
            newPopulation.speciesName = speciesName;
            newPopulation.initPopulation = speciesInitPop;
            switch(speciesDietVal.text.ToLower())
            {
                case "carnivore":
                    newPopulation.dietType = Population.DIET_TYPE.CARNIVORE;
                    break;
                case "herbivore":
                    newPopulation.dietType = Population.DIET_TYPE.HERBIVORE;
                    break;
                case "omnivore":
                    newPopulation.dietType = Population.DIET_TYPE.OMNIVORE;
                    break;
                default:
                    newPopulation.dietType = Population.DIET_TYPE.OMNIVORE;
                    break;
            }
            newPopulation.setupComplete = true;
        }

        //After setup, close this window + canvas
        CloseWindow();
    }
}
