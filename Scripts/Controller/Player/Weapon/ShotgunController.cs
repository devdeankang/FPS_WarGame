using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WarGame
{
    [System.Serializable]
    public class ShotgunController : WeaponController, IWeaponAmmo, IWeaponShootMode, IAmmoItemGetter
    {
        #region Variables
        
        [Header("Ammo")]
        public int AmmoCurrent = 12;
        public int AmmoMax = 12;
        public int AmmoMaxDefault = 12;
        public int AmmoMaxAdd = 24;
        public int AmmoCapacity { get { return ammoItem.GetCurrentCount(); } }
        public int BulletsCount = 30;
        public float BulletAngleMin = -2f;
        public float BulletAngleMax = 2f;
        public float DamageAmount = 40f;

        [Header("Options")]
        public bool isCentralAim = false;
        public bool isLowPoseInAiming = false;
        public bool isSilence = false;
        public bool isAuto = false;
        public bool isFlashlight = false;
        public bool isAddAmmo = false;
        public float LowPoseWeightTarget = 0.55f;
        public float LowPoseWeightBlendSpeed = 0.5f;
        public LayerMask FireCollisionLayer;
        public AnimationCurve RecoilCurve;        
        public float RecoilFadeOutTime = 0.15f;
        public float AimRecoilMultiplier = 0.3f;
        public float AimingNoise = 0.1f;        
        public float DetachPropsForce = 200f;
        public float BarrelSmokeDelay = 0.5f;

        public Rigidbody PropsBody;
        public Collider[] PropsCollision;
        public Transform DetachPropsDirection;
        public Animator BarrelSmokeAnimator;
        public Transform BarrelSmokeRotationReference;
        public GameObject BulletInHand;
        public GameObject BulletToGun;

        [Header("Animations")]
        [SerializeField] float fovSmoothSpeed = 20f;
        [SerializeField] float fromAimDelay = 319f;
        [SerializeField] float defaultFov = 60f;
        [SerializeField] float aimFov = 35f;
        [SerializeField] float aimFovCentral = 30f;
        
        [Header("Others")]
        public Sprite ShootModeSprite;
        public RuntimeAnimatorController ControllerDefault;
        public AnimatorOverrideController ControllerCentral;
        public GameObject GunHitDecalPrefab;
        public Transform FirePoint;
        public LerpToRoot HandsLerp;
        public TransformNoise CameraNoise;
        [SerializeField] GameObject gunshot;
        [SerializeField] GameObject gunshotSilencer;
        [SerializeField] float GunShotLiveTime = 0.05f;
        [SerializeField] PlayerController controller;
        [SerializeField] WeaponCustomization customization;
        [SerializeField] MeshRenderer sphere;
        [SerializeField] Rigidbody shell;
        [SerializeField] Transform shellForceDir;
        [SerializeField] float shellForceMin = 5f;
        [SerializeField] float shellForceMax = 8f;
        [SerializeField] Transform shellTorqueDir;
        [SerializeField] float shellTorque = 800f;
        [SerializeField] GameObject propsRoot;
        [SerializeField] float autoFireRate = 0.75f;
        int poolSize = 200;
        float hitDecalLifetime = 20f;
        float nextFireTime;
        float toIdleElapsedTime = 0;
        float fromAimTime = 0;
        float currentLowPoseWeight = 0;
        float smokeElapsedTime = 0f;
        float cameraFov = 60f;
        float recoilTime = 0;
        Vector2 currentRecoil = new Vector2();
        TransformStateSetupper propsTransformState;
        

        Vector2 recoilAmountMin = new Vector2(0.3f, 2f);
        Vector2 recoilAmountMax = new Vector2(-0.3f, 3f);

        bool isAiming = false;
        bool isCentralAimFromDefaultAim = false;
        bool isReloading = false;
        bool isUpgrading = false;
        bool needSmoothInAim = false;
        bool needLowPose = false;
        bool isOnFlashlight = false;
        bool isFirstTimeDeploy;

        Queue<GameObject> gunHitDecalPool = new Queue<GameObject>();    //@@ need to PoolManager
        Rigidbody clonedPropsBody;
        ShotgunAmmoItem ammoItem;
        Light flashlightLight;
        MeshRenderer flashLightVolumetric;

        #endregion

        #region StringParams

        string EndReloadParam = "SReloadEnd";
        string FromAimDefaultAnim = "SGFromAim";
        string UpgradeIdleAnim = "SGUpgradeLoop";
        string fovParameter = "FOV";
        string firstTimeAnim = "SGFirstDeploy";
        string lowposeAnim = "SGLowPose";
        string reloadFromIdleAnim = "SGReloadFromIdleStart";
        string reloadFromAimAnim = "SGReloadFromAimStart";
        string aimAnim = "SGAim";
        string fromAimAnim = "SGFromAim";
        string aimInAimAnim = "SGAimInAim";
        string centralAim = "SGCentralAim";
        string centralAimFromIdle = "SGCentralAimFromIdle";
        string centralFromAim = "SGCentralFromAim";
        string centralFromAimToIdle = "SGCentralFromAimToIdle";
        string toUpgradeAnim = "SGUpgradeStart";
        string fromUpgradeAnim = "SGUpgradeEnd";

        string shotAnim = "SGShot";
        string shotCentralAnim = "SGCentralShoot";
        string emptyShotAnim = "SGShotEmpty";
        string emptyCentralShotAnim = "SGCentralShootEmpty";
        string shotWithoutAimAnim = "SGShotWithoutAim";
        string emptyShotWithoutAimAnim = "SGShotEmptyWithoutAim";
        string jumpStartAimAnim = "SGJumpStartAim";
        string jumpEndAimAnim = "SGJumpEndAim";
        string jumpStartIdleAnim = "SGJumpStartIdle";
        string jumpEndIdleAnim = "SGJumpEndIdle";
        string jumpStartCentralAimAnim = "SGJumpStartAimCentral";
        string jumpEndCentralAimAnim = "SGJumpEndAimCentral";

        // Crouch
        string crouchAimAnim = "ShotgunBasicAimSitDown";
        string standUpAimAnim = "ShotgunBasicAimGetUp";
        string crouchIdleAnim = "ShotgunIdleSitDown";
        string standUpIdleAnim = "ShotgunIdleGetUp";
        string crouchCentralAimAnim = "ShotgunCentralAimSitDown";
        string standUpCentralAimAnim = "ShotgunCentralAimGetUp";

        // Damage
        string damagedBasicAimAnim = "SGDamageBasicAim";
        string damagedIdleAnim = "SGDamageIdle";
        string damagedCentralAimAnim = "SGDamageCentral";

        // Sounds
        string DefaultShoot = "shotgun basic shot";
        string SilenceShoot = "shotgun basic silencer shot";
        string DefaultCentralAimShoot = "shotgun aim shot";
        string SilenceCentralAimShoot = "shotgun aim silencer shot";

        string shotgunHitEffectPoolParam = "@Shotgun HitEffect Pool";
        #endregion

        public float CurrentAimFov { get { return isCentralAim ? aimFovCentral : aimFov; } }

        public float CameraFov { get { return cameraFov; } }
        
        public bool IsAiming { get { return isAiming; } }

        public bool IsReloading { get { return isReloading; } }
        
        public string ShotSound
        {
            get
            {
                if (isCentralAim && isAiming && lastInputData.MouseSecondHold)
                {
                    if (isSilence)
                    {
                        return SilenceCentralAimShoot;
                    }
                    else
                    {
                        return DefaultCentralAimShoot;
                    }
                }
                else
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
        }


        public override void Init(GameObject root)
        {
            base.Init(root);
            isFirstTimeDeploy = true;
            InitShotgunParams();

            propsTransformState = PropsBody.GetComponent<TransformStateSetupper>();

            InitPool();
            controller.JumpStartEvent += StartJump;
            controller.JumpEndEvent += EndJump;

            controller.CrouchEvent += Crouch;
            controller.StandUpEvent += Standup;

            controller.DamageHandler.DamagedEvent.AddListener(DamagedEvent);
            controller.DamageHandler.ResurrectEvent.AddListener(ResurrectEvent);


            if (!propsTransformState.CalculateVelocity)
            {
                Debug.LogWarning("Weapon velocity calculation disabled", PropsBody);
                propsTransformState.CalculateVelocity = true;
            }

            ammoItem = new ShotgunAmmoItem("Item_Shotgun_Ammo");
            playerInventory.AddItem(ammoItem);
            controller.PlayerFreezeChanged.AddListener(FreezeChanged);

            hitEffectPool.name = shotgunHitEffectPoolParam;
        }

        

        void InitShotgunParams()
        {
            deployedParameter = "Shotgun";
            deployAnim = "SGDeploy";
            hideAnim = "SGHide";
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
            if (!IsDeployed || isReloading || !isCanControl)
                return;

            AnimatorStateInfo state = handsAnimator.GetCurrentAnimatorStateInfo(0);
            if(state.IsName("SGCentralAimIdle"))
            {
                handsAnimator.Play(damagedCentralAimAnim);
            } else if(state.IsName("SGAimInAim"))
            {
                handsAnimator.Play(damagedBasicAimAnim);
            }
            else if(state.IsName("SGIdle"))
            {
                handsAnimator.Play(damagedIdleAnim);
            }
        }

        public virtual void Crouch()
        {
            if (!IsDeployed || isReloading || !isCanControl)
                return;

            if (isAiming)
            {
                if (isCentralAim)
                {
                    handsAnimator.CrossFadeInFixedTime(crouchCentralAimAnim, 0.05f, 0, 0);
                }
                else
                {
                    handsAnimator.CrossFadeInFixedTime(crouchAimAnim, 0.05f, 0, 0);
                }
            }
            else
            {
                if(fromAimTime > 0)
                {
                    handsAnimator.CrossFadeInFixedTime(crouchAimAnim, 0.05f, 0, 0);
                }
                else
                {
                    handsAnimator.CrossFadeInFixedTime(crouchIdleAnim, 0.05f, 0, 0);
                }
            }
        }

        public virtual void Standup()
        {
            if (!IsDeployed || isReloading || !isCanControl)
                return;

            if (isAiming)
            {
                if (isCentralAim)
                {
                    handsAnimator.CrossFadeInFixedTime(standUpCentralAimAnim, 0.05f, 0, 0);
                }
                else
                {
                    handsAnimator.CrossFadeInFixedTime(standUpAimAnim, 0.05f, 0, 0);
                }
            }
            else
            {
                if (fromAimTime > 0)
                {
                    handsAnimator.CrossFadeInFixedTime(standUpAimAnim, 0.05f, 0, 0);
                }
                else
                {
                    handsAnimator.CrossFadeInFixedTime(standUpIdleAnim, 0.05f, 0, 0);
                }
            }

        }

        public virtual void StartJump()
        {
            if (!IsDeployed || !isCanControl)
                return;

            if (!isReloading)
            {
                handsAnimator.SetBool("JumpFall", false);
                handsAnimator.SetTrigger("Jump");
            }
        }

        public virtual void EndJump()
        {
            if (!IsDeployed || isReloading|| !isCanControl)
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

            Vector3 upDirection = Vector3.up;
            Vector3 forwardDirection = BarrelSmokeRotationReference.forward;
            BarrelSmokeAnimator.transform.rotation = Quaternion.LookRotation(forwardDirection, upDirection);
            if (isReloading)
            {
                fromAimTime = Time.time + fromAimDelay;
            }

            base.Update(deltaTime);

            if (isAiming)
            {
                if (lastInputData.MouseSecondHold)
                {
                    CameraNoise.NoiseAmount = Mathf.MoveTowards(CameraNoise.NoiseAmount, AimingNoise, deltaTime * 5f);
                    fromAimTime = Time.time + fromAimDelay;
                    if (needSmoothInAim)
                    {
                        cameraFov = Mathf.Lerp(cameraFov, Mathf.Lerp(defaultFov, CurrentAimFov, handsAnimator.GetFloat(fovParameter)), deltaTime * fovSmoothSpeed);
                    }
                    else
                    {
                        cameraFov = Mathf.Lerp(defaultFov, CurrentAimFov, handsAnimator.GetFloat(fovParameter));
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

            nextFireTime += Time.deltaTime;
            if (lastInputData.MouseHold && AmmoCurrent > 0)
            {
                if (nextFireTime >= autoFireRate)
                {
                    Fire();
                }
            }

            if (smokeElapsedTime >= 0)
            {
                smokeElapsedTime += Time.deltaTime;
                if (smokeElapsedTime >= BarrelSmokeDelay)
                {
                    BarrelSmokeAnimator.Play("BarrelSmokeAnimation", 0, 0);
                    smokeElapsedTime = -1;
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

            if (isFirstTimeDeploy)
            {
                if (isDeployed)
                    return;

                if (LockControlOnDeploy)
                    isCanControl = false;

                handsAnimator.Play(firstTimeAnim);
                SetDeployed(true);

                propsRoot.SetActive(true);
            }
            else
            {
                base.Deploy();

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
        }

        public override void SetDeployed(bool value)
        {
            base.SetDeployed(value);
            handsAnimator.Play(lowposeAnim, 1);
        }

        public override void Hide()
        {
            if (!isDeployed || controller.IsFreezed)
                return;

            base.Hide();

            if (isOnFlashlight)
                ToggleFlashlight();

            isFirstTimeDeploy = false;

            toIdleElapsedTime = 0;
            isAiming = false;
            isReloading = false;
            HideBullets();
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
                UpgradeAim();
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
                throw new System.Exception("No flashlight set upped");
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

            if (isAiming)
            {
                handsAnimator.CrossFadeInFixedTime(reloadFromAimAnim, 0.1f, 0);
            }
            else
            {
                handsAnimator.CrossFadeInFixedTime(reloadFromIdleAnim, 0.1f, 0);
            }

            FromUpgrade(false);
            FromAim(false);
            isReloading = true;
        }

        public virtual void Fire()
        {
            if (isReloading)
            {
                EndReloading();
                return;
            }

            if (!isDeployed || controller.IsFreezed || nextFireTime < autoFireRate)
                return;

            if (AmmoCurrent == 0)
            {
                if (isAiming && (isCentralAimFromDefaultAim || currentInputData.MouseSecondHold))
                {
                    if (isCentralAim)
                    {
                        handsAnimator.Play(emptyCentralShotAnim, 0, 0);
                    }
                    else
                    {
                        handsAnimator.Play(emptyShotAnim, 0, 0);
                    }
                }
                else
                {
                    handsAnimator.CrossFadeInFixedTime(emptyShotWithoutAimAnim, 0.05f, 0);
                }
            }
            else
            {
                if (isAiming && (isCentralAimFromDefaultAim || currentInputData.MouseSecondHold))
                {
                    if (isCentralAim)
                    {
                        handsAnimator.Play(shotCentralAnim, 0, 0);
                    }
                    else
                    {
                        handsAnimator.Play(shotAnim, 0, 0);
                    }
                }
                else
                {
                    handsAnimator.CrossFadeInFixedTime(shotWithoutAimAnim, 0.05f, 0);
                }

                currentRecoil += GetRandomVector(recoilAmountMin, recoilAmountMax) * (currentInputData.MouseHold ? AimRecoilMultiplier : 1f);
                recoilTime = 0;
                InstantiateGunShot();
                //EmitShell();
                EmitHitEffect();
                AmmoCurrent--;
                smokeElapsedTime = 0;

                BarrelSmokeAnimator.Play("BarrelSmokeAnimation", 0, 1);
            }

            if (!isAiming && isCentralAim)
            {
                isAiming = true;
            }
            fromAimTime = Time.time + fromAimDelay;

            nextFireTime = 0;
        }

        void EmitHitEffect()
        {
            for (int i = 0; i < BulletsCount; i++)
            {
                Quaternion randomRotation = Quaternion.AngleAxis(Random.Range(BulletAngleMin, BulletAngleMax), FirePoint.up) * Quaternion.AngleAxis(Random.Range(BulletAngleMin, BulletAngleMax), FirePoint.right);

                Ray r = new Ray(FirePoint.position, randomRotation * FirePoint.forward);
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

        public void EmitShell()
        {
            Rigidbody shellInstance = GameObject.Instantiate(shell, shell.transform.position, shell.transform.rotation);

            shellInstance.gameObject.SetActive(true);
            shellInstance.transform.SetParent(null);
            shellInstance.maxAngularVelocity = 20;

            shellInstance.velocity = controller.PlayerVelocity;
            shellInstance.AddForce(shellForceDir.forward * Random.Range(shellForceMin, shellForceMax));
            shellInstance.AddTorque(shellTorqueDir.forward * shellTorque);
            shellInstance.maxAngularVelocity = 65;
        }
        
        protected virtual void Aim()
        {
            if (isReloading)
            {
                EndReloading();
                return;
            }

            if (!isDeployed || controller.IsFreezed)
                return;

            toIdleElapsedTime = 0;
            if (isAiming)
            {
                needSmoothInAim = true;

                FromUpgrade(false);
                if (currentInputData.MouseSecondHold)
                {
                    if (isCentralAim)
                    {
                        handsAnimator.CrossFadeInFixedTime(centralAim, 0.05f, 0);
                    }
                    else
                    {
                        handsAnimator.Play(aimInAimAnim, 0, 0);
                    }
                }
                else
                {
                    handsAnimator.Play(aimInAimAnim, 0, 0);
                }
                isCentralAimFromDefaultAim = true;
            }
            else
            {
                if (handsAnimator.GetCurrentAnimatorStateInfo(0).IsName(shotWithoutAimAnim))
                {
                    if (!isCentralAim)
                    {
                        handsAnimator.CrossFadeInFixedTime(aimAnim, 0.25f, 0, 0.4f);
                    }
                }
                else
                {
                    if (isCentralAim)
                    {
                        handsAnimator.CrossFadeInFixedTime(centralAimFromIdle, 0.25f, 0);
                    }
                    else
                    {
                        handsAnimator.CrossFadeInFixedTime(aimAnim, 0.05f, 0);
                    }
                }

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
                    if (withAnim)
                    {
                        if (isCentralAimFromDefaultAim)
                        {
                            handsAnimator.CrossFadeInFixedTime(centralFromAim, 0.05f, 0);
                        }
                        else
                        {
                            handsAnimator.CrossFadeInFixedTime(centralFromAim, 0.05f, 0);
                        }
                    }

                    if (isCentralAimFromDefaultAim)
                    {
                        fromAimTime = Time.time + fromAimDelay;
                    }
                    else
                    {
                        isAiming = false;
                    }
                }
                else
                {
                    if (Time.time >= fromAimTime && fromAimTime >= 0)
                    {
                        if (withAnim)
                        {
                            handsAnimator.CrossFadeInFixedTime(fromAimAnim, 0.05f, 0);
                        }
                        fromAimTime = -1;
                        isAiming = false;
                    }
                    else
                    {
                        fromAimTime = Time.time + fromAimDelay;
                        if(withAnim)
                            handsAnimator.Play(aimInAimAnim, 0, 0);
                    }
                }
            }
            isCentralAimFromDefaultAim = false;
            if (isLowPoseInAiming)
            {
                needLowPose = false;
            }
        }

        public virtual void UpgradeAim()
        {

        }

        public virtual void ToUpgrade()
        {
            if (isReloading)
            {
                EndReloading();
                return;
            }

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
                handsAnimator.CrossFadeInFixedTime(fromUpgradeAnim, 0.25f, 0);

            controller.Freeze(false);
        }

        public void HideBullets()
        {
            BulletToGun.SetActive(false);
            BulletInHand.SetActive(false);
        }

        public void ReloadBulletInserted()
        {
            BulletToGun.SetActive(false);
        }

        public void ReloadLoopStart()
        {
            BulletToGun.SetActive(true);
            if (IsNeedOneBullet())
            {
                BulletInHand.SetActive(false);
            }
            else
            {
                BulletInHand.SetActive(true);
            }
        }

        bool IsNeedOneBullet()
        {
            return AmmoCapacity == 1 || (AmmoMax - AmmoCurrent == 1);
        }

        public void AddOneBullet()
        {
            AmmoCurrent++;
            ammoItem.Consume(1);

            if (AmmoCurrent >= AmmoMax || AmmoCapacity <= 0)
            {
                EndReloading();
            }
        }

        void EndReloading()
        {
            handsAnimator.SetTrigger(EndReloadParam);
        }

        public void ReloadEnd()
        {
            isReloading = false;
        }

        public Sprite GetCurrentModeSprite()
        {
            return ShootModeSprite;
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

        public override BaseItem CreateItem()
        {
            return new ShotgunItem("Item_Shotgun");
        }

        public BaseItem GetAmmoItem()
        {
            return ammoItem;
        }
    }

    public class ShotgunItem : BaseItem
    {
        public ShotgunItem(string itemName) : base(itemName)
        {
            MaxCapacity = 1;
        }
    }

    public class ShotgunAmmoItem : BaseItem
    {
        public ShotgunAmmoItem(string itemName) : base(itemName)
        {
            MaxCapacity = 120;
            IconName = "ShotgunAmmo";
        }
    }
}