using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WarGame
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerController : MoveController
    {

        #region Variables

        [Header("Helper")]
        public MeshRenderer nearBlurSphere;
        public GameObject WeatherSystem;
        public PlayerHands Hands;                
        GameObject weatherEffect;
        GameObject weatherSound;
        protected PlayerDamageHandler damageHandler;         

        #endregion

        #region StringParams


        string HandsParam = "Hands";
        string NeckParam = "Neck";        
        string USSoldier = "USSoldier";
        string RUSoldier = "RUSoldier";
        string Building = "Building";
        string WeatherEffect = "UniStormEffect";

        #endregion

        #region Properties

        public PlayerDamageHandler DamageHandler
        {
            get
            {
                if (damageHandler == null)
                    damageHandler = GetComponent<PlayerDamageHandler>();

                return damageHandler;
            }
        }

        #endregion

        void Awake()
        {            
            GetComponents();
        }

        void Start()
        {
            mouseControl.Init(transform, controlCamera);
            InitHeadBobSystem();
        }

        void Update()
        {
            LockMouseCursor();
            ApplyWeatherFX();
        }

        void FixedUpdate()
        {
            mouseControl.LookRotation(Time.fixedDeltaTime);

            if (controller.isGrounded)
            {
                Move();
            }

            if (!controller.isGrounded) ApplyGravity();

            JumpFallControl();
            JumpEndControl();

            UpdateCurrentPhysics();
        }

        void GetComponents()
        {
            damageHandler = GetComponent<PlayerDamageHandler>();
            charactarCollider = GetComponent<CapsuleCollider>();
            controller = GetComponent<CharacterController>();
            directionRefence = GameObject.FindGameObjectWithTag(NeckParam).transform;
            controlCamera = GameObject.FindGameObjectWithTag(NeckParam).transform;
            handsHeadBobTarget = GameObject.FindGameObjectWithTag(HandsParam).transform;
            playerCamera = Camera.main;
            cameraNoise = Camera.main.GetComponent<TransformNoise>();
        }

        void ApplyWeatherFX()
        {
            if (WeatherSystem.activeSelf == false) return;

            if (weatherEffect == null)
                weatherEffect = GameObject.FindGameObjectWithTag(WeatherEffect);

            RaycastHit[] hits = Physics.RaycastAll(transform.position, playerYdir, 1000f);
            foreach (var hit in hits)
            {
                if (weatherEffect != null)
                {
                    if (hit.collider.gameObject.layer.Equals(LayerMask.NameToLayer(Building)))
                    {
                        weatherEffect.SetActive(false);
                        break;
                    }
                    else
                    {
                        weatherEffect.SetActive(true);
                    }
                }
            }
        }

        public override void Freeze(bool value)
        {
            if (!value && !damageHandler.Health.RealIsAlive)
                return;

            base.Freeze(value);
        }

        protected override void Move()
        {
            if (IsTalking == true) return;

            float h = Input.GetAxis(StrafeAxisParam);
            float v = Input.GetAxis(ForwardAxisParam);

            if (isFreeze)
            {
                h = 0;
                v = 0;
            }

            headBob.CalcHeadbob(Time.time);

            handsHeadBobTarget.localPosition -= oldHandHeadBobPos;
            handsHeadBobTarget.localPosition += headBob.HeadBobPos * handsHeadbobWeight * handsHeadbobMultiplier;

            controlCamera.localPosition -= oldCameraHeadBobPos;
            controlCamera.localPosition = controlCameraPosition;

            controlCamera.localPosition += headBob.HeadBobPos * cameraHeadbobWeight;

            oldHandHeadBobPos = headBob.HeadBobPos * handsHeadbobWeight * handsHeadbobMultiplier;
            oldCameraHeadBobPos = headBob.HeadBobPos * cameraHeadbobWeight;

            Vector3 moveVector = directionRefence.forward * v + directionRefence.right * h;
            Vector3 playerXZVelocity = Vector3.Scale(playerVelocity, new Vector3(1, 0, 1));

            float speed = this.speed;
            if (Input.GetKey(KeyCode.LeftShift) && !Input.GetMouseButton(1) && !Input.GetMouseButton(0) && playerXZVelocity.magnitude >= runSpeedThreshold && !isCrouching)
            {
                runTime += Time.fixedDeltaTime;
                Run();

                if (!Hands.IsAiming)
                    cameraNoise.NoiseAmount = Mathf.MoveTowards(cameraNoise.NoiseAmount, runNoise, Time.fixedDeltaTime * 5f);
            }
            else
            {
                runTime -= Time.fixedDeltaTime;

                if (!Hands.IsAiming)
                    cameraNoise.NoiseAmount = Mathf.MoveTowards(cameraNoise.NoiseAmount, idleNoise, Time.fixedDeltaTime * 5f);
                isRunning = false;
            }

            if (Input.GetKeyDown(KeyCode.LeftControl) && !isFreeze)
            {
                Crouch();
            }

            if ((Input.GetKeyUp(KeyCode.LeftControl) && !isFreeze)
                || (Input.GetKey(KeyCode.LeftControl) && isFreeze && isCrouching))
            {
                StandUp();
            }

            ChangeStandState();
            runTime = Mathf.Clamp(runTime, 0, runIncreaseSpeedTime);
            float runTimeFraction = runTime / runIncreaseSpeedTime;
            Hands.SetRun(runTimeFraction);
            float runMultiplier = Mathf.Lerp(1, runSpeedMultiplier, RunIncreaseSpeedCurve.Evaluate(runTimeFraction));
            speed *= runMultiplier;
            if (isCrouching)
                speed *= crouchSpeedMultiplier;

            Ray r = new Ray(transform.position, Vector3.down);
            RaycastHit hitInfo;

            Physics.SphereCast(r, charactarCollider.radius, out hitInfo, charactarCollider.height / 2f, groundLayer);

            Vector3 desiredVelocity = Vector3.ProjectOnPlane(moveVector, hitInfo.normal) * speed;
            playerVelocity.x = desiredVelocity.x;
            playerVelocity.z = desiredVelocity.z;
            playerVelocity.y = -stickToGround;

            Vector3 calculatedVelocity = playerVelocity;
            calculatedVelocity.y = 0;

            float speedFraction = calculatedVelocity.magnitude / maxSpeed;
            headBob.HeadBobWeight = Mathf.Lerp(headBob.HeadBobWeight, HeadBobBlendCurve.Evaluate(speedFraction), weightSmooth * Time.fixedDeltaTime);
            headBob.HeadBobPeriod = HeadBobPeriodBlendCurve.Evaluate(speedFraction);

            if (controller.isGrounded)
            {
                if (Input.GetKey(KeyCode.Space) && !isCrouching && !isFreeze)
                {
                    Jump();
                }
                controller.Move(playerVelocity * Time.fixedDeltaTime);
            }
        }

        public bool IsHumanoidTarget(GameObject target)
        {
            return target.CompareTag(Player) || target.CompareTag(RUSoldier) || target.CompareTag(USSoldier);
        }

    }

}