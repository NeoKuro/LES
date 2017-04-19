using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine<T>
{
    private T owner;
    private FSMState<T> currentState;
    private FSMState<T> previousState;
    private FSMState<T> globalState;

    public void Awake()
    {
        currentState = previousState = globalState = null;
    }

    public void Configure(T newOwner, FSMState<T> initialState)
    {
        owner = newOwner;
        ChangeState(initialState);
    }

    public void Update()
    {
        if(globalState != null)
        {
            globalState.Execute(owner);
        }
        if(currentState != null)
        {
            currentState.Execute(owner);
        }
    }

    public void ChangeState(FSMState<T> newState)
    {
        previousState = currentState;
        if(currentState != null)
        {
            currentState.Exit(owner);
        }

        currentState = newState;

        if(currentState != null)
        {
            currentState.Enter(owner);
        }
    }

    public void RevertTopreviousState()
    {
        if(previousState != null)
        {
            currentState = previousState;
        }
    }
}
