using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarGame
{
    public class SoldierData : MonoBehaviour
    {
        [Header("Pool Data")]
        public DataForPool soldier = default;
        public DataForPool bloodFX = default;

        [Header("Stats")]
        public GameObject handsObject;
        public GameObject MuzzleFX;
        public GameObject GunSound;
        public float damageAmount = 20f;
        public float speed = 0.25f;
        public float runSpeed = 0.4f;
        public float waitTime = 3f;
        public float autoFireRate = 0.3f;
        public float effectiveAttackRange = 20f;
        public float viewAngle = 110f;
        public float sightDistance = 2000f;
        public float gravity = 9.81f;
                
        [HideInInspector] public string Player = "Player";
        [HideInInspector] public string USSoldier = "USSoldier";
        [HideInInspector] public string RUSoldier = "RUSoldier";
        [HideInInspector] public string PlayerLayer = "Player";
        [HideInInspector] public string USLayer = "USLayer";
        [HideInInspector] public string RULayer = "RULayer"; 
        [HideInInspector] public string RU_Destinations = "RU_Destinations";
        [HideInInspector] public string US_Destinations = "US_Destinations";
        [HideInInspector] public string targetPoint = "TargetPoint";
        bool isRuSoldier = false;
        bool isUsSoldier = false;

        [HideInInspector] public float retreatDistance;
        [HideInInspector] public Dictionary<GameObject, float> targetDistances = new Dictionary<GameObject, float>();

        public bool IsRuSoldier { get { return isRuSoldier; } }
        public bool IsUsSoldier { get { return isUsSoldier; } }

        private void Awake()
        {
            IdentificationOfPeer();            
            retreatDistance = sightDistance + (sightDistance / 3);
        }

        private void OnEnable()
        {
            
            targetDistances = new Dictionary<GameObject, float>();
            targetDistances.Clear();
        }

        private void Update()
        {
            retreatDistance = sightDistance + (sightDistance / 3);
            UpdateTargetsDistance();
        }

        void UpdateTargetsDistance()
        {
            foreach (GameObject target in GetTargets())
            {
                if (!targetDistances.ContainsKey(target))
                    targetDistances.Add(target, GetDistance(target));
                else
                    targetDistances[target] = GetDistance(target);
            }
        }

        List<GameObject> GetTargets()
        {
            List<GameObject> targets = new List<GameObject>();
            if (IsRuSoldier)
            {
                targets = GameObject.FindGameObjectsWithTag(USSoldier).ToList();
                targets.Add(GameObject.FindGameObjectWithTag(Player));
            }
            else
            {
                targets = GameObject.FindGameObjectsWithTag(RUSoldier).ToList();
            }
            return targets;
        }

        float GetDistance(GameObject target)
        {
            return (target.transform.position - this.transform.position).sqrMagnitude;
        }

        void IdentificationOfPeer()
        {
            if (gameObject.CompareTag(RUSoldier))
            {
                isRuSoldier = true;
                isUsSoldier = false;
            }
            else if (gameObject.CompareTag(USSoldier))
            {
                isUsSoldier = true;
                isRuSoldier = false;
            }
            else
                Debug.LogError($" @{this.gameObject.name} is Invalid Tag.");
        }

        public bool IsCompareWithValidTarget(GameObject target)
        {
            if (IsRuSoldier)
            {
                if (target.CompareTag(USSoldier) || target.CompareTag(Player)) return true;
            }
            else if (IsUsSoldier)
            {
                if (target.CompareTag(RUSoldier)) return true;
            }

            return false;
        }
    }
}