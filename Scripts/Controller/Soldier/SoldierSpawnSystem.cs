using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierSpawnSystem : MonoBehaviour
{
    bool isPower = false;
    bool canSpawn = true;
    string SpawnPointsParam = "SpawnPoints";

    public DataForPool poolData;    
    [SerializeField] List<Transform> SpawnPoints;

    public int maxSpawnCount = 5;   // 인게임에서 생성될 수 있는 최대 병력 수
    int currentSpawnCount = 0;

    public bool isUnlimited = false;
    public int totalSpawnedCount = 15;
    [SerializeField] int spawnedCount = 0;

    [SerializeField] GameObject root;

    private void OnEnable()
    {
        isPower = true;
    }

    private void Start()
    {
        SetRootObject();
        SetSpawnPoints();
    }

    void SetSpawnPoints()
    {
        Transform spawnPoint = null;

        foreach (Transform tr in transform)
        {
            if(tr.gameObject.CompareTag(SpawnPointsParam))
            {
                spawnPoint = tr;
                break;
            }
        }

        foreach(Transform tr in spawnPoint)
        {
            SpawnPoints.Add(tr);
        }
    }

    private void Update()
    {
        if (!isPower)
            this.gameObject.SetActive(false);

        CountSpawnedSoldiers();

        if (!isUnlimited && spawnedCount >= totalSpawnedCount)
        {
            canSpawn = false;

            if (IsAllDespawned())
                isPower = false;
        }

        if (canSpawn && currentSpawnCount < maxSpawnCount)
        {
            spawnedCount++;
            StartCoroutine(SpawnSoldier(Random.Range(0, SpawnPoints.Count)));
        }

    }

    void CountSpawnedSoldiers()
    {
        currentSpawnCount = 0;
        foreach (Transform tr in root.transform)
        {
            if (tr.gameObject.activeSelf)
                currentSpawnCount++;
        }
    }

    bool IsAllDespawned()
    {
        foreach (Transform tr in root.transform)
        {
            if (tr.gameObject.activeSelf) return false;
        }

        return true;
    }

    void SetRootObject()
    {
        GameObject pools = GameObject.FindGameObjectWithTag("Pools");
        string rootName = string.Format("@{0} Pool", poolData.Prefab.name);

        foreach (Transform tr in pools.transform)
        {
            if (string.CompareOrdinal(tr.gameObject.name, rootName) == 0)
            {
                root = tr.gameObject;
                break;
            }
        }
    }

    IEnumerator SpawnSoldier(int index)
    {
        canSpawn = false;
        PoolManager.Instance.SpawnObject(poolData.Prefab, poolData.GroupName, false, SpawnPoints[index].position);
        yield return new WaitForSeconds(2f);
        canSpawn = true;
    }

}
