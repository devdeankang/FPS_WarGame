using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace WarGame
{
    [System.Serializable]
    public class RifleController : WeaponController, IWeaponAmmo, IWeaponShootMode
    {
        public enum ShootingMode
        {
            Once,
            SemiAuto,
            Auto
        }

        #region StringParams

        public string FovParameter = "FOV";
        public string FirstTimeAnim = "RifleFirstDeploy";
        public string ReloadAnim = "RifleReload";

        // Aim
        public string AimAnim = "RifleAimBasic";
        public string FromAimAnim = "RifleReAimBasic";
        public string AimInAimAnim = "RifleAimInAimBasic";
        public string AimCentralAnimFromIdle = "RifleCentralAimFromIdle";
        public string FromCentralAimToIdleAnim = "RifleCentralReAimToIdle";
        public string AimCentralAnimFromBasicAim = "RifleCentralAimFromBasicAim";
        public string FromCentralAimToBasicAimAnim = "RifleCentralAimToBasicAim";
        public string LowPoseAnim = "RifleLowPose";
        public string TopPoseAnim = "RifleTopPose";

        // Upgrade
        public string ToUpgradeAnim = "RifleUpgradeStart";
        public string FromUpgradeAnim = "RifleUpgradeEnd";
        public string UpgradeIdleAnim = "RifleUpgradeIdle";

        // Shot
        public string ShotAnim = "RifleShot";
        public string LastShotAnim = "RifleLastShot";
        public string ModeSwitchAnimation = "RifleSwitch";

        // Movement
        public string JumpAnim = "RifleJump";
        public string Landing = "RifleLanding";
        public string CrouchAnim = "RifleCrouch";
        public string StandUpAnim = "RifleStandUp";

        // Damage
        public string DamageAnim = "RifleDamage";
        public string[] StatesForDamage;

        // Sounds
        public string DefaultShoot = "RifleShot";
        public string SilenceShoot = "RifleShotMufler";
        public string DefaultLastShoot = "RifleLastShot";
        public string SilenceLastShoot = "RifleLastShotMufler";

        //
        public string EmptyParam = "Empty";
        public string PoseParam = "RiflePose";

        string rifleHitEffectPoolParam = "@Rifle HitEffect Pool";

        #endregion

        #region Variables
        [Header("Ammo")]
        public int AmmoCurrent = 40;
        public int AmmoMax = 40;
        public int AmmoCapacity { get { return ammoItem.GetCurrentCount(); } }

        [Header("Options")]
        public bool isCentralAim = true;
        public bool isBackReload = false;
        public bool isLowPoseInAiming = false;
        public bool isTopPoseInAiming = true;
        public bool isAim4X = false;
        public bool isAim12X = false;
        public bool isCollimator = false;
        public bool isSilence = false;

        [Header("Shooting")]
        public ShootingMode ShootMode = ShootingMode.Auto;
        public int SemiAutoBurst = 3;
        public float SemiAutoInterval = 0.35f;

        [Header("Poses blending")]
        public float LowPoseWeightTarget = 0.08f;
        public float LowPoseWeightBlendSpeed = 0.6f;
        public float TopPoseWeightTarget = 0.115f;
        public float TopPoseWeightZoomedAim4XTarget = 0.22f;
        public float TopPoseWeightZoomedAim12XTarget = 0.13f;
        public float TopPoseWeightBlendSpeed = 0.6f;

        [Header("Fire")]
        public float DamageAmount = 25f;
        public LayerMask FireCollisionLayer;
        public Transform FirePoint;
        public Transform FirePointAim4X;
        public Transform FirePointAim12X;
        public Transform FirePointCollimator;
        public GameObject GunHitDecalPrefab;
        public Vector2 RecoilAmountMin = new Vector2(0.3f, 0.2f);
        public Vector2 RecoilAmountMax = new Vector2(-0.3f, 0.3f);
        public AnimationCurve RecoilCurve;
        public float RecoilFadeOutTime = 0.1f;
        public float AimRecoilMultiplier = 0.3f;
        public float autoFireRate = 0.1f;

        [Header("Aiming")]
        public LerpToRoot HandsLerp;
        public float HandsLerpMultiplier = 3f;
        public float HeadbobWeightInAim = 0.25f;
        public float AimingNoise = 0.02f;
        public TransformNoise CameraNoise;
        public float SensivityInAimInZoom = 0.2f;
        public float SensivityInAimInZoom12X = 0.05f;
        public float SensivityInAim = 0.7f;
        public float DofSmoothSpeed = 17.5f;
        public PostProcessingController.DepthOfFieldSettings AimingDof;
        public PostProcessingController.DepthOfFieldSettings AimingZoomDof;

        [Header("Death props")]
        public Rigidbody PropsBody;
        public Collider[] PropsCollision;
        public float DetachPropsForce;
        public Transform DetachPropsDirection;

        TransformStateSetupper propsTransformState;
        float recoilTime = 0;
        Vector2 currentRecoil = new Vector2();

        [Header("Animations Set")]
        public float RiflePoseBlendSpeed = 2f;
        public float RifleEmptyBlendSpeed = 6f;
        public float FovSmoothSpeed = 20f;
        public float FromAimDelay = 15f;
        public float DefaultFov = 60f;
        public float AimFov = 35f;
        public float AimFovCentral = 16f;

        [Header("4X Aim Blender")]
        public Material AimLensMaterial;
        public string SpecularColorProperty = "_SpecColor";
        public string EmissionColorProperty = "_EmissionColor";
        public Color SpecularDefault;
        public Color SpecularZoomed;
        public Color EmissionDefault;
        public Color EmissionZoomed;
        public float MinAngleFromCamera = 0f;
        public float MaxAngleFromCamera = 5f;
        public Transform AimForwardReference;
        public Transform CameraForwardReference;

        [Header("Common")]
        public Sprite[] ShootModeSprites;
        public GameObject Gunshot;
        public GameObject GunshotSilencer;
        public float GunShotLiveTime = 0.05f;
        public PlayerController Controller;
        public WeaponCustomization Customization;
        public MeshRenderer Sphere;
        public Rigidbody Shell;
        public Transform ShellForceDir;
        public float ShellForceMin = 10;
        public float ShellForceMax = 15;
        public float ShellRandomAngleMin = -5;
        public float ShellRandomAngleMax = 5;
        public Transform ShellTorqueDir;
        public float ShellTorque = -1200f;        
        
        float cameraFov = 60f;
        public GameObject propsRoot;
        public GameObject propsRootFirstDeploy;
        public bool firstTimeDeploy;

        bool isAiming = false;
        bool isIdle = true;
        bool isReloading = false;
        bool isUpgrading = false;
        bool needSmoothInAim = false;
        bool needOtherPose = false;

        float toIdleElapsedTime = 0;
        float fromAimTime = 0;
        float currentOtherPoseWeight = 0;
        float nextFireTime;
        float nextBurstTime;
        int burstCount = 0;
        bool flashLightIsOn = false;
        Light flashlightLight;
        MeshRenderer flashLightVolumetric;
        int poolSize = 100;
        float hitDecalLifetime = 20f;
        float riflePose;
        float emptyValue = 0;
        float emptyTargetValue = 0;
        int targetRiflePose;

        Queue<GameObject> gunHitDecalPool = new Queue<GameObject>();
        Rigidbody clonedPropsBody;
        RifleAmmoItem ammoItem;

        #endregion

        public float CameraFov { get { return cameraFov; } }

        public bool IsAiming { get { return isAiming; } }

        public bool IsReloading { get { return isReloading; } }

        float TopPoseTarget
        {
            get
            {
                if (isAim12X)
                    return TopPoseWeightZoomedAim12XTarget;

                if (isAim4X)
                    return TopPoseWeightZoomedAim4XTarget;

                return TopPoseWeightTarget;
            }
        }

        public string ShootSound
        {
            get
            {
                if (isSilence)
                {
                    return SilenceShoot;
                }
                else
                {
                    return DefaultShoot;
                }
            }
        }

        public string LastShootSound
        {
            get
            {
                if (isSilence)
                {
                    return SilenceLastShoot;
                }
                else
                {
                    return DefaultLastShoot;
                }
            }
        }

        public float CurrentAimFov
        {
            get
            {
                return isCentralAim ? AimFovCentral : AimFov;
            }
        }



        public override void Init(GameObject root)
        {
            base.Init(root);
            firstTimeDeploy = true;

            InitRifleParams();

            propsTransformState = PropsBody.GetComponent<TransformStateSetupper>();

            InitPool();
            Controller.JumpStartEvent += StartJump;
            Controller.JumpEndEvent += EndJump;

            Controller.CrouchEvent += Crouch;
            Controller.StandUpEvent += Standup;

            Controller.DamageHandler.DamagedEvent.AddListener(DamagedEvent);
            Controller.DamageHandler.ResurrectEvent.AddListener(ResurrectEvent);

            if (!propsTransformState.CalculateVelocity)
            {
                Debug.LogWarning("Weapon velocity calculation disabled, it's enables automatically", PropsBody);
                propsTransformState.CalculateVelocity = true;
            }

            ammoItem = new RifleAmmoItem("Item_Rifle_Ammo");
            playerInventory.AddItem(ammoItem);
            Controller.PlayerFreezeChanged.AddListener(FreezeChanged);

            hitEffectPool.name = rifleHitEffectPoolParam;
            
        }

        void InitRifleParams()
        {
            deployedParameter = "Rifle";
            deployAnim = "RifleDeploy";
            hideAnim = "RifleHide";
            hideCrossFade = 0.1f;
        }

        void FreezeChanged(bool isFreezed)
        {
            if (!isFreezed)
            {
                if (isUpgrading)
                {
                    Controller.Freeze(true);
                }
            }
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
            if (!isDeployed || isReloading)
                return;

            if (AmmoCurrent == 0)
            {
                emptyTargetValue = 1f;
            }
            else
            {
                emptyTargetValue = 0f;
            }

            AnimatorStateInfo currentState = handsAnimator.GetCurrentAnimatorStateInfo(0);
            foreach(string s in StatesForDamage)
            {
                if(currentState.IsName(s))
                {
                    handsAnimator.Play(DamageAnim);
                    break;
                }
            }
        }

        public virtual void Crouch()
        {
            if (!IsDeployed || isReloading || !IsCanControl)
                return;

            if (AmmoCurrent == 0)
            {
                emptyTargetValue = 1f;
            }
            else
            {
                emptyTargetValue = 0f;
            }

            handsAnimator.CrossFadeInFixedTime(CrouchAnim, 0.1f, 0);
        }

        public virtual void Standup()
        {
            if (!IsDeployed || isReloading || !IsCanControl)
                return;

            if (AmmoCurrent == 0)
            {
                emptyTargetValue = 1f;
            }
            else
            {
                emptyTargetValue = 0f;
            }

            handsAnimator.CrossFadeInFixedTime(StandUpAnim, 0.1f, 0);
        }

        public void SetAiming(bool onlyBasic = true, bool aim4X = false, bool aim12X = false, bool collimator = false)
        {
            if(onlyBasic)
            {
                isTopPoseInAiming = false;
                isCentralAim = false;
            }
            else
            {
                isTopPoseInAiming = true;
                isCentralAim = true;
            }

            if (aim12X)
            {
                isAim4X = false;
                isAim12X = true;
                isCollimator = false;
            }
            else if (aim4X)
            {
                isAim4X = true;
                isAim12X = false;
                isCollimator = false;
            }
            else if(collimator)
            {
                isCollimator = true;
                isAim4X = false;
                isAim12X = false;
            }
            else
            {
                isCollimator = false;
                isAim4X = false;
                isAim12X = false;
            }
        }

        public virtual void StartJump()
        {
            if (!IsDeployed || !isCanControl)
                return;

            if (AmmoCurrent == 0)
            {
                emptyTargetValue = 1f;
            }
            else
            {
                emptyTargetValue = 0f;
            }

            if (!isReloading)
            {
                handsAnimator.SetBool("JumpFall", false);
                handsAnimator.CrossFadeInFixedTime(JumpAnim, 0.1f, 0);
            }
        }

        public virtual void EndJump()
        {
            if (IsDeployed || isReloading || !isCanControl)
                return;

            if (AmmoCurrent == 0)
            {
                emptyTargetValue = 1f;
            }
            else
            {
                emptyTargetValue = 0f;
            }

            handsAnimator.SetBool("JumpFall", true);
        }

        public virtual void InitPool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject instance = GameObject.Instantiate(GunHitDecalPrefab);                
                instance.transform.SetParent(hitEffectPool.transform);
                instance.SetActive(false);

                gunHitDecalPool.Enqueue(instance);
            }
        }

        public virtual GameObject SpawnHitDecal()
        {
            if (gunHitDecalPool.Count == 0)
            {
                GameObject instance = GameObject.Instantiate(GunHitDecalPrefab);
                instance.transform.SetParent(hitEffectPool.transform);
                instance.SetActive(false);

                gunHitDecalPool.Enqueue(instance);
            }

            return gunHitDecalPool.Dequeue();
        }

        public void SetCameraFov(float fov)
        {
            cameraFov = fov;
        }

        Vector2 GetRandomVector(Vector2 min, Vector2 max)
        {
            return new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
        }

        void ChangeShootMode()
        {
            if (IsReloading)
                return;

            if (AmmoCurrent == 0)
            {
                emptyTargetValue = 1f;
            }
            else
            {
                emptyTargetValue = 0f;
            }
            emptyValue = emptyTargetValue;

            handsAnimator.CrossFadeInFixedTime(ModeSwitchAnimation, 0.1f, 0);
            int shootMode = (int)ShootMode;
            shootMode++;
            if (shootMode > 2)
            {
                shootMode = 0;
            }
            ShootMode = (ShootingMode)shootMode;
        }

        public override void Update(float deltaTime)
        {
            if (isUpgrading)
                fromAimTime = Time.time + FromAimDelay;

            if (Controller.IsFreezed || !IsDeployed)
                return;

            base.Update(deltaTime);

            Vector3 forwardDirection = AimForwardReference.position - CameraForwardReference.position;
            forwardDirection.Normalize();

            float angle = Vector3.Angle(CameraForwardReference.forward, forwardDirection);

            float angleFraction = angle / MaxAngleFromCamera;

            angleFraction = Mathf.Clamp01(angleFraction);
            Color specularColor = Color.Lerp(SpecularDefault, SpecularZoomed, 1f - angleFraction);
            Color emissionColor = Color.Lerp(EmissionDefault, EmissionZoomed, 1f - angleFraction);
            AimLensMaterial.SetColor(SpecularColorProperty, specularColor);
            AimLensMaterial.SetColor(EmissionColorProperty, emissionColor);

            if (isAiming)
            {
                if (isCentralAim)
                {
                    targetRiflePose = 2;
                }
                else
                {
                    targetRiflePose = 1;
                }
            }
            else
            {
                if (isIdle)
                {
                    targetRiflePose = 0;
                }
                else
                {
                    targetRiflePose = 1;
                }
            }

            if (isCanControl)
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    ChangeShootMode();
                }
            }

            AnimatorStateInfo nextState = handsAnimator.GetNextAnimatorStateInfo(0);
            if (nextState.fullPathHash == 0)
            {
                riflePose = Mathf.MoveTowards(riflePose, targetRiflePose, RiflePoseBlendSpeed * deltaTime);
                handsAnimator.SetFloat(PoseParam, riflePose);
            }

            if (propsRoot.activeSelf)
            {
                emptyValue = Mathf.MoveTowards(emptyValue, emptyTargetValue, RifleEmptyBlendSpeed * deltaTime);
                handsAnimator.SetFloat(EmptyParam, emptyValue);
            }


            if (isAiming)
            {
                if (lastInputData.MouseSecondHold)
                {
                    CameraNoise.NoiseAmount = Mathf.MoveTowards(CameraNoise.NoiseAmount, AimingNoise, deltaTime * 5f);
                    fromAimTime = Time.time + FromAimDelay;
                    cameraFov = Mathf.Lerp(cameraFov, Mathf.Lerp(DefaultFov, CurrentAimFov, handsAnimator.GetFloat(FovParameter)), deltaTime * FovSmoothSpeed);

                    if (isAim4X || isAim12X)
                    {
                        Controller.PPController.LerpDof(Controller.PPController.DofSettings, AimingZoomDof, DofSmoothSpeed * deltaTime);
                    }
                    else
                    {
                        Controller.PPController.LerpDof(Controller.PPController.DofSettings, AimingDof, DofSmoothSpeed * deltaTime);
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        fromAimTime = 0;
                    }
                    toIdleElapsedTime = 0;
                    if (Time.time >= fromAimTime && fromAimTime >= 0)
                    {
                        FromAim();
                    }
                    cameraFov = Mathf.Lerp(cameraFov, DefaultFov, deltaTime * FovSmoothSpeed);
                    Controller.PPController.LerpDof(Controller.PPController.DofSettings, Controller.DefaultDofSettings, DofSmoothSpeed * deltaTime);
                }
            }
            else
            {
                Controller.PPController.LerpDof(Controller.PPController.DofSettings, Controller.DefaultDofSettings, DofSmoothSpeed * deltaTime);

                if (isIdle)
                {
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftShift) && !Input.GetMouseButton(0))
                    {
                        fromAimTime = 0;
                    }
                    toIdleElapsedTime = 0;
                    if (Time.time >= fromAimTime && fromAimTime >= 0)
                    {
                        FromAim();
                    }

                }
                cameraFov = Mathf.Lerp(cameraFov, DefaultFov, deltaTime * FovSmoothSpeed);
            }

            // auto and semi
            if(isCanControl)
            {
                if (ShootMode == ShootingMode.Auto)
                {
                    if (lastInputData.MouseHold && AmmoCurrent > 0)
                    {
                        nextFireTime += Time.deltaTime;
                        if (nextFireTime >= autoFireRate)
                        {
                            Fire();
                        }
                    }
                }
                else if (ShootMode == ShootingMode.SemiAuto)
                {
                    if (AmmoCurrent == 0)
                    {
                        burstCount = 0;
                    }
                    else
                    {
                        if (burstCount > 0)
                        {
                            nextFireTime += Time.deltaTime;
                            if (nextFireTime >= autoFireRate)
                            {
                                Fire();
                            }
                        }
                        else
                        {
                            if (lastInputData.MouseHold)
                            {
                                nextBurstTime += Time.deltaTime;
                                if (nextBurstTime >= SemiAutoInterval)
                                {
                                    Fire();
                                }
                            }
                        }
                    }
                }
            }

            // recoil animation
            recoilTime += Time.deltaTime;
            float recoilFraction = recoilTime / RecoilFadeOutTime;
            float recoilValue = RecoilCurve.Evaluate(recoilFraction);

            currentRecoil = Vector2.Lerp(Vector2.zero, currentRecoil, recoilValue);

            Vector2 recoilResult = currentRecoil;
            Controller.mouseControl.LookRotation(recoilResult.x, recoilResult.y, deltaTime);

            // blend pose (for aiming)
            if (IsDeployed)
            {
                if (isLowPoseInAiming)
                {
                    currentOtherPoseWeight = Mathf.MoveTowards(currentOtherPoseWeight, (needOtherPose && isDeployed) ? LowPoseWeightTarget : 0, LowPoseWeightBlendSpeed * deltaTime);
                    handsAnimator.SetLayerWeight(1, currentOtherPoseWeight);
                }
                else if(isTopPoseInAiming)
                {
                    currentOtherPoseWeight = Mathf.MoveTowards(currentOtherPoseWeight, (needOtherPose && isDeployed) ? TopPoseTarget : 0, TopPoseWeightBlendSpeed * deltaTime);
                    handsAnimator.SetLayerWeight(1, currentOtherPoseWeight);
                }
            }
        }

        public void FirstDeployEnd()
        {
            propsRoot.SetActive(true);
            propsRootFirstDeploy.SetActive(false);
        }

        public override void Deploy()
        {
            if (Controller.IsFreezed)
                return;

            if (LockControlOnDeploy)
                isCanControl = false;

            lastInputData = default(HandsControllerInput);
            currentInputData = default(HandsControllerInput);
            fromAimTime = 0;
            isAiming = false;
            isIdle = true;
            riflePose = 0;
            if (firstTimeDeploy)
            {
                if (isDeployed)
                    return;

                if (AmmoCurrent == 0)
                {
                    emptyTargetValue = 1f;
                }
                else
                {
                    emptyTargetValue = 0f;
                }

                handsAnimator.Play(FirstTimeAnim);
                SetDeployed(true);

                propsRoot.SetActive(false);
                propsRootFirstDeploy.SetActive(true);
            }
            else
            {
                base.Deploy();

                if (AmmoCurrent == 0)
                {
                    emptyTargetValue = 1f;
                }
                else
                {
                    emptyTargetValue = 0f;
                }

                propsRootFirstDeploy.SetActive(false);
                propsRoot.SetActive(true);
            }
        }

        public override void HideProps()
        {
            propsRoot.SetActive(false);
            propsRootFirstDeploy.SetActive(false);
        }

        public override void ShowProps()
        {
            if (firstTimeDeploy)
            {
                propsRoot.SetActive(false);
                propsRootFirstDeploy.SetActive(true);
            }
            else
            {
                propsRoot.SetActive(true);
                propsRootFirstDeploy.SetActive(false);
            }
        }

        public override void SetDeployed(bool value)
        {
            base.SetDeployed(value);
            if (isLowPoseInAiming)
            {
                handsAnimator.Play(LowPoseAnim, 1);
            }
            if (isTopPoseInAiming)
            {
                handsAnimator.Play(TopPoseAnim, 1);
            }
            if (LockControlOnDeploy)
                isCanControl = false;
        }

        public override void Hide()
        {
            if (!isDeployed || Controller.IsFreezed)
                return;

            base.Hide();

            firstTimeDeploy = false;

            toIdleElapsedTime = 0;
            if (AmmoCurrent == 0)
            {
                emptyTargetValue = 1f;
            }
            else
            {
                emptyTargetValue = 0f;
            }
            isAiming = false;
            isReloading = false;

            Controller.SetSensivityMultiplier(1);
        }

        public override void InputEvent(InputEventType et)
        {
            if (!propsRoot.activeSelf)
                return;

            base.InputEvent(et);

            if (!isCanControl)
                return;

            if (et == InputEventType.Reload)
            {
                Reload();
            }

            if (et == InputEventType.MouseDown)
            {
                if (IsReloading)
                    return;

                if (AmmoCurrent == 0)
                {
                    emptyTargetValue = 1f;
                }
                else
                {
                    emptyTargetValue = 0f;
                }
                emptyValue = emptyTargetValue;

                Fire();
            }

            if (et == InputEventType.MouseHold)
            {

            }

            if (et == InputEventType.MouseSecondDown)
            {
                if (IsReloading)
                    return;

                if (AmmoCurrent == 0)
                {
                    emptyTargetValue = 1f;
                }
                else
                {
                    emptyTargetValue = 0f;
                }
                emptyValue = emptyTargetValue;

                Aim();
            }

            if (et == InputEventType.MouseSecondHold)
            {
                if (IsReloading)
                    return;

                if (AmmoCurrent == 0)
                {
                    emptyTargetValue = 1f;
                }
                else
                {
                    emptyTargetValue = 0f;
                }
                emptyValue = emptyTargetValue;

                UpdateAim();
            }

            if (et == InputEventType.MouseSecondUp)
            {
                if (IsReloading)
                    return;

                if (AmmoCurrent == 0)
                {
                    emptyTargetValue = 1f;
                }
                else
                {
                    emptyTargetValue = 0f;
                }
                emptyValue = emptyTargetValue;

                FromAim();
            }

            if (et == InputEventType.Upgrade)
            {
                if (IsReloading)
                    return;

                if (AmmoCurrent == 0)
                {
                    emptyTargetValue = 1f;
                }
                else
                {
                    emptyTargetValue = 0f;
                }
                emptyValue = emptyTargetValue;

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

        public virtual void Reload()
        {
            if (isReloading || !isDeployed || Controller.IsFreezed)
                return;

            if (AmmoCurrent == AmmoMax || AmmoCapacity == 0)
                return;

            if (AmmoCurrent == 0)
            {
                emptyTargetValue = 1f;
            }
            else
            {
                emptyTargetValue = 0f;
            }
            handsAnimator.CrossFadeInFixedTime(ReloadAnim, 0.1f, 0);

            FromUpgrade(false);
            FromAim(false);
            isIdle = false;
            isReloading = true;
            fromAimTime = Time.time + FromAimDelay;
        }

        public virtual void Fire()
        {
            if (isReloading || !isDeployed || Controller.IsFreezed)
                return;

            if (isAiming)
            {
                if (isCentralAim)
                {
                    targetRiflePose = 2;
                }
                else
                {
                    targetRiflePose = 1;
                }
            }
            else
            {
                if (isIdle)
                {
                    targetRiflePose = 0;
                }
                else
                {
                    targetRiflePose = 1;
                }
            }
            if (riflePose != targetRiflePose)
            {
                riflePose = targetRiflePose;
                handsAnimator.SetFloat(PoseParam, riflePose);
            }

            isIdle = false;
            if (AmmoCurrent == 1)
            {
                handsAnimator.Play(LastShotAnim, 0, 0);
            }
            else
            {
                handsAnimator.Play(ShotAnim, 0, 0);
            }

            if (AmmoCurrent > 0)
            {
                currentRecoil += GetRandomVector(RecoilAmountMin, RecoilAmountMax) * (isAiming ? AimRecoilMultiplier : 1f);
                recoilTime = 0;
                InstantiateGunShot();
                EmitShell();
                EmitHitEffect();
                AmmoCurrent--;
            }

            if (AmmoCurrent == 0)
            {
                emptyTargetValue = 1f;
            }
            else
            {
                emptyTargetValue = 0f;
            }

            fromAimTime = Time.time + FromAimDelay;
            nextFireTime = 0;
            if(ShootMode == ShootingMode.SemiAuto)
            {
                if (burstCount == 0)
                {
                    nextBurstTime = 0;
                    burstCount = SemiAutoBurst - 1;
                }
                else
                {
                    burstCount--;
                }
            }
        }

        void EmitHitEffect()
        {
            Vector3 position = FirePoint.position;
            Vector3 direction = FirePoint.forward;

            if (isAiming)
            {
                if (isAim4X)
                {
                    position = FirePointAim4X.position;
                    direction = FirePointAim4X.forward;
                }
                else if (isAim12X)
                {
                    position = FirePointAim12X.position;
                    direction = FirePointAim12X.forward;
                } else if(isCollimator)
                {
                    position = FirePointCollimator.position;
                    direction = FirePointCollimator.forward;
                }
            }

            Ray r = new Ray(position, direction);
            RaycastHit hitInfo;

            if (Physics.Raycast(r, out hitInfo, 1000f, FireCollisionLayer, QueryTriggerInteraction.Ignore))
            {
                IHittableObject hittable = hitInfo.collider.GetComponent<IHittableObject>();

                if (hittable == null)
                {
                    hitInfo.collider.GetComponentInParent<IHittableObject>();
                }

                DecalOnHit dOnHit = hitInfo.collider.GetComponent<DecalOnHit>();

                if (dOnHit == null && !Controller.IsHumanoidTarget(hitInfo.collider.gameObject))
                {
                    GameObject decal = SpawnHitDecal();
                    decal.SetActive(true);
                    decal.GetComponent<TimedCallbackEvent>().DestroyWithDelay(hitDecalLifetime, () => DecalDestroyed(decal));
                    decal.transform.position = hitInfo.point;
                    decal.transform.forward = hitInfo.normal;
                    decal.transform.SetParent(hitInfo.transform);
                }

                if (hittable != null)
                {
                    DamageData damage = new DamageData();
                    damage.DamageAmount = DamageAmount;
                    damage.HitDirection = r.direction;
                    damage.HitPosition = hitInfo.point;
                    hittable.TakeDamage(damage);
                }
            }
        }

        void DecalDestroyed(GameObject decal)
        {            
            decal.transform.SetParent(hitEffectPool.transform);
            decal.SetActive(false);
            gunHitDecalPool.Enqueue(decal);
        }

        void InstantiateGunShot()
        {
            GameObject gunshotPrefab = isSilence ? GunshotSilencer : Gunshot;

            GameObject instance = GameObject.Instantiate(gunshotPrefab, gunshotPrefab.transform.position, gunshotPrefab.transform.rotation, gunshotPrefab.transform.parent);
            instance.SetActive(true);

            GameObject.Destroy(instance, GunShotLiveTime);
        }

        void EmitShell()
        {
            Rigidbody shellInstance = GameObject.Instantiate(Shell, Shell.transform.position, Shell.transform.rotation);

            shellInstance.gameObject.SetActive(true);
            shellInstance.transform.SetParent(null);
            shellInstance.velocity = Controller.PlayerVelocity;
            Quaternion randomRotation = Quaternion.Euler(Random.Range(ShellRandomAngleMin, ShellRandomAngleMax), Random.Range(ShellRandomAngleMin, ShellRandomAngleMax), Random.Range(ShellRandomAngleMin, ShellRandomAngleMax));
            shellInstance.AddForce(randomRotation * ShellForceDir.forward * Random.Range(ShellForceMin, ShellForceMax));
            shellInstance.AddTorque(ShellTorqueDir.forward * ShellTorque);
            shellInstance.maxAngularVelocity = 105;
        }

        public virtual void Aim()
        {
            if (isReloading || !isDeployed || Controller.IsFreezed)
                return;

            toIdleElapsedTime = 0;
            if (isLowPoseInAiming)
            {
                handsAnimator.Play(LowPoseAnim, 1);
            }
            if (isTopPoseInAiming)
            {
                handsAnimator.Play(TopPoseAnim, 1);
            }
            if (isAiming || (!isIdle && isCentralAim))
            {
                needSmoothInAim = true;
                if (AmmoCurrent == 0)
                {
                    emptyTargetValue = 1f;
                }
                else
                {
                    emptyTargetValue = 0f;
                }
                FromUpgrade(false);

                if(isCentralAim)
                {
                    handsAnimator.Play(AimCentralAnimFromBasicAim, 0, 0);
                }
                else
                {
                    handsAnimator.CrossFadeInFixedTime(AimInAimAnim, 0.075f, 0);
                }

                isAiming = true;
            }
            else
            {
                if (AmmoCurrent == 0)
                {
                    emptyTargetValue = 1f;
                }
                else
                {
                    emptyTargetValue = 0f;
                }

                if (isCentralAim)
                {
                    handsAnimator.CrossFadeInFixedTime(AimCentralAnimFromIdle, 0.075f, 0);
                }
                else
                {
                    handsAnimator.CrossFadeInFixedTime(AimAnim, 0.075f, 0);
                }

                fromAimTime = -1;
                isAiming = true;
                FromUpgrade(false);
            }
            isIdle = false;

            if (isLowPoseInAiming || isTopPoseInAiming)
            {
                needOtherPose = true;
            }

            HandsLerp.Multiplier = HandsLerpMultiplier;
            Controller.handsHeadbobMultiplier = HeadbobWeightInAim;

            if (isAim4X)
            {
                Controller.SetSensivityMultiplier(SensivityInAimInZoom);
            }
            else if (isAim12X)
            {
                Controller.SetSensivityMultiplier(SensivityInAimInZoom12X);
            }
            else
            {
                Controller.SetSensivityMultiplier(SensivityInAim);
            }
        }

        public virtual void FromAim(bool withAnim = true)
        {
            if (isReloading)
                return;

            HandsLerp.Multiplier = 1;
            toIdleElapsedTime = 0;
            if (isAiming || !isIdle)
            {
                needSmoothInAim = false;
                if (isCentralAim)
                {
                    if (Time.time >= fromAimTime && fromAimTime >= 0)
                    {
                        if (withAnim)
                        {
                            if (isAiming)
                            {
                                handsAnimator.CrossFadeInFixedTime(FromCentralAimToIdleAnim, 0.075f, 0);
                            }
                            else
                            {
                                handsAnimator.CrossFadeInFixedTime(FromAimAnim, 0.075f, 0);
                            }
                        }
                        fromAimTime = -1;
                        isAiming = false;
                        isIdle = true;
                    }
                    else
                    {
                        if (withAnim)
                        {
                            handsAnimator.CrossFadeInFixedTime(FromCentralAimToBasicAimAnim, 0.075f, 0);
                        }
                        isAiming = false;
                        isIdle = false;
                    }
                }
                else
                {
                    if (Time.time >= fromAimTime && fromAimTime >= 0)
                    {
                        if (withAnim)
                        {
                            handsAnimator.CrossFadeInFixedTime(FromAimAnim, 0.075f, 0);
                        }
                        fromAimTime = -1;
                        isAiming = false;
                        isIdle = true;
                    }
                    else
                    {
                        fromAimTime = Time.time + FromAimDelay;

                        if(withAnim)
                            handsAnimator.CrossFadeInFixedTime(AimInAimAnim, 0.075f, 0);
                    }
                }
            }
            if (isLowPoseInAiming || isTopPoseInAiming)
            {
                needOtherPose = false;
            }
            Controller.handsHeadbobMultiplier = 1f;
            Controller.SetSensivityMultiplier(1f);
        }

        public virtual void UpdateAim()
        {

        }

        public virtual void ToUpgrade()
        {
            if (isReloading)
                return;

            Sphere.enabled = true;
            Customization.ShowModifications();
            isUpgrading = true;
            FromAim(false);
            handsAnimator.CrossFadeInFixedTime(ToUpgradeAnim, 0.25f, 0);
            Controller.Freeze(true);
        }

        public virtual void FromUpgrade(bool withAnim = true)
        {
            if (isReloading)
                return;

            Sphere.enabled = false;
            Customization.CloseModifications();
            isUpgrading = false;

            if (withAnim)
                handsAnimator.CrossFadeInFixedTime(FromUpgradeAnim, 0.25f, 0);

            Controller.Freeze(false);
            isIdle = false;
        }

        public void Reloaded()
        {
            fromAimTime = Time.time + FromAimDelay;
            int needToReload = AmmoMax - AmmoCurrent;
            int willBeReload = ammoItem.Consume(needToReload);

            AmmoCurrent += willBeReload;

            isReloading = false;
        }

        public void ReloadEnd()
        {
            fromAimTime = Time.time + FromAimDelay;
            if (AmmoCurrent == 0)
            {
                emptyTargetValue = 1f;
            }
            else
            {
                emptyTargetValue = 0f;
            }
        }

        public int GetCurrentAmmo()
        {
            return AmmoCurrent;
        }

        public int GetMaxAmmo()
        {
            return AmmoMax;
        }

        public string GetCurrentAmmoString()
        {
            return AmmoCurrent.ToString();
        }

        public string GetCapacityAmmoString()
        {
            return AmmoCapacity.ToString();
        }

        public Sprite GetCurrentModeSprite()
        {
            int shootMode = (int)ShootMode;

            return ShootModeSprites[shootMode];
        }
        public override BaseItem CreateItem()
        {
            return new RifleItem("Item_Rifle");
        }
    }

    public class RifleItem : BaseItem
    {
        public RifleItem(string itemName) : base(itemName)
        {
            MaxCapacity = 1;
        }
    }

    public class RifleAmmoItem : BaseItem
    {
        public RifleAmmoItem(string itemName) : base(itemName)
        {
            MaxCapacity = 600;
            IconName = "RifleAmmo";
        }
    }
}