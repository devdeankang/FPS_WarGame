using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitObjectPool : MonoBehaviour
{
    [Header("Init Pool Data")]
    [SerializeField] private DataForPool usSoldiers = default;
    [SerializeField] private DataForPool ruSoldiers = default;
    [SerializeField] private DataForPool bloodFX = default;
    [SerializeField] private DataForPool explosionFX = default;
    void Start()
    {
        PoolManager.Instance.PoolInstaller(usSoldiers.Prefab, usSoldiers.NumPreparedObjects, usSoldiers.GroupName);
        PoolManager.Instance.PoolInstaller(ruSoldiers.Prefab, ruSoldiers.NumPreparedObjects, ruSoldiers.GroupName);
        PoolManager.Instance.PoolInstaller(bloodFX.Prefab, bloodFX.NumPreparedObjects, bloodFX.GroupName);        
        PoolManager.Instance.PoolInstaller(explosionFX.Prefab, explosionFX.NumPreparedObjects, explosionFX.GroupName);
    }

    void Update()
    {

    }
}
