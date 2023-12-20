using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace WarGame
{
    public class PlayerHealthBar : MonoBehaviour
    {
        [SerializeField]
        private PlayerHealth health;
        public Image TargetImage;

        public PlayerHealth Health
        {
            get
            {
                if (health == null)
                    health = GameObject.FindObjectOfType<PlayerHealth>();

                return health;
            }

            set
            {
                health = value;
            }
        }

        private void Update()
        {
            TargetImage.fillAmount = Health.HealthFraction;
        }
    }
}