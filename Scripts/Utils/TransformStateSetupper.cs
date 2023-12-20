using UnityEngine;
using System.Collections;

namespace WarGame
{
    public class TransformStateSetupper : MonoBehaviour
    {
        public Vector3 PositionState;
        public Quaternion RotationState;

        public bool CalculateVelocity = false;
        public bool UsePosition = true;
        public bool UseRotation = true;

        Vector3 velocity;
        Vector3 angularVelocity;
        Vector3 oldPosition;
        Vector3 oldEulers;

        public Vector3 Velocity { get { return velocity; } }

        public Vector3 AngularVelocity { get { return angularVelocity; } }


        [ContextMenu("Save current state")]
        public void SaveCurrentState()
        {
            PositionState = transform.localPosition;
            RotationState = transform.localRotation;
        }

        public void LoadSavedState()
        {
            if(UsePosition)
                transform.localPosition = PositionState;

            if(UseRotation)
                transform.localRotation = RotationState;
        }

        private void FixedUpdate()
        {
            if(CalculateVelocity)
            {
                velocity = (transform.position - oldPosition) / Time.fixedDeltaTime;

                angularVelocity = (transform.eulerAngles - oldEulers) / Time.fixedDeltaTime;

                oldPosition = transform.position;
                oldEulers = transform.eulerAngles;
            }
        }
    }
}