using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace WarGame
{
    public class SoldierHealth : BaseHittableObject
    {
        SoldierData data;
        Animator animator;
        public float PlayerGrenadeDamageMul = 2f;
        bool isTakeDamage = false;

        public new bool IsAlive { get { return Health > 0; } }

        public bool IsTakeDamage { get { return IsAlive && isTakeDamage; } }

        private void Awake()
        {
            animator = GetComponent<Animator>();
            data = GetComponent<SoldierData>();
            StartHealth = Health;
        }

        public override void TakeDamage(DamageData damage)
        {
            isTakeDamage = true;

            if (damage.HitType == DamageData.DamageType.Explosion)
            {
                damage.DamageAmount *= PlayerGrenadeDamageMul;
            }

            damage.Receiver = this;

            if (!Invulnerable)
                Health -= damage.DamageAmount;

            if (damage.Deadly)
                Health = 0;

            if (IsAlive)
            {
                animator.SetTrigger("Pain");
                BloodEffect();
            }

            isTakeDamage = false;
        }

        void BloodEffect()
        {            
            Vector3 damagePos = this.transform.position;
            for (int i = 0; i < this.transform.childCount; i++)
            {
                if (this.transform.GetChild(i).CompareTag(data.targetPoint))
                {
                    damagePos = this.transform.GetChild(i).transform.position;
                    break;
                }
            }
            PoolManager.Instance.SpawnObject(data.bloodFX.Prefab, data.bloodFX.GroupName, true, damagePos);            
        }
    }
}