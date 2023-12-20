using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class ShootingTargetData : BaseHittableObject
    {
        public GameObject ShootingQuest;
        public string EnemyTagParam = "RUSoldier";

        string MapTagParam = "Map";
        int targetHP = 100;
        bool isTakeDamage = false;

        public new bool IsAlive { get { return Health > 0; } }

        public bool IsTakeDamage { get { return IsAlive && isTakeDamage; } }

        private void Awake()
        {
            StartHealth = Health = targetHP;
        }

        private void Update()
        {
            if (ShootingQuest.activeSelf == true)
            {
                Health = StartHealth;
                Invulnerable = true;

                if (this.gameObject.CompareTag(EnemyTagParam))
                    this.gameObject.tag = MapTagParam;

                GetComponent<ShootingTargetData>().enabled = false;                
            }
        }

        public override void TakeDamage(DamageData damage)
        {            
            isTakeDamage = true;

            if (damage.HitType == DamageData.DamageType.Explosion)
                return;

            damage.Receiver = this;

            if (!Invulnerable)
                Health -= damage.DamageAmount;

            if(Health < StartHealth/2)
            {
                if (ShootingQuest.activeSelf == false)
                    ShootingQuest.SetActive(true);
            }

            isTakeDamage = false;
        }
    }
}