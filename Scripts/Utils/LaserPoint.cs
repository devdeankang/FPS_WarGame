using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPoint : MonoBehaviour
{
    public Transform Decal;
    public float Threshold = 0.1f;
    public LayerMask CollisionLayer;
    public Transform StartPoint;
    
	void Start ()
    {
		
	}
	
	void Update ()
    {
        Ray r = new Ray(StartPoint.position, StartPoint.forward);

        RaycastHit hitInfo;

        if(Physics.Raycast(r, out hitInfo, Mathf.Infinity, CollisionLayer, QueryTriggerInteraction.Ignore))
        {
            Decal.gameObject.SetActive(true);
            Decal.position = hitInfo.point + hitInfo.normal * Threshold;
            Decal.rotation = Quaternion.LookRotation(-hitInfo.normal);
        }
        else
        {
            Decal.gameObject.SetActive(false);
        }
	}

    void OnDisable()
    {
        Decal.gameObject.SetActive(false);
    }
    
    void OnEnable()
    {
        Decal.gameObject.SetActive(true);
    }
}
