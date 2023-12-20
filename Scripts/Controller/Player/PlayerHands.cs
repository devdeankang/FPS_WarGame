using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace WarGame
{
    public class PlayerHands : MonoBehaviour
    {
        [Header("Weapon Controller Setting")]
        public PistolController PistolHands;
        public KnifeController KnifeHands;
        public ShotgunController ShotgunHands;
        public RifleController RifleHands;
        public GrenadeController GrenadeHands;
        
        [Header("Pistol Mods")]
        public int[] ModificatorsIdsForCentrailAim;
        public int[] ModificatorsIdsForBackReload;
        public int[] ModificatorsIdsForFlashlight;
        public int[] ModificatorsIdsForLowPose;
        public int[] ModificatorsIdsForLongReload;
        public int[] ModificatorsIdsForSilencer;

        [Header("SG Mods")]
        public int[] SGModificatorsIdsForCentrailAim;
        public int[] SGModificatorsIdsForFlashlight;
        public int[] SGModificatorsIdsForLowPose;
        public int[] SGModificatorsIdsForSilencer;
        public int[] SGModificatorsIdsForAddAmmo;

        [Header("Rifle Mods")]
        public int[] RifleModificatorsIdsForSilencer;
        public int[] RifleModificatorsIdsForBasicAim;
        public int[] RifleModificatorsIdsForCollimatorAim;
        public int[] RifleModificatorsIdsForZoomedAim;
        public int[] RifleModificatorsIdsForZoomed12XAim;

        [Header("Other")]        
        [SerializeField] float defaultFOV = 60f;
        [SerializeField] float runAddedFOV = 5f;        
        [SerializeField] float dofSmoothSpeed = 17.5f;
        int knifeHitCount = 6;
        float runFraction;
        
        [Header("AudioSources")]
        public AudioSource[] Sources;

        [HideInInspector] public HandsChangedEvent HandsChanged = new HandsChangedEvent();
        Dictionary<string, AudioSource> sounds = new Dictionary<string, AudioSource>();
        List<WeaponController> registeredItems = new List<WeaponController>();
        WeaponController prevItem = null;
        WeaponController nextItem = null;
        WeaponController deployedBeforeInjection;
        bool isWaitForDeploy = false;

        Animator animator;
        InjectorController injector;
        WeaponCustomization customization;

        Camera targetCamera;
        PlayerController controller;
        WeaponController.HandsControllerInput handsInput;
        PlayerInventory inventory;

        #region params
        
        string PlayerParam = "Player";
        string RunParam = "Run";
        string KnifeHitParam = "KnifeHit";
        string KnifeParam = "Knife";
        string PistolParam = "Pistol";
        string ShotgunParam = "Shotgun";
        string RifleParam = "Rifle";
        string GrenadeParam = "Grenade";
        string JumpAnimParam = "PlayerJump";
        string LandAnimParam = "PlayerLanding";
        string CrouchAnimParam = "PlayerCrouch";
        string StandupAnimParam = "PlayerStandUp";

        #endregion

        public bool IsShowAim { get { return !IsAiming; } }

        public bool IsAiming
        {
            get
            {
                return (PistolHands.IsAiming && PistolHands.isCentralAim) || (ShotgunHands.IsAiming && ShotgunHands.isCentralAim) || (RifleHands.IsAiming && RifleHands.isCentralAim);
            }
        }


        void Awake()
        {
            GetComponents();
            InitAudioSources();
            InitEvents();
            InitWeaponRegistration();
        }

        void Update()
        {
            handsInput = new WeaponController.HandsControllerInput();

            ControlMouseCursor();
            InputHandsControllers();
            DeployHandsWeapon();
        }


        void GetComponents()
        {
            animator = GetComponent<Animator>();
            inventory = GetComponent<PlayerInventory>();
            customization = GetComponent<WeaponCustomization>();
            injector = GetComponent<InjectorController>();
            controller = GameObject.FindGameObjectWithTag(PlayerParam).GetComponent<PlayerController>();
            targetCamera = Camera.main;
        }

        void InitWeaponRegistration()
        {
            PistolHands.Init(gameObject);
            PistolHands.SetCameraFov(60);
            KnifeHands.Init(gameObject);
            ShotgunHands.Init(gameObject);
            ShotgunHands.SetCameraFov(60);
            RifleHands.Init(gameObject);
            GrenadeHands.Init(gameObject);

            registeredItems.Add(PistolHands);
            registeredItems.Add(KnifeHands);
            registeredItems.Add(ShotgunHands);
            registeredItems.Add(RifleHands);
            registeredItems.Add(GrenadeHands);
            injector.InjectionStartEvent.AddListener(StartInjection);
            injector.InjectionEndEvent.AddListener(EndInjection);
            
            foreach (WeaponController item in registeredItems)
            {
                WeaponController weapon = item;
                item.PickedUp.AddListener(() => PickedUpItems(weapon));
            }
        }

        void InitAudioSources()
        {
            foreach (AudioSource s in Sources)
            {
                sounds.Add(s.name, s);
            }
        }

        void InitEvents()
        {
            customization.ModificationEnd += EndModification;
            controller.RunStartEvent += StartRun;
            controller.JumpStartEvent += StartJump;
            controller.JumpEndEvent += EndJump;
            controller.CrouchEvent += Crouch;
            controller.StandUpEvent += Standup;
        }

        public void PlayShotgunSound()
        {
            PlaySFX(ShotgunHands.ShotSound);
        }

        public void EnablePistolVisualBullets()
        {
            PistolHands.EnableVisualBullets();
        }

        public void DisablePistolVisualFirstTimeBullet()
        {
            PistolHands.DisableVisualFirstTimeBullet();
        }

        void PickedUpItems(WeaponController weapon)
        {
            bool anyDeployed = false;
            foreach (WeaponController item in registeredItems)
            {
                if (item.IsDeployed)
                {
                    anyDeployed = true;
                    break;
                }
            }

            if(!anyDeployed)
            {
                nextItem = weapon;
                weapon.Deploy();
                HandsChanged.Invoke(weapon);
            }
        }

        public void PlayRifleShot()
        {
            PlaySFX(RifleHands.ShootSound);
        }

        public void PlayRifleLastShot()
        {
            PlaySFX(RifleHands.LastShootSound);
        }

        void StartInjection()
        {
            if (deployedBeforeInjection != null)
                deployedBeforeInjection.HideProps();
        }

        void EndInjection()
        {
            if (deployedBeforeInjection != null)
            {
                nextItem = deployedBeforeInjection;
                deployedBeforeInjection.Deploy();
            }

            deployedBeforeInjection = null;
        }

        public void DetachPropsOnDead()
        {
            if (PistolHands.IsDeployed)
                PistolHands.DetachProps();

            if (RifleHands.IsDeployed)
                RifleHands.DetachProps();

            if (ShotgunHands.IsDeployed)
                ShotgunHands.DetachProps();

            if (KnifeHands.IsDeployed)
                KnifeHands.DetachProps();

            if (GrenadeHands.IsDeployed)
                GrenadeHands.DetachProps();
        }

        public void FullDeployEvent()
        {
            foreach(WeaponController h in registeredItems)
            {
                if(h.IsDeployed)
                {
                    h.SetFullyDeployed();
                }
            }
        }

        public bool HideWeapons()
        {
            if (PistolHands.IsDeployed)
            {
                PistolHands.Hide();
                return true;
            }

            if (KnifeHands.IsDeployed)
            {
                KnifeHands.Hide();
                return true;
            }

            if (ShotgunHands.IsDeployed)
            {
                ShotgunHands.Hide();
                return true;
            }

            if (RifleHands.IsDeployed)
            {
                RifleHands.Hide();
                return true;
            }

            if (GrenadeHands.IsDeployed)
            {
                GrenadeHands.Hide();
                return true;
            }

            return false;
        }

        public WeaponController GetHandsItem(string itemName)
        {
            if(itemName.Equals(KnifeParam))
            {
                return KnifeHands;
            } else if(itemName.Equals(PistolParam))
            {
                return PistolHands;
            } else if(itemName.Equals(ShotgunParam))
            {
                return ShotgunHands;
            }
            else if (itemName.Equals(RifleParam))
            {
                return RifleHands;
            }
            else if (itemName.Equals(GrenadeParam))
            {
                return GrenadeHands;
            }

            return null;
        }

        public virtual void StartJump()
        {
            if(controller.DamageHandler.Health.RealIsAlive)
                animator.Play(JumpAnimParam, 3, 0);
        }

        public virtual void EndJump()
        {
            if(controller.DamageHandler.Health.RealIsAlive)
                animator.Play(LandAnimParam, 3, 0);
        }

        public virtual void Crouch()
        {
            if (controller.DamageHandler.Health.RealIsAlive)
            {
                animator.Play(CrouchAnimParam, 3, 0);                
            }
        }

        public virtual void Standup()
        {
            if (controller.DamageHandler.Health.RealIsAlive)
            {
                animator.Play(StandupAnimParam, 3, 0);
            }
        }

        public void PlaySFX(string sourceName)
        {
            AudioSource source;
            if (sounds.TryGetValue(sourceName, out source))
            {
                playSoundInstance(source);
            }
        }

        public void PlayPistolShoot()
        {
            PlaySFX(PistolHands.ShootSound);
        }

        public void PlayKnifeHit()
        {
            int randomHit = Random.Range(0, knifeHitCount);
            PlaySFX(KnifeHitParam + randomHit);
        }

        void playSoundInstance(AudioSource source)
        {
            AudioSource instance = Instantiate(source, source.transform.parent);
            instance.Play();
            Destroy(instance.gameObject, 10);
        }

        void StartRun()
        {

        }

        void EndModification()
        {
            ApplyPistolMods();
            ApplyShotgunMods();
            ApplyRifleMods();
        }

        void ApplyRifleMods()
        {
            RifleHands.isSilence = false;
            RifleHands.isCentralAim = false;
            RifleHands.isLowPoseInAiming = false;
            RifleHands.isTopPoseInAiming = false;
            RifleHands.SetAiming(true, false, false, false);
            foreach (Modificator m in customization.AttachedModificators)
            {
                foreach (int mId in RifleModificatorsIdsForBasicAim)
                {
                    if (m.ModId == mId)
                    {
                        RifleHands.SetAiming(false, false, false, false);
                        break;
                    }
                }
                foreach (int mId in RifleModificatorsIdsForCollimatorAim)
                {
                    if (m.ModId == mId)
                    {
                        RifleHands.SetAiming(false, false, false, true);
                        break;
                    }
                }
                foreach (int mId in RifleModificatorsIdsForZoomedAim)
                {
                    if (m.ModId == mId)
                    {
                        RifleHands.SetAiming(false, true, false, false);
                        break;
                    }
                }
                foreach (int mId in RifleModificatorsIdsForZoomed12XAim)
                {
                    if (m.ModId == mId)
                    {
                        RifleHands.SetAiming(false, false, true, false);
                        break;
                    }
                }
                foreach (int mId in RifleModificatorsIdsForSilencer)
                {
                    if (m.ModId == mId)
                    {
                        RifleHands.isSilence = true;
                        break;
                    }
                }

            }
        }

        void ApplyShotgunMods()
        {
            ShotgunHands.isCentralAim = false;
            ShotgunHands.isLowPoseInAiming = false;
            ShotgunHands.isSilence = false;
            ShotgunHands.isFlashlight = false;
            ShotgunHands.isAddAmmo = false;
            foreach (Modificator m in customization.AttachedModificators)
            {
                foreach (int mId in SGModificatorsIdsForCentrailAim)
                {
                    if (m.ModId == mId)
                    {
                        ShotgunHands.isCentralAim = true;
                        break;
                    }
                }
                foreach (int mId in SGModificatorsIdsForLowPose)
                {
                    if (m.ModId == mId)
                    {
                        ShotgunHands.isLowPoseInAiming = true;
                        break;
                    }
                }
                foreach (int mId in SGModificatorsIdsForSilencer)
                {
                    if (m.ModId == mId)
                    {
                        ShotgunHands.isSilence = true;
                        break;
                    }
                }
                foreach (int mId in SGModificatorsIdsForFlashlight)
                {
                    if (m.ModId == mId)
                    {
                        ShotgunHands.isFlashlight = true;
                        ShotgunHands.PutFlashLight((m.CustomData as GameObject));
                        break;
                    }
                }
                foreach (int mId in SGModificatorsIdsForAddAmmo)
                {
                    if (m.ModId == mId)
                    {
                        ShotgunHands.isAddAmmo = true;
                        break;
                    }
                }
            }

            ShotgunHands.AmmoMax = ShotgunHands.isAddAmmo ? ShotgunHands.AmmoMaxAdd : ShotgunHands.AmmoMaxDefault;
            int ammoToCapacity = ShotgunHands.AmmoCurrent - ShotgunHands.AmmoMax;
            ammoToCapacity = Mathf.Clamp(ammoToCapacity, 0, ShotgunHands.AmmoMax);


            int ammoDelta = ShotgunHands.AmmoCurrent - ShotgunHands.AmmoMax;

            if (ammoDelta > 0)
            {
                ShotgunHands.AmmoCurrent -= ammoDelta;
                ShotgunHands.GetAmmoItem().Add(ammoDelta);
            }
        }
        
        void ApplyPistolMods()
        {
            PistolHands.isCentralAim = false;
            PistolHands.isLowPoseInAiming = false;
            PistolHands.isLongReload = false;
            PistolHands.isSilence = false;
            PistolHands.isFlashlight = false;
            PistolHands.isBackReload = false;
            foreach (Modificator m in customization.AttachedModificators)
            {
                foreach (int mId in ModificatorsIdsForCentrailAim)
                {
                    if (m.ModId == mId)
                    {
                        PistolHands.isCentralAim = true;
                        break;
                    }
                }
                foreach (int mId in ModificatorsIdsForLowPose)
                {
                    if (m.ModId == mId)
                    {
                        PistolHands.isLowPoseInAiming = true;
                        break;
                    }
                }
                foreach (int mId in ModificatorsIdsForLongReload)
                {
                    if (m.ModId == mId)
                    {
                        PistolHands.isLongReload = true;
                        break;
                    }
                }
                foreach (int mId in ModificatorsIdsForSilencer)
                {
                    if (m.ModId == mId)
                    {
                        PistolHands.isSilence = true;
                        break;
                    }
                }
                foreach (int mId in ModificatorsIdsForBackReload)
                {
                    if (m.ModId == mId)
                    {
                        PistolHands.isBackReload = true;
                        break;
                    }
                }
                foreach (int mId in ModificatorsIdsForFlashlight)
                {
                    if (m.ModId == mId)
                    {
                        PistolHands.isFlashlight = true;
                        PistolHands.PutFlashLight((m.CustomData as GameObject));
                        break;
                    }
                }
            }

            PistolHands.AmmoMax = PistolHands.isLongReload ? PistolHands.AmmoMaxLong : PistolHands.AmmoMaxDefault;

            int ammoDelta = PistolHands.AmmoCurrent - PistolHands.AmmoMax;

            if(ammoDelta > 0)
            {
                PistolHands.AmmoCurrent -= ammoDelta;
                PistolHands.GetAmmoItem().Add(ammoDelta);
            }

            if (PistolHands.IsDeployed)
            {
                bool nowIsCentralController = animator.runtimeAnimatorController == PistolHands.ControllerCentral;

                if (PistolHands.isCentralAim != nowIsCentralController)
                {
                    if (PistolHands.AmmoCurrent == 0)
                    {
                        animator.SetFloat(PistolHands.EmptyParam, 1f);
                    }
                    else
                    {
                        animator.SetFloat(PistolHands.EmptyParam, 0f);
                    }

                    if (PistolHands.IsDeployed)
                    {
                        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                        animator.runtimeAnimatorController = PistolHands.isCentralAim ? PistolHands.ControllerCentral : PistolHands.ControllerDefault;

                        animator.Play(PistolHands.UpgradeIdleAnim, 0, state.normalizedTime);
                    }
                }
            }
        }

        public void SetRun(float run)
        {
            animator.SetFloat(RunParam, run);
            runFraction = run;
        }

        public void ReloadedEvent()
        {
            PistolHands.Reloaded();
        }

        public void ReloadEndEvent()
        {
            PistolHands.ReloadEnd();
        }

        public void EmitShotgunShell()
        {
            ShotgunHands.EmitShell();
        }

        public void RifleFirstDeployEnd()
        {
            RifleHands.FirstDeployEnd();
        }

        public void RifleReloadedEvent()
        {
            RifleHands.Reloaded();
        }

        public void RifleReloadedEndEvent()
        {
            RifleHands.ReloadEnd();
        }

        public void SpawnGrenade()
        {
            GrenadeHands.SpawnGrenade();
        }

        public void ShotgunAddBullet()
        {
            ShotgunHands.AddOneBullet();
            ShotgunHands.ReloadBulletInserted();
        }

        public void ShotgunReloadHideBullets()
        {
            ShotgunHands.HideBullets();
        }

        public void ShotgunReloadEndEvent()
        {
            ShotgunHands.ReloadEnd();
        }

        public void ShotgunReloadLoopStart()
        {
            ShotgunHands.ReloadLoopStart();
        }

        public void GrenadeAnimationDeployEvent()
        {
            if (GrenadeHands.GrenadesCount > 0)
                GrenadeHands.DeployedAnimationEvent();
            else
            {
                prevItem = null;
                nextItem = null;

                GrenadeHands.HideImmediately();
                HandsChanged.Invoke(null);
            }
        }

        // Switch props
        public void SwitchObjects()
        {

            if (prevItem != null)
            {
                prevItem.HideProps();
            }

            isWaitForDeploy = false;

            if (nextItem != null)
            {
                nextItem.Deploy();
                nextItem = null;
            }
            else
            {
                bool isDeployed = false;
                foreach (WeaponController item in registeredItems)
                {
                    if (item.IsDeployed)
                    {
                        isDeployed = true;
                        break;
                    }
                }
                if(!isDeployed)
                    animator.Play("Default State", 0, 0);
            }
        }

        // Switch props on interrupt
        public void SwitchObjectsOnInterrupt()
        {
            foreach (WeaponController current in registeredItems)
            {
                if (current == prevItem)
                {
                    prevItem.ShowProps();
                }
                else if (current == nextItem)
                {
                    nextItem.HideProps();
                }
                else
                {
                    current.HideProps();
                }
            }
        }

        public void HitEnd()
        {
            KnifeHands.NextAttack();
        }

        void DeployHideInput()
        {

            if(Input.GetKeyDown(KeyCode.H) && !injector.InjectionInProcess && injector.TargetHealth.HealthFraction < 1f)
            {
                bool isDeployed = false;
                WeaponController deployedItems = null;
                foreach (WeaponController current in registeredItems)
                {
                    if (current.IsDeployed)
                    {
                        isDeployed = true;
                        deployedItems = current;
                        break;
                    }
                }
                if(isDeployed)
                {
                    if (deployedItems.IsCanControl)
                    {
                        deployedBeforeInjection = deployedItems;
                        deployedBeforeInjection.Hide();
                        injector.StartInject();
                    }
                }
                else
                {
                    injector.StartInject();
                }
                return;
            }

            if (Input.GetKeyUp(KeyCode.H))
            {
                if (injector.InjectionInProcess)
                {
                    injector.ForceInjectionEnd();
                }
                return;
            }

            if (injector.InjectionInProcess)
                return;

            if (!isWaitForDeploy)
            {
                foreach (WeaponController item in registeredItems)
                {
                    if (Input.GetKeyDown(item.DeployHideKey) && item.HandsItem.HasAny())
                    {
                        bool isDeployed = false;
                        WeaponController deployedItems = null;
                        foreach (WeaponController current in registeredItems)
                        {
                            if (current.IsDeployed)
                            {
                                isDeployed = true;
                                deployedItems = current;
                                break;
                            }
                        }
                        if (item.IsDeployed)
                        {
                            if (item.IsCanControl)
                            {
                                prevItem = item;
                                nextItem = null;

                                isWaitForDeploy = true;
                                item.Hide();
                                HandsChanged.Invoke(null);
                            }
                        }
                        else
                        {
                            if (isDeployed)
                            {
                                if (deployedItems.IsCanControl)
                                {
                                    nextItem = item;
                                    prevItem = deployedItems;

                                    isWaitForDeploy = true;
                                    deployedItems.Hide();
                                    SwitchObjectsOnInterrupt();
                                    HandsChanged.Invoke(item);
                                }
                            }
                            else
                            {
                                nextItem = item;
                                prevItem = null;
                                item.Deploy();
                                HandsChanged.Invoke(item);
                            }
                        }
                        break;
                    }
                }
            }
        }


        void ControlMouseCursor()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Input.GetKeyDown(KeyCode.End))
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        void InputHandsControllers()
        {
            if (!controller.IsFreezed)
            {
                DeployHideInput();

                handsInput.MouseDown = Input.GetMouseButtonDown(0);
                handsInput.MouseHold = Input.GetMouseButton(0);
                handsInput.MouseUp = Input.GetMouseButtonUp(0);

                handsInput.MouseSecondDown = Input.GetMouseButtonDown(1);
                handsInput.MouseSecondHold = Input.GetMouseButton(1);
                handsInput.MouseSecondUp = Input.GetMouseButtonUp(1);

                handsInput.Reload = Input.GetKeyDown(KeyCode.R);
                handsInput.Flashlight = Input.GetKeyDown(KeyCode.F);
            }

            handsInput.Upgrade = Input.GetKeyDown(KeyCode.V);
        }

        void DeployHandsWeapon()
        {
            bool isDeployed = false;
            float deltaTime = Time.deltaTime;

            if (KnifeHands.IsDeployed)
            {
                isDeployed = true;
                KnifeHands.ApplyInput(handsInput);
                KnifeHands.Update(deltaTime);

                targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, defaultFOV + runAddedFOV * runFraction, Time.deltaTime * 5f);
            }

            if (PistolHands.IsDeployed)
            {
                isDeployed = true;
                PistolHands.ApplyInput(handsInput);
                PistolHands.Update(deltaTime);

                targetCamera.fieldOfView = PistolHands.CameraFov + runAddedFOV * runFraction;
            }

            if (ShotgunHands.IsDeployed)
            {
                isDeployed = true;
                ShotgunHands.ApplyInput(handsInput);
                ShotgunHands.Update(deltaTime);

                targetCamera.fieldOfView = ShotgunHands.CameraFov + runAddedFOV * runFraction;
            }

            if (RifleHands.IsDeployed)
            {
                isDeployed = true;
                RifleHands.ApplyInput(handsInput);
                RifleHands.Update(deltaTime);

                targetCamera.fieldOfView = RifleHands.CameraFov + runAddedFOV * runFraction;
            }

            if (GrenadeHands.IsDeployed)
            {
                isDeployed = true;
                GrenadeHands.ApplyInput(handsInput);
                GrenadeHands.Update(deltaTime);

                targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, defaultFOV + runAddedFOV * runFraction, Time.deltaTime * 5f);
            }

            if (!isDeployed)
            {
                targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, defaultFOV + runAddedFOV * runFraction, Time.deltaTime * 5f);
                controller.PPController.LerpDof(controller.PPController.DofSettings, controller.DefaultDofSettings, dofSmoothSpeed * Time.deltaTime);
            }
        }
    }

    public class HandsChangedEvent : UnityEvent<WeaponController>
    {

    }
}