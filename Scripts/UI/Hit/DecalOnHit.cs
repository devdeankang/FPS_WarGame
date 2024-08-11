using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalOnHit : MonoBehaviour
{
    public GameObject DecalPrefab;
    public bool Parent = true;
    public float AutoDestroy = 30f;
    public float IntervalPerDecals = 0f;
    BaseHittableObject hittableObject;

    float lastDecalSpawnTime = 0;

    private void Awake()
    {
        hittableObject = GetComponent<BaseHittableObject>();

        hittableObject.DamagedEvent.AddListener(damageTaken);
        hittableObject.DieEvent.AddListener(die);
    }

    void damageTaken(DamageData dmg)
    {
        if (lastDecalSpawnTime + IntervalPerDecals <= Time.time)
        {
            GameObject decal = Instantiate(DecalPrefab);
            decal.transform.position = dmg.HitPosition;
            decal.transform.rotation = Quaternion.LookRotation(dmg.HitDirection, Vector3.up);
            if (Parent)
            {
                decal.transform.SetParent(transform);
            }
            decal.SetActive(true);

            Destroy(decal, AutoDestroy);
            lastDecalSpawnTime = Time.time;
        }
    }

    void die(DamageData dmg)
    {
        if (lastDecalSpawnTime + IntervalPerDecals <= Time.time)
        {
            GameObject decal = Instantiate(DecalPrefab);
            decal.transform.position = dmg.HitPosition;
            decal.transform.rotation = Quaternion.LookRotation(dmg.HitDirection, Vector3.up);
            if (Parent)
            {
                decal.transform.SetParent(transform);
            }
            decal.SetActive(true);
            Destroy(decal, AutoDestroy);
            lastDecalSpawnTime = Time.time;
        }
    }
}
