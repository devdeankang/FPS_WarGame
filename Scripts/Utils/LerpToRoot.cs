using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpToRoot : MonoBehaviour, INoiseApplier
{
    public float PositionSmooth = 15f;
    public float RotationSmooth = 15f;
    public float Multiplier = 1;

    public Transform Root;
    public Transform RotationRoot;
    public Transform RootChanger
    {
        get
        {
            return Root;
        }
        set
        {
            Root = value;
        }
    }

    Vector3 position;
    Quaternion rotation;

    Vector3 noisePos;
    Quaternion noiseRot;

    void Start()
    {
        position = transform.position;
        rotation = transform.rotation;
    }

    public void ApplyAdditive(Vector3 pos, Quaternion rot)
    {
        noisePos = pos;
        noiseRot = rot;

    }

    public void Apply(Vector3 pos, Quaternion rot)
    {
        // not need
    }

    void FixedUpdate ()
    {
        if(Application.isPlaying)
        {
            position = Vector3.Lerp(position, Root.position + noisePos, PositionSmooth * Time.fixedDeltaTime * Multiplier);
            rotation = Quaternion.Lerp(rotation, RotationRoot.rotation * noiseRot, RotationSmooth * Time.fixedDeltaTime * Multiplier); // need add noise rot: RotationRoot.rotation * noiseRot

            transform.position = position;
            transform.rotation = rotation;
        }
    }
}
