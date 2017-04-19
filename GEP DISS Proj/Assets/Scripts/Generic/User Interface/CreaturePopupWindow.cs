using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreaturePopupWindow : PopupWindow
{
    public GameObject traitListing;

    //Standard Private
    private string creatureName = "";
    private float expandedYSize = 360.0f;   //Size the window will expand to - Defaults to 240 (the current size)
    private float defaultYSize = 240.0f;   //Size the window will expand to - Defaults to 240 (the current size)
    private bool expanded = false;          //Is the popup window expanded, showing additional information?
    private bool updateSize = false;        //Toggle, prevents input twice

    //Monobehaviour Private
    private GameObject creatureObject;
    GameObject titleObject = null;
    private GameObject statsList = null;
    private GameObject traitsList = null;
    private List<GameObject> traitListingObjects = new List<GameObject>();

    //Going to need additional window options (IE "Expanded" to show more options such as current action, thoughts, spouse, children etc)
    //Possibly hide "Traits" section, and instead by default show Spouse, Thoughts, action, children etc?

    //New Layout:
    //              Default:    Attributes on top as is
    //                          Below is "Stats" (Pregnant?, Child count, Disease count?, Fertile?, Current action/State)
    //              EXPANDED:   Display Traits when expanded (Life Expectancy, Food/Water/Energy levels, Reproductive age, Gest Period. All but consumption rates?)

    private void FixedUpdate()
    {
        if (creatureObject == null)
        {
            //Could destroy window, but allow to stay open so user can review stats etc
            return;
        }

        if (creatureObject.GetComponent<AIBehaviour>().updateStats)
        {
            UpdateData();
            creatureObject.GetComponent<AIBehaviour>().updateStats = false;
        }
    }

    public void InitializeWindow(GameObject creature)
    {
        creatureObject = creature;
        creatureName = creatureObject.name;


        //Update the info on screen
        SetData();
    }

    public override void CloseWindow()
    {
        if (creatureObject != null)
        {
            creatureObject.GetComponent<UIInput>().popupWindowOpen = false;
            creatureObject.GetComponent<UIInput>().popUpWindow = null;
        }
        traitListingObjects.Clear();

        Destroy(gameObject);
    }

    public void ChangeWindowSize()
    {
        if (!updateSize)
        {
            updateSize = true;
            StartCoroutine(ExpandWindow());
        }

    }

    private void SetData()
    {
        LoadTitleData();
        LoadAttributes();
        LoadStats();
        LoadTraits();
    }

    private void UpdateData()
    {
        titleObject.transform.GetChild(1).GetComponent<Text>().text = creatureObject.GetComponent<Stats>().age.ToString("F1");
        UpdateStats();
        //Only update data if is on display. 
        //Prevents unnecessary executions
        if (expanded)
        {
            UpdateTraits();
        }
    }

    private void LoadTitleData()
    {        
        Transform[] children = gameObject.GetComponentsInChildren<Transform>();

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].CompareTag("CPopup_Title"))
            {
                titleObject = children[i].gameObject;
                break;
            }
        }

        if (titleObject == null)
        {
            Debug.Log("Unable to find titleObject ('CPopup_Title')");
            return;
        }

        titleObject.transform.GetChild(0).GetComponent<Text>().text = creatureName;
        titleObject.transform.GetChild(1).GetComponent<Text>().text = creatureObject.GetComponent<Stats>().age.ToString("F1");
    }

    private void LoadAttributes()
    {
        GameObject attributesList = null;
        Transform[] children = gameObject.GetComponentsInChildren<Transform>();

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].CompareTag("CPopup_Attr"))
            {
                attributesList = children[i].gameObject;
                break;
            }
        }

        if (attributesList == null)
        {
            Debug.Log("Unable to find Attributes List ('CPopup_Attr')");
            return;
        }

        Text thisText = null;
        float attrVal = 0;

        for (int i = 0; i < attributesList.transform.childCount - 1; i++)
        {
            thisText = attributesList.transform.GetChild(i).gameObject.GetComponent<Text>();

            if (thisText != null)
            {
                attrVal = creatureObject.GetComponent<Stats>().GetStatValue(thisText.name);
                thisText.text = attrVal.ToString("F1");
            }
            else
            {
                Debug.Log("Unable to find " + thisText.name + " Object");
            }
        }
    }

    private void LoadStats()
    {
        Transform[] children = gameObject.GetComponentsInChildren<Transform>();

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].CompareTag("CPopup_Stats"))
            {
                statsList = children[i].gameObject;
                break;
            }
        }

        if (statsList == null)
        {
            Debug.Log("Unable to find Stats List ('CPopup_Stats')");
            return;
        }
        UpdateStats();
    }

    private void LoadTraits()
    {
        //Will search even inactive objects (Traits starts off as disabled)
        GridLayoutGroup[] children = gameObject.GetComponentsInChildren<GridLayoutGroup>();

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].CompareTag("CPopup_Traits"))
            {
                traitsList = children[i].gameObject;
                break;
            }
        }

        if (traitsList == null)
        {
            Debug.Log("Unable to find Traits List ('CPopup_Traits')");
            return;
        }

        Dictionary<string, float> traits = creatureObject.GetComponent<Stats>().traits;


        if (traitListing == null)
        {
            Debug.Log("traitListing not assigned");
            return;
        }

        foreach (KeyValuePair<string, float> trait in traits)
        {
            GameObject newListing = Instantiate(traitListing);
            newListing.transform.SetParent(traitsList.transform);

            newListing.transform.GetChild(0).GetComponent<Text>().text = trait.Key;
            newListing.transform.GetChild(1).GetComponent<Text>().text = trait.Value.ToString("F1");
            traitListingObjects.Add(newListing);
        }
    }

    private string GetStatValue(string statName)
    {
        switch (statName.ToLower())
        {
            case "action":
                return creatureObject.GetComponent<AIBehaviour>().currentState.ToString();
            case "pregnant":
                return creatureObject.GetComponent<Stats>().pregnant.ToString();
            case "fertile":
                return creatureObject.GetComponent<Stats>().fertile.ToString();
            case "priority":
                return creatureObject.GetComponent<AIBehaviour>().currentPriority.ToString();
            case "generation":
                return creatureObject.GetComponent<Stats>().generation.ToString();
            case "children":
                return creatureObject.GetComponent<Stats>().childrenCount.ToString();
            default:
                return "NULL";
        }
    }

    private IEnumerator ExpandWindow()
    {
        Vector2 newSize = GetComponent<RectTransform>().sizeDelta;
        float targetSize = 0.0f;
        if (!expanded)
        {
            targetSize = expandedYSize;
            while (newSize.y < expandedYSize)
            {
                newSize.y++;
                GetComponent<RectTransform>().sizeDelta = newSize;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            targetSize = defaultYSize;
            while (newSize.y > defaultYSize)
            {
                newSize.y--;
                GetComponent<RectTransform>().sizeDelta = newSize;
                yield return new WaitForEndOfFrame();
            }
        }

        newSize.y = targetSize;

        GetComponent<RectTransform>().sizeDelta = newSize;

        updateSize = false;

        expanded = !expanded;

        if (expanded)
        {
            LoadTraits();
        }
    }

    private void UpdateStats()
    {

        Text thisText = null;

        for (int i = 0; i < statsList.transform.childCount - 1; i++)
        {
            thisText = statsList.transform.GetChild(i).gameObject.GetComponent<Text>();

            if (thisText != null)
            {
                thisText.text = GetStatValue(thisText.name);
            }
            else
            {
                Debug.Log("Unable to find " + thisText.name + " Object");
            }
        }
    }

    private void UpdateTraits()
    {
        if (traitsList == null)
        {
            Debug.Log("Unable to find Traits List ('CPopup_Traits')");
            return;
        }

        Dictionary<string, float> traits = creatureObject.GetComponent<Stats>().traits;

        foreach (KeyValuePair<string, float> trait in traits)
        {
            for (int i = 0; i < traitListingObjects.Count; i++)
            {
                string name = traitListingObjects[i].transform.GetChild(0).GetComponent<Text>().text;
                if (name == trait.Key)
                {
                    traitListingObjects[i].transform.GetChild(1).GetComponent<Text>().text = trait.Value.ToString("F1");
                }
            }
        }
    }
}
