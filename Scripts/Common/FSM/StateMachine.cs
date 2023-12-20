using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T>
{
    public IState<T> CurrentState { get; protected set; }

    private T _sender;

    public StateMachine(T sender, IState<T> state)
    {
        _sender = sender;
        SetState(state);
    }

    public void SetState(IState<T> state)
    {
        Debug.LogWarning("SetState : " + state); // 로그출력

        if (_sender == null)
        {
            Debug.LogError("Invalid m_sender");
            return;
        }

        if (CurrentState == state)
        {

            Debug.LogWarningFormat("Equal state : ", state);
            return;
        }
        if (CurrentState != null)
            CurrentState.OperateExit(_sender);

        CurrentState = state;

        if (CurrentState != null)
            CurrentState.OperateEnter(_sender);

        Debug.LogWarning("Set NextState : " + state); // 로그출력
    }

    public void OperateFixedUpdate()
    {
        if (_sender == null)
        {
            Debug.LogError("Invalid sender");
            return;
        }
        CurrentState.OperateFixedUpdate(_sender);
    }

        
    public void OperateUpdate()
    {
        if (_sender == null)
        {
            Debug.LogError("Invalid sender");
            return;
        }
        CurrentState.OperateUpdate(_sender);
    }
}
