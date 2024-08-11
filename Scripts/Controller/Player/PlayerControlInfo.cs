using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class PlayerControlInfo : MonoBehaviour
    {
        public GameObject InfoRoot;
        private PlayerController controller;
        private MeshRenderer blurSphere;

        bool isInfoEnabled = false;

        public PlayerController Controller
        {
            get
            {
                if (controller == null)
                    controller = FindObjectOfType<PlayerController>();

                return controller;
            }

            set
            {
                controller = value;
            }
        }

        public MeshRenderer BlurSphere
        {
            get
            {
                if (blurSphere == null)
                    blurSphere = Controller.nearBlurSphere;

                return blurSphere;
            }

            set
            {
                blurSphere = value;
            }
        }

        void Start()
        {
            enableInfo();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isInfoEnabled)
                {
                    disableInfo();
                }
                else
                {
                    enableInfo();
                }
            }
        }

        void enableInfo()
        {
            isInfoEnabled = true;
            InfoRoot.SetActive(true);
            Controller.Freeze(true);
            BlurSphere.enabled = true;
        }

        void disableInfo()
        {
            isInfoEnabled = false;
            InfoRoot.SetActive(false);
            Controller.Freeze(false);
            BlurSphere.enabled = false;
        }
    }
}