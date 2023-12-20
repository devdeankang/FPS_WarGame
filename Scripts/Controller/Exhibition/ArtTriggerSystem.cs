using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtTriggerSystem : MonoBehaviour
{
    public GameObject artAlertUI;
    public float timer = 3f;
    
    void Start()
    {

    }

    void Update()
    {
        AlertArtTips();
    }
        
    private void OnTriggerExit(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            OpenNotification();
        }
    }


    void AlertArtTips()
    {
        if (Input.GetKeyUp(KeyCode.F2))
        {
            OpenNotification();
        }
        else if (Input.GetKeyUp(KeyCode.F4))
        {
            CloseNotification();
        }
    }

    public void OpenNotification()
    {
        artAlertUI.GetComponent<Animator>().Play("In");
        StartCoroutine("StartTimer");
    }

    public void CloseNotification()
    {
        artAlertUI.GetComponent<Animator>().Play("Out");
    }

    IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(timer);
        CloseNotification();
    }

}
