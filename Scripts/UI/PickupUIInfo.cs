using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

namespace WarGame
{
    public class PickupUIInfo : MonoBehaviour
    {
        public Image Icon;
        public Text Label;
        public float LifeTime = 7f;
        public AnimationCurve AlphaCurve;
        public CanvasGroup Group;

        float elapsedTime = 0;

        public void Show(string text, Sprite icon, UnityAction destroyCallback)
        {
            Label.text = text;
            Icon.sprite = icon;
            StartCoroutine(destroyInfo(LifeTime, destroyCallback));
        }

        IEnumerator destroyInfo(float lifeTime, UnityAction destroyCallback)
        {
            elapsedTime = 0;
            while(elapsedTime < lifeTime)
            {
                elapsedTime += Time.deltaTime;

                float fraction = elapsedTime / lifeTime;
                float alpha = AlphaCurve.Evaluate(fraction);

                Group.alpha = alpha;

                yield return null;
            }
            if(destroyCallback != null)
                destroyCallback();
            Destroy(gameObject);
        }
    }
}