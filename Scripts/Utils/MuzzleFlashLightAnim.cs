using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlashLightAnim : MonoBehaviour
{
    public float LiveTime = 0.13f;
    public float TargetIntensity = 0;
    public AnimationCurve Curve;
    public Light TargetLight;
    public Transform MuzzleShot;
    public Transform Effects;
    public float EffectsLiveTime = 5f;
    public float AngleStep = -1;
    public MeshRenderer TargetRenderer;
    public Material[] RandomMaterials;
    public float MinScale = 1f;
    public float MaxScale = 1f;

    float startIntensity;
    float startTime;
    
	void Awake ()
    {
        startIntensity = TargetLight.intensity;

    }

    void OnEnable()
    {
        if (AngleStep < 0)
        {
            MuzzleShot.rotation *= Quaternion.AngleAxis(Random.value * 360f, Vector3.forward);
        }
        else
        {
            MuzzleShot.rotation *= Quaternion.AngleAxis(Random.Range(0, 360) * AngleStep, Vector3.forward);
        }
        TargetLight.intensity = startIntensity;
        startTime = Time.time;
        if (Effects != null)
        {
            Effects.SetParent(null);
            Destroy(Effects.gameObject, EffectsLiveTime);
        }

        if(TargetRenderer != null)
        {
            TargetRenderer.sharedMaterial = RandomMaterials[Random.Range(0, RandomMaterials.Length)];
        }

        transform.localScale *= Random.Range(MinScale, MaxScale);
    }
	
	void Update ()
    {
        float fraction = (Time.time - startTime) / LiveTime;
        float value = Curve.Evaluate(fraction);

        TargetLight.intensity = Mathf.Lerp(startIntensity, TargetIntensity, value);
	}
}
