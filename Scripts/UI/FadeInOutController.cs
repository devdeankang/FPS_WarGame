using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInOutController : MonoBehaviour
{
    public bool IsFadeIn = false;
    public bool IsFadeOut = false;
    public float delayTime = 3f;

    private void OnEnable()
    {
        if(IsFadeOut && !IsFadeIn)
        {
            Fader.Instance.FadeIn(delayTime);
            this.gameObject.SetActive(false);            
        }
        else if (!IsFadeOut && IsFadeIn)
        {
            Fader.Instance.FadeOut(delayTime);
            this.gameObject.SetActive(false);
        }
    }
}
