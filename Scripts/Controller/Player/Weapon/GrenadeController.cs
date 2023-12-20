using UnityEngine;
using UnityEngine.UI;

namespace WarGame
{
    [System.Serializable]
    public class GrenadeController : WeaponController, IWeaponAmmo, IWeaponShootMode
    {
        #region StringParams

        string ThrowStartAnimParam = "GrenadeThrowStart";
        string ThrowAnimParam = "GrenadeThrow";
        string DamageAnimParam = "GrenadeDamage";
        string CrouchAnimParam = "GrenadeCrouch";
        string StandupAnimParam = "GrenadeStandUp";
        string ToUpgradeAnimParam = "GrenadeToUpgrade";
        string FromUpgradeAnimParam = "GrenadeFromUpgrade";
        string GrenadeParam = "Grenade";
        string GrenadeDeployParam = "GrenadeDeploy";
        string GrenadeHideParam = "GrenadeHide";

        #endregion

        #region Variables
        public GameObject HandsProps;
        public PlayerController Controller;

        public Rigidbody PropsBody;
        public Collider[] PropsCollision;
        public float DetachPropsForce = 50f;
        public Transform DetachPropsDirection;

        public Grenade GrenadeTemplate;
        public int GrenadesCount { get { return ammoItem.GetCurrentCount(); } }

        public Transform GrenadeThrowDirection;

        [SerializeField] MeshRenderer sphere;
        [SerializeField] WeaponCustomization customization;

        TransformStateSetupper propsTransformState;
        Rigidbody clonedPropsBody;
        GrenadeItem ammoItem;

        bool inLoad = false;
        bool isUpgrading = false;

        #endregion

        public override void Init(GameObject root)
        {
            base.Init(root);

            propsTransformState = PropsBody.GetComponent<TransformStateSetupper>();

            InitGrenadeParams();

            Controller.JumpStartEvent += StartJump;
            Controller.JumpEndEvent += EndJump;

            Controller.CrouchEvent += Crouch;
            Controller.StandUpEvent += StandUp;

            Controller.DamageHandler.DamagedEvent.AddListener(DamagedEvent);
            Controller.DamageHandler.ResurrectEvent.AddListener(ResurrectEvent);


            if (!propsTransformState.CalculateVelocity)
            {
                Debug.LogWarning("Weapon velocity calculation disabled, it's enables automatically", PropsBody);
                propsTransformState.CalculateVelocity = true;
            }
            ammoItem = HandsItem as GrenadeItem;

            if (hitEffectPool != null) GameObject.Destroy(hitEffectPool);
        }

        void InitGrenadeParams()
        {
            deployedParameter = GrenadeParam;
            deployAnim = GrenadeDeployParam;
            hideAnim = GrenadeHideParam;
            hideCrossFade = 0.1f;
        }

        void Crouch()
        {
            if (!isDeployed || inLoad || !isCanControl || !HandsProps.activeSelf)
                return;

            AnimatorStateInfo state = handsAnimator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName(ThrowAnimParam) || state.IsName(ThrowStartAnimParam))
                return;
            handsAnimator.Play(CrouchAnimParam);
        }

        void StandUp()
        {
            if (!isDeployed || inLoad || !isCanControl || !HandsProps.activeSelf)
                return;

            AnimatorStateInfo state = handsAnimator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName(ThrowAnimParam) || state.IsName(ThrowStartAnimParam))
                return;
            handsAnimator.Play(StandupAnimParam);
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
            if (GrenadesCount > 0)
            {
                clonedPropsBody = GameObject.Instantiate(PropsBody, PropsBody.transform.parent);
                clonedPropsBody.transform.SetParent(null);
                clonedPropsBody.gameObject.SetActive(true);
                clonedPropsBody.isKinematic = false;
                clonedPropsBody.velocity = propsTransformState.Velocity;
                clonedPropsBody.angularVelocity = propsTransformState.AngularVelocity;
                clonedPropsBody.AddForce(DetachPropsDirection.forward * DetachPropsForce);
            }
        }

        void DamagedEvent(DamageData damage)
        {
            if (!isDeployed || inLoad || !isCanControl || GrenadesCount == 0 || !HandsProps.activeSelf)
                return;

            AnimatorStateInfo state = handsAnimator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName(ThrowAnimParam) || state.IsName(ThrowStartAnimParam))
                return;

            handsAnimator.Play(DamageAnimParam);
        }

        public virtual void StartJump()
        {
            if (!IsDeployed || inLoad || !isCanControl || GrenadesCount == 0 || !HandsProps.activeSelf)
                return;

            AnimatorStateInfo state = handsAnimator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName(ThrowAnimParam) || state.IsName(ThrowStartAnimParam))
                return;

            handsAnimator.SetBool("JumpFall", false);
            handsAnimator.SetTrigger("Jump");
        }

        public virtual void EndJump()
        {
            if (!IsDeployed || inLoad || !isCanControl || GrenadesCount == 0 || !HandsProps.activeSelf)
                return;

            AnimatorStateInfo state = handsAnimator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName(ThrowAnimParam) || state.IsName(ThrowStartAnimParam))
                return;

            handsAnimator.SetBool("JumpFall", true);
            handsAnimator.ResetTrigger("Jump");
        }

        public void DeployedAnimationEvent()
        {
            ShowProps();
        }

        public override void Deploy()
        {
            base.Deploy();
            ShowProps();

            inLoad = false;
        }

        public override void HideProps()
        {
            HandsProps.SetActive(false);
        }

        public override void ShowProps()
        {
            if (GrenadesCount > 0)
                HandsProps.SetActive(true);
            else
                HandsProps.SetActive(false);
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
                    ToUpdate();
                }
            }
        }

        public virtual void ToUpdate()
        {
            if (inLoad || !HandsProps.activeSelf)
                return;

            sphere.enabled = true;
            customization.ShowModifications();
            isUpgrading = true;
            handsAnimator.CrossFadeInFixedTime(ToUpgradeAnimParam, 0.25f, 0);
            Controller.Freeze(true);
        }

        public virtual void FromUpgrade(bool withAnim = true)
        {
            sphere.enabled = false;
            customization.CloseModifications();
            isUpgrading = false;

            if (withAnim)
            {
                handsAnimator.CrossFadeInFixedTime(FromUpgradeAnimParam, 0.25f, 0);
            }

            Controller.Freeze(false);
        }

        public virtual void Attack()
        {
            if (inLoad || GrenadesCount <= 0)
                return;

            AnimatorStateInfo state = handsAnimator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName(ThrowAnimParam) || state.IsName(ThrowStartAnimParam))
                return;

            handsAnimator.SetBool("ThrowGrenade", false);
            handsAnimator.Play(ThrowStartAnimParam);
            inLoad = true;
        }

        public virtual void ThrowGrenade()
        {
            handsAnimator.Play(ThrowAnimParam);
            inLoad = false;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            handsAnimator.SetInteger("GrenadesCount", GrenadesCount);

            if (inLoad && !lastInputData.MouseHold)
            {
                AnimatorStateInfo state = handsAnimator.GetCurrentAnimatorStateInfo(0);
                if (state.IsName(ThrowStartAnimParam) && state.normalizedTime >= 0.9f)
                    ThrowGrenade();
            }
        }

        public override BaseItem CreateItem()
        {
            return new GrenadeItem("Item_Grenade");
        }

        public virtual void SpawnGrenade()
        {
            ammoItem.Consume(1);
            HideProps();
            Grenade g = GameObject.Instantiate(GrenadeTemplate, GrenadeTemplate.transform.parent);
            g.gameObject.SetActive(true);
            g.Throw(GrenadeThrowDirection.forward);
        }

        public override void Hide()
        {
            base.Hide();
        }

        public Sprite GetCurrentModeSprite()
        {
            return null;
        }

        public int GetCurrentAmmo()
        {
            if (GrenadesCount > 0)
                return 1;
            else
                return 0;
        }

        public int GetMaxAmmo()
        {
            return 1;
        }

        public string GetCurrentAmmoString()
        {
            if (GrenadesCount > 0)
                return "1";
            else
                return "0";
        }

        public string GetCapacityAmmoString()
        {
            if (GrenadesCount > 0)
                return (GrenadesCount - 1).ToString();
            else
                return "0";
        }
    }

    [System.Serializable]
    public class GrenadeItem : BaseItem
    {
        public GrenadeItem(string itemName) : base(itemName)
        {
            MaxCapacity = 6;
            IconName = "Grenade";
        }
    }
}