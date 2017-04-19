using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInput : MonoBehaviour
{
    public GameObject creaturePopUpWindow;
    public GameObject popUpWindow;
    public GameObject userInterface;
    public bool popupWindowOpen = false;

    public void Start()
    {
        if (userInterface == null)
        {
            userInterface = GameObject.Find("UserInterface");
        }
    }

    public void CreaturePressed()
    {
        if (creaturePopUpWindow == null)
        {
            Debug.Log("creaturePopUpWindow not assigned");
            return;
        }
        if(GetComponent<CreatureManager>().inSetup)
        {
            return;
        }

        if (!popupWindowOpen)
        {
            popUpWindow = Instantiate(creaturePopUpWindow, creaturePopUpWindow.transform.position, creaturePopUpWindow.transform.rotation);
            popUpWindow.transform.SetParent(userInterface.transform, false);

            popUpWindow.GetComponent<CreaturePopupWindow>().InitializeWindow(gameObject);
            popupWindowOpen = true;
        }
    }

    public void OnMouseDown()
    {
        if (GlobalGEPSettings.gameStatus == GlobalGEPSettings.GAME_STATE.RUNNING)
            CreaturePressed();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Ignore if still setting creature up (IE positioning etc)
        if (GetComponent<CreatureManager>().inSetup && GetComponent<CreatureManager>().replicationMethod != REPLICATION_METHOD.REPRODUCE)
        {
            Vector3 pos = GetComponent<CreatureManager>().population.gameObject.transform.position;
            pos = new Vector3(pos.x, pos.y, -0.5f);
            gameObject.transform.position = pos;
            GetComponent<CreatureManager>().inSetup = false;
            return;
        }

        if (GetComponent<AIBehaviour>() == null || GetComponent<CreatureManager>().inSetup)
        {
            return;
        }

        //If Collides with another creature, check whether its been discovered before. If not Add it to the list otherwise ignore it
        if (collision.gameObject.CompareTag("Creature_Senses"))
        {
            GameObject creatureObj = collision.transform.parent.gameObject;
            if (creatureObj.GetComponent<CreatureManager>().population.populationIndex != GetComponent<CreatureManager>().population.populationIndex)
            {
                //Still setting up - to prevent bad data ignore it
                if (creatureObj.GetComponent<CreatureManager>().inSetup)
                {
                    return;
                }

                for (int i = 0; i < GetComponent<CreatureManager>().population.discoveredPopulations.Count; i++)
                {
                    if (GetComponent<CreatureManager>().population.discoveredPopulations[i].populationIndex == creatureObj.GetComponent<CreatureManager>().population.populationIndex)
                    {
                        return;
                    }
                }
                GetComponent<CreatureManager>().population.discoveredPopulations.Add(creatureObj.GetComponent<CreatureManager>().population);
            }
            return;
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Ignore if still setting creature up (IE positioning etc)
        if (GetComponent<CreatureManager>().inSetup && GetComponent<CreatureManager>().replicationMethod != REPLICATION_METHOD.REPRODUCE)
        {
            Vector3 pos = GetComponent<CreatureManager>().population.gameObject.transform.position;
            pos = new Vector3(pos.x, pos.y, -0.5f);
            gameObject.transform.position = pos;
            GetComponent<CreatureManager>().inSetup = false;
            return;
        }

        if (GetComponent<AIBehaviour>() == null || GetComponent<CreatureManager>().inSetup)
        {
            return;
        }

        if (collision.gameObject.layer == 11)
        {
            //Otherwise if collides with anything else (IE Water) then reverse direction
            if (GetComponent<AIBehaviour>().currentState == AIBehaviour.BEHAVIOUR_STATE.IDLE)
            {
                GetComponent<AIBehaviour>().reverseDirection = true;
            }
        }
    }


}


