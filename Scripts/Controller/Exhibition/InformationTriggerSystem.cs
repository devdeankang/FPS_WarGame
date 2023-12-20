using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InformationTriggerSystem : MonoBehaviour
{    
    public GameObject infomationUI;
    public float interactableDistance = 15f;

    Animator infoAnimator;
    GameObject player;
    GameObject information;
    
    void Start()
    {
        infoAnimator = infomationUI.GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        information = GameObject.FindGameObjectWithTag("Information");
    }

    void Update()
    {
        if(IsInteractable())
        {
            ReadInformation();
        }        
    }

    bool IsInteractable()
    {
        float distance = (player.transform.position - information.transform.position).sqrMagnitude;

        if (distance <= interactableDistance) 
            return true;
        else
            return false;
    }

    void ReadInformation()
    {        
        if (Input.GetKeyUp(KeyCode.R))
        {
            SetActiveUI(true);
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            SetActiveUI(false);
        }
    }

    public void SetActiveUI(bool value)
    {
        if(value)
        {            
            SetActiveMovement(false);
            infoAnimator.Play("Fade-in");
        }
        else
        {
            
            SetActiveMovement(true);
            infoAnimator.Play("Fade-out");
        }
    }

    public void SetActiveMovement(bool value)
    {
        if(value)
        {
            player.GetComponent<MoveController>().Freeze(false);
            //player.GetComponent<MoveController>().enabled = true;
        }
        else
        {
            player.GetComponent<MoveController>().Freeze(true);
            //player.GetComponent<MoveController>().enabled = false;
        }
    }

}
