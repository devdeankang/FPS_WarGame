using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEvent : MonoBehaviour
{
    public DataForPool explosionsFX;

    [Header("Time")]
    [Range(1, 20)] public int minTime = 3;
    [Range(1, 60)] public int maxTime = 40;
    [Range(1, 10)] public int cycle = 3;
    [Range(1, 10)] public int once = 6;

    List<Transform> explosionAllSpots = new List<Transform>();
    List<Transform> explosionSpots = new List<Transform>();
    float time = 0f;
    bool canDrop = true;

    private void Awake()
    {
        foreach(Transform tr in transform)
        {
            explosionAllSpots.Add(tr);
        }
    }

    private void OnEnable()
    {
        time = 0f;
        canDrop = true;
    }

    private void FixedUpdate()
    {
        if(gameObject.activeSelf)
        {
            time += Time.deltaTime;
        }
    }

    private void Update()
    {
        if(time > maxTime)
        {
            gameObject.SetActive(false);
        }

        if(time >= minTime && canDrop == true)
        {
            explosionSpots.Clear();
            SetExplosionSpots();

            for(int i =0; i<explosionSpots.Count; i++)
            {
                PoolManager.Instance.SpawnObject(explosionsFX.Prefab, explosionsFX.GroupName, false, explosionSpots[i].position);
            }

            StartCoroutine(ExplosionDelay());
        }
    }

    void SetExplosionSpots()
    {
        while(explosionSpots.Count < once)
        {
            int random = Random.Range(0, explosionAllSpots.Count);
            if (explosionSpots.Contains(explosionAllSpots[random]))
                continue;

            explosionSpots.Add(explosionAllSpots[random]);
        }
    }

    IEnumerator ExplosionDelay()
    {
        canDrop = false;
        yield return new WaitForSeconds(cycle);
        canDrop = true;
    }   
}