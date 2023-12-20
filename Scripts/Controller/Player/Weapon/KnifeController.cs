using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WarGame
{
    [System.Serializable]
    public class KnifeController : WeaponController, IWeaponAmmo, IWeaponShootMode
    {
        #region StringParams

        string ToIdle = "KnifeHitToIdle";
        string DamageAnim = "KnifeDamage";
        string toUpgradeAnim = "KnifeToUpgrade";
        string fromUpgradeAnim = "KnifeFromUpgrade";

        #endregion

        #region Varaibles
        public string[] Attacks;
        public string[] CameraAnimations;
        public GameObject HandsProps;
        public PlayerController Controller;
        public string InfinitySymbol = "∞";
        public Rigidbody PropsBody;
        public Collider[] PropsCollision;
        public float DetachPropsForce = 50f;
        public Transform DetachPropsDirection;
        public LayerMask HitLayer;
        public Transform HitSphere;
        public float Damage = 15f;
        public float HitRadius = 0.2f;
        [SerializeField] MeshRenderer sphere;
        [SerializeField] WeaponCustomization customization;
        [SerializeField] PlayerController controller;
        TransformStateSetupper propsTransformState;
        Rigidbody clonedPropsBody;
        int currentAttack = 0;

        List<IHittableObject> hittedTargets = new List<IHittableObject>();

        bool needNextAttack = false;
        bool isAttacking = false;
        bool isUpgrading = false;

        #endregion

        public override void Init(GameObject root)
        {
            base.Init(root);

            propsTransformState = PropsBody.GetComponent<TransformStateSetupper>();

            InitKnifeParams();

            Controller.JumpStartEvent += StartJump;
            Controller.JumpEndEvent += EndJump;

            Controller.DamageHandler.DamagedEvent.AddListener(DamagedEvent);
            Controller.DamageHandler.ResurrectEvent.AddListener(ResurrectEvent);


            if (!propsTransformState.CalculateVelocity)
            {
                Debug.LogWarning("Weapon velocity calculation disabled, it's enables automatically", PropsBody);
                propsTransformState.CalculateVelocity = true;
            }

            Controller.PlayerFreezeChanged.AddListener(FreezeChanged);

            if (hitEffectPool != null) GameObject.Destroy(hitEffectPool);
        }

        void InitKnifeParams()
        {
            deployedParameter = "Knife";
            deployAnim = "KnifeDeploy";
            hideAnim = "KnifeHide";
            hideCrossFade = 0.1f;
        }

        void FreezeChanged(bool isFreezed)
        {
            if(!isFreezed)
            {
                if(isUpgrading)
                {
                    Controller.Freeze(true);
                }
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            float animationIsAttack = handsAnimator.GetFloat("IsKnifeAttacking");
            if(animationIsAttack >= 0.9f)
            {
                List<IHittableObject> hitted = getTargetsInSphere(HitSphere, HitRadius, HitLayer);
                foreach(IHittableObject h in hitted)
                {
                    if (hittedTargets.Contains(h))
                        continue;

                    DamageData damage = new DamageData();
                    damage.DamageAmount = Damage;
                    damage.HitDirection = HitSphere.forward;
                    damage.HitPosition = HitSphere.position;
                    damage.Receiver = h;
                    damage.HitType = DamageData.DamageType.KnifeHit;
                    h.TakeDamage(damage);

                    hittedTargets.Add(h);
                }
            }
            else
            {
                hittedTargets.Clear();
            }
        }

        List<IHittableObject> getTargetsInSphere(Transform sphere, float radius, LayerMask mask)
        {
            List<IHittableObject> targets = new List<IHittableObject>();

            Collider[] colliders = Physics.OverlapSphere(sphere.position, HitRadius, HitLayer);

            foreach(Collider c in colliders)
            {
                IHittableObject target = c.GetComponent<IHittableObject>();

                if(target != null)
                {
                    targets.Add(target);
                }
            }

            return targets;
        }

        void ResurrectEvent()
        {
            if (IsDeployed)
            {
                ShowProps();
            }

            if (clonedPropsBody != null)
            {
                GameObject targetDestroy = clonedPropsBody.gameObject;
                GameObject.Destroy(targetDestroy);
            }
        }

        public void DetachProps()
        {
            HideProps();
            clonedPropsBody = GameObject.Instantiate(PropsBody, PropsBody.transform.parent);
            clonedPropsBody.transform.SetParent(null);
            clonedPropsBody.gameObject.SetActive(true);
            clonedPropsBody.isKinematic = false;
            clonedPropsBody.velocity = propsTransformState.Velocity;
            clonedPropsBody.angularVelocity = propsTransformState.AngularVelocity;
            clonedPropsBody.AddForce(DetachPropsDirection.forward * DetachPropsForce);
        }

        void DamagedEvent(DamageData damage)
        {
            if (!isDeployed)
                return;

            handsAnimator.Play(DamageAnim);
        }

        public virtual void StartJump()
        {
            if (!IsDeployed || !isCanControl)
                return;

            handsAnimator.SetBool("JumpFall", false);
            handsAnimator.SetTrigger("Jump");
        }

        public virtual void EndJump()
        {
            if (!IsDeployed || !isCanControl)
                return;

            handsAnimator.SetBool("JumpFall", true);
            handsAnimator.ResetTrigger("Jump");
        }

        public override void Deploy()
        {
            base.Deploy();
            HandsProps.SetActive(true);
        }

        public override void HideProps()
        {
            HandsProps.SetActive(false);
        }

        public override void ShowProps()
        {
            HandsProps.SetActive(true);
        }

        public override void InputEvent(InputEventType et)
        {
            base.InputEvent(et);

            if (!HandsProps.activeSelf || !isCanControl)
                return;

            if (et == InputEventType.MouseDown)
            {
                Attack();
            }

            if (et == InputEventType.Upgrade)
            {
                if (isUpgrading)
                {
                    FromUpgrade(true);
                }
                else
                {
                    ToUpgrade();
                }
            }

        }

        public virtual void ToUpgrade()
        {
            if (isAttacking)
                return;

            sphere.enabled = true;
            customization.ShowModifications();
            isUpgrading = true;
            handsAnimator.CrossFadeInFixedTime(toUpgradeAnim, 0.25f, 0);
            controller.Freeze(true);
        }

        public virtual void FromUpgrade(bool withAnim = true)
        {
            sphere.enabled = false;
            customization.CloseModifications();
            isUpgrading = false;

            if (withAnim)
            {
                handsAnimator.Play(fromUpgradeAnim, 0, 0);
            }

            controller.Freeze(false);
        }

        public virtual void NextAttack()
        {
            if (!IsDeployed || !IsCanControl)
                return;

            hittedTargets.Clear();
            if (needNextAttack)
            {
                needNextAttack = false;
                if (currentAttack == 0)
                    currentAttack = 1;
                PlayAttack(true);
            }
            else
            {
                isAttacking = false;
                handsAnimator.Play(ToIdle, 0, 0);
            }
        }

        public virtual void PlayAttack(bool isNextAttack)
        {
            if (isNextAttack)
            {
                handsAnimator.CrossFadeInFixedTime(CameraAnimations[currentAttack], 0.1f, 3, 0);
                handsAnimator.CrossFadeInFixedTime(Attacks[currentAttack], 0.1f, 0, 0);
            }
            else
            {
                handsAnimator.Play(CameraAnimations[currentAttack], 3, 0);
                handsAnimator.Play(Attacks[currentAttack], 0, 0);
            }
            currentAttack++;
            if (currentAttack >= Attacks.Length)
            {
                currentAttack = 0;
            }
        }

        public virtual void Attack()
        {
            if (isAttacking)
            {
                if (!needNextAttack)
                {
                    needNextAttack = true;
                }
            }
            else
            {
                hittedTargets.Clear();
                currentAttack = 0;
                PlayAttack(false);
                isAttacking = true;
            }
        }

        public override void Hide()
        {
            base.Hide();
            isAttacking = false;
            needNextAttack = false;
        }

        public int GetCurrentAmmo()
        {
            return 0;
        }

        public int GetMaxAmmo()
        {
            return 0;
        }

        public string GetCurrentAmmoString()
        {
            return InfinitySymbol;
        }

        public string GetCapacityAmmoString()
        {
            return InfinitySymbol;
        }

        public Sprite GetCurrentModeSprite()
        {
            return null;
        }

        public override BaseItem CreateItem()
        {
            return new KnifeItem("Item_Knife");
        }
    }

    public class KnifeItem : BaseItem
    {
        public KnifeItem(string itemName) : base(itemName)
        {
            MaxCapacity = 1;
        }
    }

}