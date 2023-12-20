using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningController : MonoBehaviour
{
    void Start()
    {
        if(SceneDataManager.Instance.once == false)
        {
            this.gameObject.SetActive(false);
        }
    }
}
