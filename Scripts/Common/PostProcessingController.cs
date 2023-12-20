using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingController : MonoBehaviour
{
    public PostProcessVolume Volume;
    public DepthOfFieldSettings DofSettings;

    public ColorGradingSettings ColorSettings;

    DepthOfField dof;
    ColorGrading colorGrading;
   
    void Start()
    {
        Volume.sharedProfile = Instantiate(Volume.sharedProfile);

        dof = Volume.profile.GetSetting<DepthOfField>();
        colorGrading = Volume.profile.GetSetting<ColorGrading>();

        DofSettings.focusDistance = dof.focusDistance.value;
        DofSettings.aperture = dof.aperture.value;
        DofSettings.focalLength = dof.focalLength.value;

        ColorSettings.Lift = colorGrading.lift.value;
        ColorSettings.GreenOutGreenIn = colorGrading.mixerGreenOutGreenIn.value;
    }
    
    void Update()
    {
        dof.focusDistance.value = DofSettings.focusDistance;
        dof.aperture.value = DofSettings.aperture;
        dof.focalLength.value = DofSettings.focalLength;

        colorGrading.lift.value = ColorSettings.Lift;
        colorGrading.mixerGreenOutGreenIn.value = ColorSettings.GreenOutGreenIn;
    }

    public void LerpDof(DepthOfFieldSettings start, DepthOfFieldSettings end, float t)
    {
        DofSettings.focusDistance = Mathf.Lerp(start.focusDistance, end.focusDistance, t);
        DofSettings.aperture = Mathf.Lerp(start.aperture, end.aperture, t);
        DofSettings.focalLength = Mathf.Lerp(start.focalLength, end.focalLength, t);
    }

    public void LerpColorGrading(ColorGradingSettings start, ColorGradingSettings end, float t)
    {
        ColorSettings.Lift = Vector4.Lerp(start.Lift, end.Lift, t);
        ColorSettings.GreenOutGreenIn = Mathf.Lerp(start.GreenOutGreenIn, end.GreenOutGreenIn, t);
    }

    [ContextMenu("Show grading")]
    void ShowLift()
    {
        colorGrading = Volume.profile.GetSetting<ColorGrading>();
        Debug.Log(colorGrading.lift.value);
        Debug.Log(colorGrading.mixerGreenOutGreenIn.value);
    }

    [System.Serializable]
    public struct DepthOfFieldSettings
    {
        public float focusDistance;
        public float aperture;
        public float focalLength;
    }

    [System.Serializable]
    public struct ColorGradingSettings
    {
        public Vector4 Lift;
        public float GreenOutGreenIn;
    }
}
