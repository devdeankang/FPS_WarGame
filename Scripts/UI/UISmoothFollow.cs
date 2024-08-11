using UnityEngine;
using System.Collections;

namespace WarGame
{
    public class UISmoothFollow : MonoBehaviour
    {
        public RectTransform Target;
        public float Smooth = 17.5f;
        RectTransform rectTransform;

        private void Update()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (Target != null)
                rectTransform.position = Vector3.Lerp(rectTransform.position, Target.position, Time.deltaTime * Smooth);
        }
    }
}