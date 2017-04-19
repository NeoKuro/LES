using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Idle : FSMState<AIBehaviour>
{
    //Standard Public
    public float angle = 0.0f;


    //Standard Private
    private bool wait = false;
    private int chooseAction = 1;
    private float lastAction = 0.0f;



    public Idle()
    {

    }

    public override void Enter(AIBehaviour entity)
    {
        //Debug.Log(entity.gameObject.name + ": Derpy Derp");
        //Eventually replace with text bubble "?" or "..." 
        entity.GetComponent<AILerp>().enabled = false;
    }

    public override void Execute(AIBehaviour entity)
    {
        if(entity.reverseDirection && !wait)
        {
            angle = 180;
            entity.reverseDirection = false;
            entity.gameObject.transform.Rotate(Vector3.forward, angle);
            lastAction = Time.time;
            wait = true;
        }

        //Choose action for next period
        //Happens every 1.5 seconds
        if ((Time.time - lastAction) >= 1.5f)
        {
            angle = Random.Range(0, 360);   //Choose random direction to move in
            entity.gameObject.transform.Rotate(Vector3.forward, angle);
            chooseAction = Random.Range(0, 2);
            lastAction = Time.time;
            wait = false;
        }

        //Choose random action (move, stand still)
        if (chooseAction == 0)
        {
            //Move around randomly
            entity.gameObject.transform.localPosition -= entity.gameObject.transform.up * Time.deltaTime;
        }
        else if (chooseAction == 1)
        {
            //stand still
        }
        else
        {
            //default behaviour
        }
    }

    public override void Exit(AIBehaviour entity)
    {
        //Debug.Log(entity.gameObject.name + ": Omg Something to do!");
    }
}
