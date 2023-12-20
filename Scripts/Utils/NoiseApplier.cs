using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseApplier : MonoBehaviour
{
    public TransformNoise NoiseController;
    public List<Transform> Targets;

    public bool Additive = true;
    public bool UpdateFixedUpdate = true;

    Vector3 lastPos;
    Quaternion lastRot;

    List<INoiseApplier> appliers;

    void Start()
    {
        List<Transform> manualTargets = new List<Transform>();
        appliers = new List<INoiseApplier>();
        foreach (Transform t in Targets)
        {
            INoiseApplier applier = t.GetComponent<INoiseApplier>();
            if (applier != null)
            {
                appliers.Add(applier);
                manualTargets.Add(t);
            }
        }
        foreach(Transform t in manualTargets)
        {
            Targets.Remove(t);
        }
    }

    void Update()
    {
        if (!UpdateFixedUpdate)
        {
            ApplyNoise();
        }
    }

    void FixedUpdate()
    {
        if (UpdateFixedUpdate)
        {
            ApplyNoise();
        }
    }

    void ApplyNoise()
    {
        if (Additive)
        {
            ApplyAdditive(NoiseController.NoisedPosition, NoiseController.NoisedRotation);
        }
        else
        {
            Apply(NoiseController.NoisedPosition, NoiseController.NoisedRotation);
        }
    }

    public void ApplyAdditive(Vector3 pos, Quaternion rot)
    {
        foreach(INoiseApplier nApplier in appliers)
        {
            nApplier.ApplyAdditive(pos, rot);
        }
        foreach (Transform t in Targets)
        {
            t.localPosition -= lastPos;
            t.localRotation *= Quaternion.Inverse(lastRot);

            t.localPosition += pos;
            t.localRotation *= rot;
        }
        lastPos = pos;
        lastRot = rot;
    }

    public void Apply(Vector3 pos, Quaternion rot)
    {
        foreach (INoiseApplier nApplier in appliers)
        {
            nApplier.Apply(pos, rot);
        }
        foreach (Transform t in Targets)
        {
            t.localPosition = pos;
            t.localRotation = rot;
        }
    }
}

[System.Serializable]
public class NoiseApplierHelper
{
    public Transform[] Targets;

    Vector3 lastPos;
    Quaternion lastRot;

    public void ApplyNoise(Vector3 pos, Quaternion rot)
    {

    }
}

public interface INoiseApplier
{
    void ApplyAdditive(Vector3 pos, Quaternion rot);
    void Apply(Vector3 pos, Quaternion rot);
}