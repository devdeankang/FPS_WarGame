using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TransformTranslater : MonoBehaviour
{
    public Transform reference;
    public Transform target;
    public bool isInLocal = true;
    public bool isTranslatePos = false;
    public bool isTranslateRot = true;
    public bool isAdditive = true;

    Quaternion lastRot;
    Vector3 lastPos;

    Transform Target
    {
        get
        {
            if (target == null)
                return transform;

            return target;
        }
    }

    private void Awake()
    {
        if (target == null)
            target = transform;
    }

    private void OnEnable()
    {
        if (reference == null)
            return;

        if (isInLocal)
        {
            lastPos = reference.localPosition;
            lastRot = reference.localRotation;
        }
        else
        {
            lastPos = reference.position;
            lastRot = reference.rotation;
        }
    }

    void FixedUpdate ()
    {
        if (reference == null)
            return;

        if (isAdditive)
            ApplyAdditive();
        else
            ApplyForce();
    }

    void ApplyAdditive()
    {
        if (isInLocal)
        {
            if (isTranslatePos)
            {
                Target.localPosition -= lastPos;
                Target.localPosition += reference.localPosition;
                lastPos = reference.localPosition;
            }

            if (isTranslateRot)
            {
                Target.localRotation *= Quaternion.Inverse(lastRot);
                Target.localRotation *= reference.localRotation;
                lastRot = reference.localRotation;
            }
        }
        else
        {
            if (isTranslatePos)
            {
                Target.position -= lastPos;
                Target.position += reference.position;
                lastPos = reference.position;
            }

            if (isTranslateRot)
            {
                Target.rotation *= Quaternion.Inverse(lastRot);
                Target.rotation *= reference.rotation;
                lastRot = reference.rotation;
            }
        }
    }

    void ApplyForce()
    {
        if (isInLocal)
        {
            if (isTranslatePos)
            {
                Target.localPosition = reference.localPosition;
            }

            if (isTranslateRot)
            {
                Target.localRotation = reference.localRotation;
            }
        }
        else
        {
            if (isTranslatePos)
            {
                Target.position = reference.position;
            }

            if (isTranslateRot)
            {
                Target.rotation = reference.rotation;
            }
        }
    }

    void Update()
    {
        if(!Application.isPlaying)
        {
            if (isAdditive)
                ApplyAdditive();
            else
                ApplyForce();
        }
    }
}
