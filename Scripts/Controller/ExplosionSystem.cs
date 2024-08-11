using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ExplosionSystem : MonoBehaviour
{
    public DataForPool explosionsFX;
    public float particleDuration = 20f;
    public List<ParticleData> particleData;

    int particleCount;
    float time;

    private void OnEnable()
    {
        particleCount = particleData.Count;
        time = 0f;

        foreach (var data in particleData)
        {
            data.isExecuted = false;            
            data.particle.SetActive(false);
            data.audio.Stop();
        }        
    }

    private void Update()
    {
        if(time > particleDuration)
        {
            foreach(Transform tr in transform)
            {
                if(tr.gameObject.GetComponent<ParticleSystem>() != null)
                {
                    tr.gameObject.GetComponent<ParticleSystem>().Stop();
                }

                if(tr.gameObject.GetComponent<AudioSource>() != null)
                {
                    tr.gameObject.GetComponent<AudioSource>().Stop();
                }                
            }

            PoolManager.Instance.BackToPool(this.gameObject, explosionsFX.GroupName);
        }

        CreateExplosionFX();
    }

    private void FixedUpdate()
    {
        time += Time.deltaTime;
    }

    void CreateExplosionFX()
    {
        for (int i = 0; i < particleCount; i++)
        {
            if (particleData[i].isExecuted) continue;

            if (time >= particleData[i].startTime)
            {
                if (i != 0)
                {
                    particleData[i - 1].audio.Stop();
                    particleData[i - 1].particle.SetActive(false);
                }

                particleData[i].particle.SetActive(true);
                particleData[i].particle.GetComponent<ParticleSystem>().Play();
                particleData[i].audio.Play();
                particleData[i].isExecuted = true;
            }
        }
    }
}


[Serializable]
public class ParticleData
{
    public bool isExecuted;
    public GameObject particle;
    public float startTime;
    public AudioSource audio;
}