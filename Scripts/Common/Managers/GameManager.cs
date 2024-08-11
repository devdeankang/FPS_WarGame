using DialogueQuests;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public UnityEvent EndGameEvent;
    public SceneType TypeLoad;

    #region Singleton
    private static GameManager instance = null;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }
    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        InitEvent();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void InitEvent()
    {
        if (EndGameEvent == null)
            EndGameEvent = new UnityEvent();
    }

    public void AddEventListener(UnityEvent ev, UnityAction callback)
    {
        if (callback != null)
        {
            ev.AddListener(callback);
        }
        else
        {
            Debug.LogWarningFormat("The UnityAction({0}) in the UnityEvent({1}) you called is the NULL Reference.", callback, ev);
        }
    }
}