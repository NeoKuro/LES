using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public enum INPUT_MODE
    {
        IDLE,
        PLACE_HUMAN_CENTRE,
        PLACE_POP_CENTRE
    }
    public static bool camControlsEnabled = true;    //Are the camera controls enabled (Disabled IE when a popup opens with Input request etc)
    public int populationCentreCount = 1;   //Always be at least 1
    public GameObject popCentre;
    public GameObject humanPopCentre;
    public List<GameObject> popList = new List<GameObject>();

    private INPUT_MODE currentMode = INPUT_MODE.IDLE;


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && currentMode != INPUT_MODE.PLACE_POP_CENTRE && currentMode != INPUT_MODE.PLACE_HUMAN_CENTRE)
        {
            currentMode = INPUT_MODE.PLACE_POP_CENTRE;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            currentMode = INPUT_MODE.IDLE;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (currentMode == INPUT_MODE.PLACE_POP_CENTRE)
            {
                PlaceNewPopulationCentre();
            }
            else if (currentMode == INPUT_MODE.PLACE_HUMAN_CENTRE)
            {
                PlaceHumanCentre();
            }
        }
        if (camControlsEnabled)
        {
            if (Input.GetKey(KeyCode.W))
            {
                //Move Camera Up
                gameObject.transform.position += Vector3.up * Time.deltaTime * GlobalGEPSettings.MOVE_SENSITIVITY;
                if(gameObject.transform.position.y > 50.0f)
                {
                    gameObject.transform.position = new Vector3(gameObject.transform.position.x, 50.0f, gameObject.transform.position.z);
                }
            }
            if (Input.GetKey(KeyCode.A))
            {
                //Move Camera Left
                gameObject.transform.position += Vector3.left * Time.deltaTime * GlobalGEPSettings.MOVE_SENSITIVITY;
                if (gameObject.transform.position.x < -50.0f)
                {
                    gameObject.transform.position = new Vector3(-50.0f, gameObject.transform.position.y, gameObject.transform.position.z);
                }
            }
            if (Input.GetKey(KeyCode.S))
            {
                //Move Camera Down
                gameObject.transform.position += Vector3.down * Time.deltaTime * GlobalGEPSettings.MOVE_SENSITIVITY;
                if (gameObject.transform.position.y < -50.0f)
                {
                    gameObject.transform.position = new Vector3(gameObject.transform.position.x, -50.0f, gameObject.transform.position.z);
                }
            }
            if (Input.GetKey(KeyCode.D))
            {
                //Move Camera Right
                gameObject.transform.position += Vector3.right * Time.deltaTime * GlobalGEPSettings.MOVE_SENSITIVITY;
                if (gameObject.transform.position.x > 50.0f)
                {
                    gameObject.transform.position = new Vector3(50.0f, gameObject.transform.position.y, gameObject.transform.position.z);
                }
            }

            //Add in Click-Drag?
        }
    }

    private void PlaceNewPopulationCentre()
    {
        if (popCentre == null)
        {
            Debug.Log("popCentre = null");
            return;
        }

        Vector3 popLocation = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        popLocation.z = -1.0f;

        GameObject newPopCentre = Instantiate(popCentre);
        newPopCentre.transform.position = popLocation;
        newPopCentre.GetComponent<Population>().populationIndex = populationCentreCount;
        newPopCentre.GetComponent<Population>().InitializeNewPopulation();
        popList.Add(newPopCentre);
        populationCentreCount++;

        Debug.Log("Pop centre Added");
        currentMode = INPUT_MODE.IDLE;
    }

    private void PlaceHumanCentre()
    {
        if (humanPopCentre == null)
        {
            Debug.Log("humanPopCentre = null");
            return;
        }

        Vector3 popLocation = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        popLocation.z = -1.0f;

        GameObject newPopCentre = Instantiate(humanPopCentre);
        newPopCentre.transform.position = popLocation;
        newPopCentre.GetComponent<Population>().populationIndex = populationCentreCount;
        newPopCentre.GetComponent<Population>().InitializeNewPopulation();
        popList.Add(newPopCentre);
        populationCentreCount++;

        Debug.Log("H Pop centre Added");
        currentMode = INPUT_MODE.IDLE;
    }
}
