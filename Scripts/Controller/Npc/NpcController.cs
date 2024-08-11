using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour
{
    #region StringParam

    string idleParam = "Idling";
    string nonCombatParam = "NonCombat";

    #endregion
        
    Animator animator;    

    private void Awake()
    {
        animator = GetComponent<Animator>();        
    }

    private void Start()
    {
        animator.SetBool(idleParam, true);
        animator.SetBool(nonCombatParam, true);
    }
}
