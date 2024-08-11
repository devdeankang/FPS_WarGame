using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformNoise : MonoBehaviour
{
    public PerlinNoise[] PositionNoises;
    public PerlinNoise[] RotationNoises;
    public bool AutoApply;
    public float NoiseAmount = 1f;
    public bool UpdateFixedUpdate = true;

    Vector3 position;
    Vector3 eulerRotation;
    PerlinNoise.PerlinNoiseData noiseData;
    

    public Vector3 NoisedPosition { get { return position * NoiseAmount; } }

    public Quaternion NoisedRotation { get { return Quaternion.Euler(eulerRotation * NoiseAmount); } }


    void Start ()
    {
		noiseData.offset = new Vector3(
                    UnityEngine.Random.Range(-10000f, 10000f),
                    UnityEngine.Random.Range(-10000f, 10000f),
                    UnityEngine.Random.Range(-10000f, 10000f));
    }

    void Update()
    {
        if(!UpdateFixedUpdate)
        {
            UpdateNoise();
        }
    }

    void FixedUpdate()
    {
        if (UpdateFixedUpdate)
        {
            UpdateNoise();
        }
    }
	
	void UpdateNoise ()
    {
        position = Vector3.zero;
        eulerRotation = Vector3.zero;

        Vector3 time = Vector3.one * Time.time;
        noiseData.time = time;
        foreach (PerlinNoise n in PositionNoises)
        {
            position += n.GetPositionNoise(noiseData);
        }
        foreach (PerlinNoise n in RotationNoises)
        {
            eulerRotation += n.GetRotationNoise(noiseData);
        }

        if (AutoApply)
        {
            transform.localPosition = position;
            transform.localRotation = Quaternion.Euler(eulerRotation);
        }
	}
}

[System.Serializable]
public class PerlinNoise : BasicNoise<PerlinNoise.PerlinNoiseData>
{
    public Vector3 amplitude;
    public Vector3 frequency;

    public struct PerlinNoiseData
    {
        public Vector3 time;
        public Vector3 offset;
    }

    public override Vector3 GetPositionNoise(PerlinNoiseData data)
    {
        float xPos = 0f;
        float yPos = 0f;
        float zPos = 0f;

        Vector3 timeVal = Vector3.Scale(data.time, frequency);
        timeVal += data.offset;

        Vector3 noise = new Vector3(
                Mathf.PerlinNoise(timeVal.x, 0f) - 0.5f,
                Mathf.PerlinNoise(timeVal.y, 0f) - 0.5f,
                Mathf.PerlinNoise(timeVal.z, 0f) - 0.5f);

        xPos += noise.x * amplitude.x;
        yPos += noise.y * amplitude.y;
        zPos += noise.z * amplitude.z;

        return new Vector3(xPos, yPos, zPos);
    }

    public override Vector3 GetRotationNoise(PerlinNoiseData data)
    {
        float xPos = 0f;
        float yPos = 0f;
        float zPos = 0f;

        Vector3 timeVal = Vector3.Scale(data.time, frequency);
        timeVal += data.offset;

        Vector3 noise = new Vector3(
                Mathf.PerlinNoise(timeVal.x, 0f) - 0.5f,
                Mathf.PerlinNoise(timeVal.y, 0f) - 0.5f,
                Mathf.PerlinNoise(timeVal.z, 0f) - 0.5f);

        xPos += noise.x * amplitude.x;
        yPos += noise.y * amplitude.y;
        zPos += noise.z * amplitude.z;

        return new Vector3(xPos, yPos, zPos);
    }


}

public abstract class BasicNoise<T>
{
    public abstract Vector3 GetPositionNoise(T data);
    public abstract Vector3 GetRotationNoise(T data);
}