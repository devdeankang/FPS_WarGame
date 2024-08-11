using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WarGame
{
    [System.Serializable]
    public class PistolController : WeaponController, IWeaponAmmo, IWeaponShootMode, IAmmoItemGetter
    {
        #region Variables

        [Header("Ammo")]
        public int AmmoCurrent = 17;
        public int AmmoMaxDefault = 17;
        public int AmmoMaxLong = 33;
        public int AmmoMax = 17;
        public int AmmoCapacity { get { return ammoItem.GetCurrentCount(); } }
        public float DamageAmount = 10;

        [Header("Options")]
        public bool isCentralAim = false;
        public bool isBackReload = false;
        public bool isLowPoseInAiming = false;
        public bool isLongReload = false;
        public bool isSilence = false;
        public bool isAuto = false;
        public bool isFlashlight = false;
        public float LowPoseWeightTarget = 0.09f;
        public float LowPoseWeightBlendSpeed = 0.2f;
        public LayerMask FireCollisionLayer;
        public Transform FirePoint;        
        public GameObject GunHitDecalPrefab;    // need to delete
        public LerpToRoot HandsLerp;
        public AnimationCurve RecoilCurve;
        public Vector2 RecoilAmountMin = new Vector2(0.3f, 1f);
        public Vector2 RecoilAmountMax = new Vector2(-0.3f, 1.2f);
        public float RecoilFadeOutTime = 0.1f;
        public float AimRecoilMultiplier = 0.3f;
        public float AimingNoise = 0.1f;
        public TransformNoise CameraNoise;
        public Rigidbody PropsBody;
        public Collider[] PropsCollision;
        public float DetachPropsForce = 100f;
        public Transform DetachPropsDirection;

        [Header("Animations")]
        [SerializeField] float fovSmoothSpeed = 10f;
        [SerializeField] float fromAimDelay = 7f;
        [SerializeField] float defaultFov = 60f;
        [SerializeField] float aimFov = 35f;
        [SerializeField] float aimFovCentral = 30f;
        
        [Header("Common")]
        public Sprite ShootModeSprite;
        public RuntimeAnimatorController ControllerDefault;
        public AnimatorOverrideController ControllerCentral;
        [SerializeField] GameObject gunshot;
        [SerializeField] GameObject gunshotSilencer;
        [SerializeField] float GunShotLiveTime = 0.1f;
        [SerializeField] PlayerController controller;
        [SerializeField] WeaponCustomization customization;
        [SerializeField] MeshRenderer sphere;
        [SerializeField] Rigidbody shell;
        [SerializeField] Transform shellForceDir;
        [SerializeField] Transform shellTorqueDir;
        float shellForceMin = 5f;
        float shellForceMax = 8f;
        float shellTorque = 600f;        

        [Header("Other")]
        public GameObject VisualBulletFirstTime;
        public GameObject VisualBullets;
        [SerializeField] GameObject propsRoot;
        protected bool firstTimeDeploy;

        bool isAiming = false;
        bool isReloading = false;
        bool isUpgrading = false;
        bool isOnFlashlight = false;
        bool needSmoothInAim = false;
        bool needLowPose = false;

        float cameraFov = 60f;
        float toIdleElapsedTime = 0;
        float fromAimTime = 0;
        float currentLowPoseWeight = 0;
        float autoFireRate = 0.1f;
        float nextFireTime;
        int poolSize = 100;
        float hitDecalLifetime = 20f;
        float recoilTime = 0;
        Vector2 currentRecoil = new Vector2();

        DataForPool poolData = default;
        TransformStateSetupper propsTransformState;
        Queue<GameObject> gunHitDecalPool = new Queue<GameObject>();
        Light flashlightLight;
        MeshRenderer flashLightVolumetric;
        Rigidbody clonedPropsBody;
        PistolAmmoItem ammoItem;

        #endregion

        #region StringParams

        public string FromAimDefaultAnim = "FromAimDefault";
        public string CentralToBasicAnim = "PistolCentralToBasic";
        string fovParameter = "FOV";
        string firstTimeAnim = "FirstTime";
        string reloadAnim = "Reload";
        string reloadBackAnim = "ReloadBack";
        string reloadLongAnim = "ReloadLong";
        string reloadLongBackAnim = "ReloadLongBack";
        string aimAnim = "Aim";
        string fromAimAnim = "FromAim";
        string fromAimToIdleAnim = "FromAimDefault";
        string aimInAimAnim = "AimInAim";
        string aimFromAimAnim = "AimFromAim";
        string basicToCentralAimAnim = "PistolBasicToCentral";
        string toUpgradeAnim = "UpgradeDeploy";
        string fromUpgradeAnim = "UpgradeHide";
        string lowposeAnim = "LowPose";

        [HideInInspector] public string UpgradeIdleAnim = "UpgradeIdle";

        string shotAnim = "Shot";
        string shotWithoutAimAnim = "ShotWithoutAim";
        string lastShotWithoutAimAnim = "LastShotWithoutAim";
        string lastShotAnim = "LastShot";
        string jumpStartAimAnim = "JumpStartAim";
        string jumpEndAimAnim = "JumpEndAim";
        string jumpStartIdleAnim = "JumpStartIdle";
        string jumpEndIdleAnim = "JumpEndIdle";
        string jumpStartCentralAimAnim = "JumpStartAimCentral";
        string jumpEndCentralAimAnim = "JumpEndAimCentral";

        string damageAimAnim = "PistolDamageAim";
        string damageIdleAnim = "PistolDamageIdle";

        public string DefaultShoot = "PistolShoot";
        public string SilenceShoot = "PistolShootSilence";

        public string CrouchCentralAnim = "PistolCentralCrouch";
        public string CrouchAimAnim = "PistolAimCrouch";
        public string CrouchIdleAnim = "PistolIdleCrouch";
        public string StandUpCentralAnim = "PistolCentralCrouch";
        public string StandUpAimAnim = "PistolAimCrouch";
        public string StandUpIdleAnim = "PistolIdleCrouch";

        public string EmptyParam = "Empty";

        string pistolHitEffectPoolParam = "@Pistol HitEffect Pool";
        #endregion

        public float CameraFov { get { return cameraFov; } }

        public bool IsAiming { get { return isAiming && lastInputData.MouseSecondHold; } }

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

        public bool IsReloading
        {
            get
            {
                return isReloading;
            }
        }

        public float currentAimFov
        {
            get
            {
                return isCentralAim ? aimFovCentral : aimFov;
            }
        }

        public override void Init(GameObject root)
        {
            base.Init(root);

            propsTransformState = PropsBody.GetComponent<TransformStateSetupper>();

            InitPistolParams();

            firstTimeDeploy = true;

            InitPool();
            controller.JumpStartEvent += StartJump;
            controller.JumpEndEvent += EndJump;

            controller.CrouchEvent += Crouch;
            controller.StandUpEvent += Standup;

            controller.DamageHandler.DamagedEvent.AddListener(DamagedEvent);
            controller.DamageHandler.ResurrectEvent.AddListener(ResurrectEvent);

            if(!propsTransformState.CalculateVelocity)
            {
                Debug.LogWarning("Weapon velocity calculation disabled, it's enables automatically", PropsBody);
                propsTransformState.CalculateVelocity = true;
            }

            playerInventory.AddItem(new PistolAmmoItem("Item_Pistol_Ammo"));
            ammoItem = playerInventory.GetItem<PistolAmmoItem>();
            controller.PlayerFreezeChanged.AddListener(FreezeChanged);

            hitEffectPool.name = pistolHitEffectPoolParam;
        }

        void InitPistolParams()
        {
            deployedParameter = "Pistol";
            deployAnim = "Deploy";
            hideAnim = "Hide";
            hideCrossFade = 0.1f;
        }

        void FreezeChanged(bool isFreezed)
        {
            if (!isFreezed)
            {
                if (isUpgrading)
                {
                    controller.Freeze(true);
                }
            }
        }

        public void DisableVisualFirstTimeBullet()
        {
            VisualBulletFirstTime.SetActive(false);
        }

        public void EnableVisualBullets()
        {
            VisualBullets.SetActive(true);
        }

        public void DisableVisualBullets()
        {
            VisualBullets.SetActive(false);
        }

        public virtual void Crouch()
        {
            if (!IsDeployed || isReloading || !IsCanControl)
                return;

            if (AmmoCurrent == 0)
            {
                handsAnimator.SetFloat(EmptyParam, 1f);
            }
            else
            {
                handsAnimator.SetFloat(EmptyParam, 0f);
            }

            if (isCentralAim && isAiming && lastInputData.MouseSecondHold)
                handsAnimator.CrossFadeInFixedTime(CrouchCentralAnim, 0.1f, 0);
            else if(isAiming)
                handsAnimator.CrossFadeInFixedTime(CrouchAimAnim, 0.1f, 0);
            else
                handsAnimator.CrossFadeInFixedTime(CrouchIdleAnim, 0.1f, 0);
        }

        public virtual void Standup()
        {
            if (!IsDeployed || IsCanControl || !IsCanControl)
                return;

            if (AmmoCurrent == 0)
            {
                handsAnimator.SetFloat(EmptyParam, 1f);
            }
            else
            {
                handsAnimator.SetFloat(EmptyParam, 0f);
            }

            if (isCentralAim && isAiming && lastInputData.MouseSecondHold)
                handsAnimator.CrossFadeInFixedTime(StandUpCentralAnim, 0.1f, 0);
            else if (isAiming)
                handsAnimator.CrossFadeInFixedTime(StandUpAimAnim, 0.1f, 0);
            else
                handsAnimator.CrossFadeInFixedTime(StandUpIdleAnim, 0.1f, 0);
        }
        public override BaseItem CreateItem()
        {
            return new PistolItem("Item_Pistol");
        }

        void ResurrectEvent()
        {
            if(IsDeployed)
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
            if (!isDeployed || IsReloading)
                return;

            AnimatorStateInfo currentState = handsAnimator.GetCurrentAnimatorStateInfo(0);

            if(currentState.IsName("Idle"))
            {
                handsAnimator.Play(damageIdleAnim);
            } else if((currentState.IsName("Aim") || currentState.IsName("AimInAim")) && currentState.normalizedTime >= 0.6f)
            {
                handsAnimator.Play(damageAimAnim);
            }
        }

        public virtual void StartJump()
        {
            if (!IsDeployed)
                return;

            if (AmmoCurrent == 0)
            {
                handsAnimator.SetFloat(EmptyParam, 1f);
            }
            else
            {
                handsAnimator.SetFloat(EmptyParam, 0f);
            }

            if (!isReloading)
            {
                handsAnimator.SetBool("JumpFall", false);
                handsAnimator.SetTrigger("Jump");
            }
        }

        public virtual void EndJump()
        {
            if (!IsDeployed)
                return;
            if (isReloading)
                return;

            handsAnimator.SetBool("JumpFall", true);
            handsAnimator.ResetTrigger("Jump");
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

        public override void Update(float deltaTime)
        {
            if (controller.IsFreezed || !isCanControl)
                return;

            base.Update(deltaTime);

            bool nowIsCentralController = handsAnimator.runtimeAnimatorController == ControllerCentral;

            if (isCentralAim != nowIsCentralController)
            {
                handsAnimator.runtimeAnimatorController = isCentralAim ? ControllerCentral : ControllerDefault;
                if (isDeployed)
                {
                    handsAnimator.Play(deployAnim, 0, 1);
                }
            }

            if (isAiming)
            {
                if (lastInputData.MouseSecondHold)
                {
                    CameraNoise.NoiseAmount = Mathf.MoveTowards(CameraNoise.NoiseAmount, AimingNoise, deltaTime * 5f);
                    fromAimTime = Time.time + fromAimDelay;
                    if (needSmoothInAim)
                    {
                        cameraFov = Mathf.Lerp(cameraFov, Mathf.Lerp(defaultFov, currentAimFov, handsAnimator.GetFloat(fovParameter)), deltaTime * fovSmoothSpeed);
                    }
                    else
                    {
                        cameraFov = Mathf.Lerp(defaultFov, currentAimFov, handsAnimator.GetFloat(fovParameter));
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
                    cameraFov = Mathf.Lerp(cameraFov, defaultFov, Time.deltaTime * fovSmoothSpeed);
                }
            }
            else
            {

                cameraFov = Mathf.Lerp(cameraFov, defaultFov, Time.deltaTime * fovSmoothSpeed);
            }

            if (isAuto)
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

            recoilTime += Time.deltaTime;
            float recoilFraction = recoilTime / RecoilFadeOutTime;
            float recoilValue = RecoilCurve.Evaluate(recoilFraction);

            currentRecoil = Vector2.Lerp(Vector2.zero, currentRecoil, recoilValue);

            Vector2 recoilResult = currentRecoil;
            controller.mouseControl.LookRotation(recoilResult.x, recoilResult.y, deltaTime);

            if (IsDeployed)
            {
                currentLowPoseWeight = Mathf.MoveTowards(currentLowPoseWeight, (needLowPose && isLowPoseInAiming && isDeployed) ? LowPoseWeightTarget : 0, LowPoseWeightBlendSpeed * deltaTime);
                handsAnimator.SetLayerWeight(1, currentLowPoseWeight);
            }
        }

        public override void Deploy()
        {
            if (controller.IsFreezed)
                return;

            if (firstTimeDeploy)
            {
                if (isDeployed)
                    return;

                if (LockControlOnDeploy)
                    isCanControl = false;

                if (AmmoCurrent == 0)
                {
                    handsAnimator.SetFloat(EmptyParam, 1f);
                }
                else
                {
                    handsAnimator.SetFloat(EmptyParam, 0f);
                }

                handsAnimator.Play(firstTimeAnim);
                SetDeployed(true);

                propsRoot.SetActive(true);
            }
            else
            {
                base.Deploy();

                if (AmmoCurrent == 0)
                {
                    handsAnimator.SetFloat(EmptyParam, 1f);
                }
                else
                {
                    handsAnimator.SetFloat(EmptyParam, 0f);
                }

                propsRoot.SetActive(true);
            }
        }

        public override void HideProps()
        {
            propsRoot.SetActive(false);
        }

        public override void ShowProps()
        {
            propsRoot.SetActive(true);

            if (AmmoCurrent == 0)
            {
                handsAnimator.SetFloat(EmptyParam, 1f);
            }
            else
            {
                handsAnimator.SetFloat(EmptyParam, 0f);
            }
        }

        public override void SetDeployed(bool value)
        {
            base.SetDeployed(value);
            handsAnimator.Play(lowposeAnim, 1);
            if (LockControlOnDeploy)
                isCanControl = false;
        }

        public override void Hide()
        {
            if (!isDeployed || controller.IsFreezed)
                return;

            base.Hide();

            if (isOnFlashlight)
                ToggleFlashlight();

            firstTimeDeploy = false;

            toIdleElapsedTime = 0;
            if (AmmoCurrent == 0)
            {
                handsAnimator.SetFloat(EmptyParam, 1f);
            }
            else
            {
                handsAnimator.SetFloat(EmptyParam, 0f);
            }
            isAiming = false;
            isReloading = false;
        }

        public override void InputEvent(InputEventType et)
        {   
            if (!propsRoot.activeSelf || !isCanControl)
                return;

            base.InputEvent(et);

            if (et == InputEventType.Reload)
            {
                Reload();
            }

            if (et == InputEventType.MouseDown)
            {
                Fire();
            }

            if (et == InputEventType.MouseHold)
            {

            }

            if (et == InputEventType.MouseSecondDown)
            {
                Aim();
            }

            if (et == InputEventType.MouseSecondHold)
            {
                UpdateAim();
            }

            if (et == InputEventType.MouseSecondUp)
            {
                FromAim();
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

            if (et == InputEventType.Flashlight)
            {
                ToggleFlashlight();
            }
        }

        public virtual void ToggleFlashlight()
        {
            if (flashlightLight == null || flashLightVolumetric == null)
            {
                throw new System.Exception("No flashlight Set Upped");
            }

            isOnFlashlight = !isOnFlashlight;

            flashlightLight.enabled = isOnFlashlight;
            flashLightVolumetric.enabled = isOnFlashlight;
        }

        public virtual void PutFlashLight(GameObject root)
        {
            flashlightLight = root.GetComponent<Light>();
            flashLightVolumetric = root.transform.GetChild(0).GetComponent<MeshRenderer>();
        }

        public virtual void Reload()
        {
            if (isReloading || !isDeployed || controller.IsFreezed)
                return;

            if (AmmoCurrent == AmmoMax || AmmoCapacity == 0)
                return;

            if (AmmoCurrent == 0)
            {
                handsAnimator.SetFloat(EmptyParam, 1f);

                if (isLongReload)
                {
                    handsAnimator.CrossFadeInFixedTime(reloadLongAnim, 0.1f, 0);
                }
                else
                {
                    handsAnimator.CrossFadeInFixedTime(reloadAnim, 0.1f, 0);
                }

                DisableVisualBullets();
            }
            else
            {
                handsAnimator.SetFloat(EmptyParam, 0f);

                if (isLongReload)
                {
                    if (isBackReload)
                    {
                        handsAnimator.CrossFadeInFixedTime(reloadLongBackAnim, 0.1f, 0);
                    }
                    else
                    {
                        handsAnimator.CrossFadeInFixedTime(reloadLongAnim, 0.1f, 0);
                    }
                }
                else
                {
                    if (isBackReload)
                    {
                        handsAnimator.CrossFadeInFixedTime(reloadBackAnim, 0.1f, 0);
                    }
                    else
                    {
                        handsAnimator.CrossFadeInFixedTime(reloadAnim, 0.1f, 0);
                    }
                }
                EnableVisualBullets();
            }

            FromUpgrade(false);
            FromAim(false);
            isReloading = true;
        }

        public virtual void Fire()
        {
            if (isReloading || !isDeployed || controller.IsFreezed)
                return;

            if (AmmoCurrent == 0)
            {
                if (isAiming && lastInputData.MouseSecondHold)
                {
                    handsAnimator.Play(shotAnim, 0, 0);
                }
                else
                {
                    handsAnimator.CrossFadeInFixedTime(shotWithoutAimAnim, 0.05f, 0);
                }
            }
            else
            {
                if (isAiming && lastInputData.MouseSecondHold)
                {
                    if (AmmoCurrent == 1)
                    {
                        handsAnimator.Play(lastShotAnim, 0, 0);
                    }
                    else
                    {
                        handsAnimator.Play(shotAnim, 0, 0);
                    }
                }
                else
                {
                    if (AmmoCurrent == 1)
                    {
                        handsAnimator.CrossFadeInFixedTime(lastShotWithoutAimAnim, 0.05f, 0);
                    }
                    else
                    {
                        handsAnimator.CrossFadeInFixedTime(shotWithoutAimAnim, 0.05f, 0);
                    }
                }

                currentRecoil += GetRandomVector(RecoilAmountMin, RecoilAmountMax) * (isAiming ? AimRecoilMultiplier : 1f);
                recoilTime = 0;
                InstantiateGunShot();
                EmitShell();
                EmitHitEffect();
                AmmoCurrent--;
            }

            if (AmmoCurrent == 0)
            {
                handsAnimator.SetFloat(EmptyParam, 1f);
            }
            else
            {
                handsAnimator.SetFloat(EmptyParam, 0f);
            }

            if (isCentralAim && isAiming)
                isAiming = true;

            if (!isAiming)
            {
                isAiming = true;
            }
            fromAimTime = Time.time + fromAimDelay;

            nextFireTime = 0;
        }

        void EmitHitEffect()
        {
            Ray r = new Ray(FirePoint.position, FirePoint.forward);
            RaycastHit hitInfo;
            
            if (Physics.Raycast(r, out hitInfo, 1000f, FireCollisionLayer, QueryTriggerInteraction.Ignore))
            {
                IHittableObject hittable = hitInfo.collider.GetComponent<IHittableObject>();
                
                if (hittable == null)
                {
                    hitInfo.collider.GetComponentInParent<IHittableObject>();
                }

                DecalOnHit dOnHit = hitInfo.collider.GetComponent<DecalOnHit>();
                
                if (dOnHit == null && !controller.IsHumanoidTarget(hitInfo.collider.gameObject))
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
            decal.transform.SetParent(null);
            decal.SetActive(false);
            gunHitDecalPool.Enqueue(decal);
        }

        void InstantiateGunShot()
        {
            GameObject gunshotPrefab = isSilence ? gunshotSilencer : gunshot;
            
            GameObject instance = GameObject.Instantiate(gunshotPrefab, gunshotPrefab.transform.position, gunshotPrefab.transform.rotation, gunshotPrefab.transform.parent);
            instance.SetActive(true);

            GameObject.Destroy(instance, GunShotLiveTime);
        }

        void EmitShell()
        {
            Rigidbody shellInstance = GameObject.Instantiate(shell, shell.transform.position, shell.transform.rotation);

            shellInstance.gameObject.SetActive(true);
            shellInstance.transform.SetParent(null);
            shellInstance.velocity = controller.PlayerVelocity;
            shellInstance.AddForce(shellForceDir.forward * Random.Range(shellForceMin, shellForceMax));
            shellInstance.AddTorque(shellTorqueDir.forward * shellTorque);
            shellInstance.maxAngularVelocity = 65;
        }

        public virtual void Aim()
        {
            if (isReloading || !isDeployed || controller.IsFreezed)
                return;

            toIdleElapsedTime = 0;
            if (isAiming)
            {
                needSmoothInAim = true;
                if (AmmoCurrent == 0)
                {
                    handsAnimator.SetFloat(EmptyParam, 1f);
                }
                else
                {
                    handsAnimator.SetFloat(EmptyParam, 0f);
                }

                FromUpgrade(false);
                if (isCentralAim)
                {
                    handsAnimator.Play(basicToCentralAimAnim, 0, 0);
                }
                else
                {
                    handsAnimator.Play(aimInAimAnim, 0, 0);
                }
            }
            else
            {
                if (AmmoCurrent == 0)
                {
                    handsAnimator.SetFloat(EmptyParam, 1f);
                }
                else
                {
                    handsAnimator.SetFloat(EmptyParam, 0f);
                }

                if (handsAnimator.GetCurrentAnimatorStateInfo(0).IsName(shotWithoutAimAnim))
                    handsAnimator.CrossFadeInFixedTime(aimAnim, 0.25f, 0, 0.4f);
                else
                    handsAnimator.CrossFadeInFixedTime(aimAnim, 0.25f, 0);

                fromAimTime = -1;
                isAiming = true;
                FromUpgrade(false);
            }

            if (isLowPoseInAiming)
            {
                needLowPose = true;
            }

            HandsLerp.Multiplier = 1.5f;
        }

        public virtual void FromAim(bool withAnim = true)
        {
            if (isReloading)
                return;

            HandsLerp.Multiplier = 1;
            toIdleElapsedTime = 0;
            if (isAiming)
            {
                needSmoothInAim = false;
                if (isCentralAim)
                {
                    if (Time.time >= fromAimTime && fromAimTime >= 0)
                    {
                        if (withAnim)
                        {
                            if (AmmoCurrent == 0)
                            {
                                handsAnimator.SetFloat(EmptyParam, 1f);
                            }
                            else
                            {
                                handsAnimator.SetFloat(EmptyParam, 0f);
                            }
                            handsAnimator.CrossFadeInFixedTime(fromAimToIdleAnim, 0.25f, 0);
                        }
                        fromAimTime = -1;
                        isAiming = false;
                    }
                    else
                    {
                        fromAimTime = Time.time + fromAimDelay;

                        if(withAnim)
                            PlayFromAimAnim();
                    }
                }
                else
                {
                    if (Time.time >= fromAimTime && fromAimTime >= 0)
                    {
                        if (withAnim)
                        {
                            PlayFromAimAnim();
                        }
                        fromAimTime = -1;
                        isAiming = false;
                    }
                    else
                    {
                        fromAimTime = Time.time + fromAimDelay;

                        if (withAnim)
                            handsAnimator.Play(aimFromAimAnim, 0, 0);
                    }
                }
            }
            if (isLowPoseInAiming)
            {
                needLowPose = false;
            }
        }

        public virtual void PlayFromAimAnim()
        {
            if (AmmoCurrent == 0)
            {
                handsAnimator.SetFloat(EmptyParam, 1f);
            }
            else
            {
                handsAnimator.SetFloat(EmptyParam, 0f);
            }

            if (isCentralAim)
            {
                handsAnimator.CrossFadeInFixedTime(CentralToBasicAnim, 0.25f, 0);
            }
            else
            {
                handsAnimator.CrossFadeInFixedTime(fromAimAnim, 0.25f, 0);
            }
        }

        public virtual void UpdateAim()
        {

        }

        public virtual void ToUpgrade()
        {
            if (isReloading)
                return;

            sphere.enabled = true;
            customization.ShowModifications();
            isUpgrading = true;
            FromAim(false);
            handsAnimator.CrossFadeInFixedTime(toUpgradeAnim, 0.25f, 0);
            controller.Freeze(true);
        }

        public virtual void FromUpgrade(bool withAnim = true)
        {
            if (isReloading)
                return;

            sphere.enabled = false;
            customization.CloseModifications();
            isUpgrading = false;

            if (withAnim)
            {
                handsAnimator.CrossFadeInFixedTime(fromUpgradeAnim, 0.25f, 0);
                isAiming = false;
            }

            controller.Freeze(false);
        }

        public void Reloaded()
        {
            int needToReload = AmmoMax - AmmoCurrent;
            int willBeReload = ammoItem.Consume(needToReload);

            AmmoCurrent += willBeReload;

            isReloading = false;
        }

        public void ReloadEnd()
        {
            if (AmmoCurrent == 0)
            {
                handsAnimator.SetFloat(EmptyParam, 1f);
            }
            else
            {
                handsAnimator.SetFloat(EmptyParam, 0f);
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
            return ShootModeSprite;
        }

        public BaseItem GetAmmoItem()
        {
            return ammoItem;
        }
    }

    [SerializeField]
    public class PistolItem : BaseItem
    {
        public PistolItem(string itemName) : base(itemName)
        {
            MaxCapacity = 1;
        }
    }

    [SerializeField]
    public class PistolAmmoItem : BaseItem
    {
        public PistolAmmoItem(string itemName) : base(itemName)
        {
            MaxCapacity = 120;
            IconName = "PistolAmmo";
        }
    }
}
