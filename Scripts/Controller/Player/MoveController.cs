using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CapsuleCollider))]
public class MoveController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerStandState
    {
        public float ControlCameraHeight;
        public float ColliderHeight;
        public float ColliderCenterHeight;
    }

    #region Variables

    [Header("Helper")]
    public MouseControl mouseControl = new MouseControl();
    public HeadBob headBob = new HeadBob();
    public LayerMask groundLayer;
    //public MeshRenderer nearBlurSphere;
    //public GameObject WeatherSystem;

    [Header("Animation Curve")]
    public AnimationCurve HeadBobBlendCurve;
    public AnimationCurve HeadBobPeriodBlendCurve;
    public AnimationCurve RunIncreaseSpeedCurve;
    public AnimationCurve StateChangeCurve;

    [Header("State")]
    public PlayerStandState StandState;
    public PlayerStandState CrouchState;
    protected float stateChangeSpeed = 3f;
    [HideInInspector] public float weightSmooth = 6f;

    [Header("Movement")]
    public float speed = 2f;
    public float maxSpeed = 1f;
    public float jumpSpeed = 4f;
    protected float crouchSpeedMultiplier = 0.75f;
    protected float runSpeedMultiplier = 2f;
    protected float runIncreaseSpeedTime = 1f;
    protected float runSpeedThreshold = 1f;
    protected float gravity = 9.81f;
    protected float stickToGround = 9.81f;

    [Header("Others")]
    public Transform directionRefence;
    public Camera playerCamera;
    public Transform controlCamera;
    public Transform handsHeadBobTarget;
    public TransformNoise cameraNoise;

    protected float idleNoise = 0.5f;
    protected float runNoise = 4f;
    protected float cameraHeadbobWeight = 1f;
    protected float handsHeadbobWeight = 0.3f;
    [HideInInspector] public float handsHeadbobMultiplier = 1f;
    [HideInInspector] public Vector3 playerVelocity = Vector3.zero;
    protected Vector3 oldPlayerVelocity = Vector3.zero;
    protected Vector3 oldPosition;
    protected Vector3 oldHandHeadBobPos;
    protected Vector3 oldCameraHeadBobPos;
    protected Vector3 controlCameraPosition;
    protected float standStateBlend;
    protected float runTime = 0f;
    protected float defaultHandsHeadbobWeight;
    //GameObject weatherEffect;
    //GameObject weatherSound;
    protected Vector3 playerYdir = Vector3.up;

    //public PlayerHands Hands;
    public PostProcessingController PPController;
    public PostProcessingController.DepthOfFieldSettings DefaultDofSettings;
    public PlayerFreezeChangedEvent PlayerFreezeChanged = new PlayerFreezeChangedEvent();
    protected CapsuleCollider charactarCollider;
    protected CharacterController controller;
    //PlayerDamageHandler damageHandler;
    //RaycastHit hitForWeather;

    protected bool isTalking = false;
    protected bool isMove = false;
    protected bool isFreeze = false;
    protected bool isRunning = false;
    protected bool isOldGrounded = false;
    protected bool isCrouching = false;

    #endregion

    #region StringParams

    protected string ForwardAxisParam = "Vertical";
    protected string StrafeAxisParam = "Horizontal";    
    protected string Player = "Player";    

    #endregion

    #region Events

    [HideInInspector] public UnityAction RunStartEvent;
    [HideInInspector] public UnityAction JumpStartEvent;
    [HideInInspector] public UnityAction JumpFallEvent;
    [HideInInspector] public UnityAction JumpEndEvent;
    [HideInInspector] public UnityAction CrouchEvent;
    [HideInInspector] public UnityAction StandUpEvent;

    #endregion

    #region Properties

    public virtual bool IsTalking { get { return isTalking; } set { isTalking = value; } }

    public virtual Vector3 PlayerVelocity { get { return controller.velocity; } }

    public virtual bool IsFreezed { get { return isFreeze; } }

    public virtual bool IsRunning { get { return isRunning; } }

    public virtual bool IsJumpKey { get { if (Input.GetKey(KeyCode.Space)) return true; else return false; } }

    public virtual bool IsCrouching { get { return isCrouching; } }

    public virtual bool IsCrouchKey { get { if (Input.GetKey(KeyCode.LeftControl)) return true; else return false; } }

    public virtual bool IsGrounded { get { return controller.isGrounded; } }

    public virtual bool IsMoving
    {
        get
        {
            if (PlayerVelocity.x != 0f || PlayerVelocity.z != 0f)
                isMove = true;
            else
                isMove = false;

            return isMove;
        }
    }

    public virtual float DefaultHandsHeadbobWeight { get { return defaultHandsHeadbobWeight; } }

    //public PlayerDamageHandler DamageHandler
    //{
    //    get
    //    {
    //        if (damageHandler == null)
    //            damageHandler = GetComponent<PlayerDamageHandler>();

    //        return damageHandler;
    //    }
    //}

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
        //damageHandler = GetComponent<PlayerDamageHandler>();
        charactarCollider = GetComponent<CapsuleCollider>();
        controller = GetComponent<CharacterController>();
        //directionRefence = GameObject.FindGameObjectWithTag(NeckParam).transform;
        //controlCamera = GameObject.FindGameObjectWithTag(NeckParam).transform;
        //handsHeadBobTarget = GameObject.FindGameObjectWithTag(HandsParam).transform;
        playerCamera = Camera.main;
        cameraNoise = Camera.main.GetComponent<TransformNoise>();
    }

    protected virtual void LockMouseCursor()
    {
        if (Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    protected virtual void InitHeadBobSystem()
    {
        defaultHandsHeadbobWeight = handsHeadbobWeight;
        controlCameraPosition = controlCamera.localPosition;
    }

    //void ApplyWeatherFX()
    //{
    //    if (WeatherSystem.activeSelf == false) return;

    //    if (weatherEffect == null)
    //        weatherEffect = GameObject.FindGameObjectWithTag(WeatherEffect);

    //    RaycastHit[] hits = Physics.RaycastAll(transform.position, playerYdir, 1000f);
    //    foreach (var hit in hits)
    //    {
    //        if (weatherEffect != null)
    //        {
    //            if (hit.collider.gameObject.layer.Equals(LayerMask.NameToLayer(Building)))
    //            {
    //                weatherEffect.SetActive(false);
    //                break;
    //            }
    //            else
    //            {
    //                weatherEffect.SetActive(true);
    //            }
    //        }
    //    }
    //}

    protected virtual void UpdateCurrentPhysics()
    {
        oldPlayerVelocity = playerVelocity;
        isOldGrounded = controller.isGrounded;
        oldPosition = transform.position;
    }

    public void UpdateDefaultDeath()
    {
        mouseControl.RotateCameraSmoothlyTo(0, Time.deltaTime);
    }

    public void JumpEndControl()
    {
        if (controller.isGrounded && !isOldGrounded)
        {
            if (JumpEndEvent != null)
            {
                JumpEndEvent();
            }
        }
    }

    public void JumpFallControl()
    {
        if (oldPlayerVelocity.y > 0 && playerVelocity.y < 0)
        {
            if (JumpFallEvent != null)
            {
                JumpFallEvent();
            }
        }
    }

    public virtual void Freeze(bool value)
    {
        //if (!value && !damageHandler.Health.RealIsAlive)
        //    return;

        mouseControl.Enabled = !value;
        isFreeze = value;

        PlayerFreezeChanged.Invoke(isFreeze);
    }


    public void SetNoiseEnabled(bool isEnabled)
    {
        cameraNoise.enabled = isEnabled;
    }

    protected virtual void Move()
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

            //if (!Hands.IsAiming)
            //    cameraNoise.NoiseAmount = Mathf.MoveTowards(cameraNoise.NoiseAmount, runNoise, Time.fixedDeltaTime * 5f);
        }
        else
        {
            runTime -= Time.fixedDeltaTime;

            //if (!Hands.IsAiming)
            //    cameraNoise.NoiseAmount = Mathf.MoveTowards(cameraNoise.NoiseAmount, idleNoise, Time.fixedDeltaTime * 5f);
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
        //Hands.SetRun(runTimeFraction);
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

    protected virtual void Run()
    {
        if (!isRunning)
        {
            isRunning = true;
            if (RunStartEvent != null)
            {
                RunStartEvent();
            }
        }
    }

    protected virtual void Jump()
    {
        playerVelocity.y = jumpSpeed;
        if (JumpStartEvent != null)
            JumpStartEvent();
    }

    protected virtual void Crouch()
    {
        isCrouching = true;

        if (CrouchEvent != null)
        {
            CrouchEvent();
        }
    }

    protected virtual void StandUp()
    {
        isCrouching = false;

        //if (damageHandler.Health.RealIsAlive)
        //{
        //    if (StandUpEvent != null)
        //    {
        //        StandUpEvent();
        //    }
        //}
    }

    protected virtual void ChangeStandState()
    {
        standStateBlend = Mathf.MoveTowards(standStateBlend, isCrouching ? 1f : 0f, Time.deltaTime * stateChangeSpeed);

        charactarCollider.height = Mathf.Lerp(
            StandState.ColliderHeight,
            CrouchState.ColliderHeight,
            StateChangeCurve.Evaluate(standStateBlend)
            );


        Vector3 colliderCenter = charactarCollider.center;

        colliderCenter.y = Mathf.Lerp(
            StandState.ColliderCenterHeight,
            CrouchState.ColliderCenterHeight,
            StateChangeCurve.Evaluate(standStateBlend)
            );
        charactarCollider.center = colliderCenter;

        controller.height = charactarCollider.height;
        controller.center = charactarCollider.center;

        controlCameraPosition.y = Mathf.Lerp(
            StandState.ControlCameraHeight,
            CrouchState.ControlCameraHeight,
            StateChangeCurve.Evaluate(standStateBlend)
            );
    }

    public void SetSensivityMultiplier(float multiplier)
    {
        mouseControl.SensivityMultiplier = multiplier;
    }

    public void ApplyGravity()
    {
        playerVelocity += Vector3.down * gravity * Time.fixedDeltaTime;
        controller.Move(playerVelocity * Time.fixedDeltaTime);
    }

    //public bool IsHumanoidTarget(GameObject target)
    //{
    //    return target.CompareTag(Player) || target.CompareTag(RUSoldier) || target.CompareTag(USSoldier);
    //}

    [System.Serializable]
    public class HeadBob
    {
        public bool Enabled = true;
        public float HeadBobWeight = 5.605194e-45f;
        public Vector2 HeadBobAmount = new Vector2(0.03f, 0.03f);
        public float HeadBobPeriod = 1f;
        public AnimationCurve HeadBobCurveX;
        public AnimationCurve HeadBobCurveY;

        public Vector3 HeadBobPos
        {
            get
            {
                return resultHeadbob;
            }
        }

        Vector3 resultHeadbob;

        public void CalcHeadbob(float currentTime)
        {
            float headBob = Mathf.PingPong(currentTime, HeadBobPeriod) / HeadBobPeriod;

            Vector3 headBobVector = new Vector3();

            headBobVector.x = HeadBobCurveX.Evaluate(headBob) * HeadBobAmount.x;
            headBobVector.y = HeadBobCurveY.Evaluate(headBob) * HeadBobAmount.y;

            headBobVector = Vector3.LerpUnclamped(Vector3.zero, headBobVector, HeadBobWeight);

            if (!Application.isPlaying)
            {
                headBobVector = Vector2.zero;
            }

            if (Enabled)
            {
                resultHeadbob = headBobVector;
            }
        }
    }

    [System.Serializable]
    public class MouseControl
    {
        public bool Enabled;
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public float SensivityMultiplier = 1f;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public float SmoothTime = 15f;
        public bool ClampVerticalRotation = true;

        public string AxisXName = "Mouse X";
        public string AxisYName = "Mouse Y";

        private Quaternion characterTargetRot;
        private Quaternion cameraTargetRot;

        private Transform character;
        private Transform camera;

        public void Init(Transform character, Transform camera)
        {
            characterTargetRot = character.localRotation;
            cameraTargetRot = camera.localRotation;

            this.character = character;
            this.camera = camera;
        }

        public void LookRotation(float deltaTime)
        {
            if (!Enabled)
                return;

            LookRotation(Input.GetAxis(AxisXName) * XSensitivity * SensivityMultiplier, Input.GetAxis(AxisYName) * YSensitivity * SensivityMultiplier, deltaTime);
        }

        public void LookRotation(float yRot, float xRot, float deltaTime)
        {
            characterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            cameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (ClampVerticalRotation)
                cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot);

            character.localRotation = Quaternion.Slerp(character.localRotation, characterTargetRot, SmoothTime * deltaTime);
            camera.localRotation = Quaternion.Slerp(camera.localRotation, cameraTargetRot, SmoothTime * deltaTime);
        }

        public void RotateCameraSmoothlyTo(float xRot, float deltaTime)
        {
            cameraTargetRot = Quaternion.Euler(xRot, 0f, 0f);

            if (ClampVerticalRotation)
                cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot);

            camera.localRotation = Quaternion.Slerp(camera.localRotation, cameraTargetRot, SmoothTime * deltaTime);
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
}

public class PlayerFreezeChangedEvent : UnityEvent<bool>
{ }