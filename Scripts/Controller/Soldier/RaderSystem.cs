using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace WarGame
{
    public class RaderSystem : MonoBehaviour
    {
        float viewAngle;
        float sightDistance;

        [Header("Draw Ray")]
        public bool isDrawRays = false;
        public bool isDrawVisionCone = false;
        public bool isDrawOverlapSphere = false;

        SoldierData data;

        public GameObject soldier;
        bool isClearingDetectedTargets;
        Vector3 offset = Vector3.zero;

        public List<GameObject> detectedTargets;
        int OverlapSphereLayer
        {
            get
            {
                if (data.IsRuSoldier)
                {
                    return 1 << LayerMask.NameToLayer(data.PlayerLayer) | 1 << LayerMask.NameToLayer(data.USLayer);
                }
                else if (data.IsUsSoldier)
                {
                    return 1 << LayerMask.NameToLayer(data.RULayer);
                }
                else
                {
                    return -1;
                }
            }
        }

        private void Awake()
        {
            data = GetComponentInParent<SoldierData>();
            soldier = transform.parent.gameObject;
            viewAngle = data.viewAngle;
            sightDistance = data.sightDistance;
        }

        private void OnEnable()
        {
            if (detectedTargets != null) detectedTargets.Clear();
        }

        void Start()
        {
            detectedTargets = new List<GameObject>();
        }

        void Update()
        {
            DoRaderSystem();
        }

        public float GetSqrMagnitude(GameObject target)
        {
            return (target.transform.position - soldier.transform.position).sqrMagnitude;
        }

        public GameObject GetNearestTarget(List<GameObject> detectedList)
        {
            GameObject target = null;
            float visibility = data.sightDistance;

            if (detectedList == null) return null;

            foreach (GameObject go in detectedList)
            {
                float targetDistance = GetSqrMagnitude(go);
                if (targetDistance < visibility)
                {
                    visibility = targetDistance;
                    target = go;
                }
            }

            return target;
        }

        void DoRaderSystem()
        {
            RaycastHit hit;

            Collider[] hitColliders = Physics.OverlapSphere(soldier.transform.position, data.sightDistance, OverlapSphereLayer);
            for (int i = 0; i < hitColliders.Length; i += 1)
            {
                GameObject target = hitColliders[i].gameObject;

                Vector3 dirToTarget = (target.transform.position - soldier.transform.position).normalized;
                if (Vector3.Angle(soldier.transform.forward, dirToTarget) < viewAngle / 2)
                {
                    if (isDrawRays)
                        Debug.DrawRay(soldier.transform.position, dirToTarget * data.sightDistance, Color.blue);

                    if (Physics.Raycast(soldier.transform.position, dirToTarget, out hit, data.sightDistance))
                    {
                        if (!data.IsCompareWithValidTarget(hit.collider.gameObject)) continue;

                        if (hit.collider.gameObject.activeSelf && !detectedTargets.Contains(hit.collider.gameObject))
                            detectedTargets.Add(hit.collider.gameObject);

                        if (!isClearingDetectedTargets)
                            StartCoroutine(ClearDetectedList());
                    }
                }
            }
        }

        public void OnDrawGizmos()
        {
            if (isDrawOverlapSphere)
                Gizmos.DrawWireSphere(soldier.transform.position, sightDistance);

            if (isDrawVisionCone)
            {
#if UNITY_EDITOR
                Color mColor = Handles.color;
                Color color = Color.blue;
                color.a = 0.1f;
                Handles.color = color;
                var halfFOV = viewAngle * 0.5f;
                var beginDirection = Quaternion.AngleAxis(-halfFOV, (Vector3.up)) * (soldier.transform.forward);
                Handles.DrawSolidArc(soldier.transform.TransformPoint(offset), soldier.transform.up, beginDirection,
                    viewAngle, data.sightDistance);
                Handles.color = mColor;
#endif
            }

        }

        IEnumerator ClearDetectedList()
        {
            isClearingDetectedTargets = true;
            yield return new WaitForSecondsRealtime(0.5f);
            detectedTargets.Clear();
            isClearingDetectedTargets = false;
        }


    }
}