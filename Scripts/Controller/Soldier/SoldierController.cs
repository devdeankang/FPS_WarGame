using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace WarGame
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SoldierData))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class SoldierController : MonoBehaviour
    {
        [Header("Audio Clips")]
        public AudioClip gunFireSound;
        public AudioClip deathSound;

        [Header("Destinations")]
        public Transform startPoint;
        public List<Transform> destinations;        
        public float occupiedDistance = 8f;
        public Transform destination;

        [Header("Others")]
        public LayerMask FireCollisionLayer;

        [HideInInspector] public List<GameObject> detectedTargets;
        Vector3 offset = Vector3.zero;

        public SoldierData data;
        public SoldierHealth health;
        public RaderSystem rader;
        GameObject raderSystem;
        StateMachine<SoldierController> soldierSM;
        Dictionary<SoldierState, IState<SoldierController>> soldierStates = new Dictionary<SoldierState, IState<SoldierController>>();
        [HideInInspector] public Animator animator;
        [HideInInspector] public CharacterController characterController;
        [HideInInspector] public NavMeshAgent agent;

        [HideInInspector] public bool isForcedRaderShutDown = false;

        public enum SoldierState
        {
            Idle,
            Track,
            Move,
            Occupied,
            Attack,
            Dead
        }

        public bool IsEnemy { get { return data.IsRuSoldier; } }
        public float ShootDistance { get { return data.sightDistance * 0.5f; } }
        public bool IsOccupied
        {
            get
            {
                float currentDistance = (destination.transform.position - this.transform.position).sqrMagnitude;
                if (currentDistance <= occupiedDistance)
                    return true;
                else
                    return false;
            }
        }

        public bool IsSightDistance
        {
            get
            {
                foreach(var targetData in data.targetDistances)
                {
                    if (targetData.Value <= data.sightDistance) return true;
                }
                return false;
            }
        }
        
        public bool IsViewAngle
        {            
            get
            {
                foreach(var targetData in data.targetDistances)
                {
                    GameObject target = targetData.Key;
                    Vector3 dirToTarget = (target.transform.position - this.transform.position).normalized;
                    if (Vector3.Angle(this.transform.forward, dirToTarget) < data.viewAngle / 2) return true;
                }
                return false;
            }
        }

        public bool NeedRaderSystem
        {
            get
            {
                if (IsSightDistance && IsViewAngle) return true;
                else return false;
            }
        }


        private void Awake()
        {
            GetComponents();
            SetPlayerStates();            
        }

        private void OnEnable()
        {                        
         
        }

        void Start()
        {
            data.MuzzleFX.SetActive(false);
            SetNaviAgent(); // default   
        }

        void Update()
        {   
            ApplyGravity();
            OnRaderSystem();
            soldierSM.OperateUpdate();
        }

        private void FixedUpdate()
        {
            soldierSM.OperateFixedUpdate();
        }

        private void OnDisable()
        {
            health.Health = health.StartHealth;
        }

        public void OnRaderSystem()
        {
            if(isForcedRaderShutDown)
            {
                raderSystem.SetActive(false);
                return;
            }

            if (NeedRaderSystem)
                raderSystem.SetActive(true);
            else
                raderSystem.SetActive(false);
        }

        void GetComponents()
        {
            animator = GetComponent<Animator>();
            characterController = GetComponent<CharacterController>();
            data = GetComponent<SoldierData>();
            health = GetComponent<SoldierHealth>();
            agent = GetComponent<NavMeshAgent>();
            rader = GetComponentInChildren<RaderSystem>();
            raderSystem = GetComponentInChildren<RaderSystem>().gameObject;
        }

        public void SetNaviAgent(float movementSpeed = 0.05f, bool autoBreaking = false, float angularSpeed = 360f, float stoppingDistance = 3f)
        {
            agent.angularSpeed = angularSpeed;
            agent.speed = movementSpeed;
            agent.autoBraking = autoBreaking;
            agent.stoppingDistance = stoppingDistance;            
        }

        public bool IsAttackableTarget(GameObject target)
        {
            if (target == null) return false;

            bool isAttackable = false;
            GameObject hands = data.handsObject;
            Vector3 rayDirection = this.transform.forward;
            if (target != null) rayDirection = (target.transform.position - hands.transform.position).normalized;

            Ray r = new Ray(hands.transform.position, rayDirection);
            RaycastHit hitInfo;

            if (Physics.Raycast(r, out hitInfo, ShootDistance, FireCollisionLayer, QueryTriggerInteraction.Ignore))
            {
                if (hitInfo.collider.gameObject == target)
                    isAttackable = true;
                else
                    isAttackable = false;
            }

            return isAttackable;
        }


        public void ApplyGravity()
        {
            if (characterController.isGrounded) return;

            Vector3 velocity = Vector3.zero;
            velocity += Vector3.down * data.gravity * Time.fixedDeltaTime;
            characterController.Move(velocity);
        }


        public bool IsShootDistance(GameObject target)
        {
            if (!NeedRaderSystem) return false;

            if (rader.GetSqrMagnitude(target) <= ShootDistance)
                return true;
            else
                return false;
        }

        #region FSM
        void SetPlayerStates()
        {
            IState<SoldierController> idle = new SoldierIdle();
            IState<SoldierController> track = new SoldierTrack();
            IState<SoldierController> move = new SoldierMove();
            IState<SoldierController> occupied = new SoldierOccupied();
            IState<SoldierController> attack = new SoldierAttack();
            IState<SoldierController> dead = new SoldierDead();

            soldierStates.Add(SoldierState.Idle, idle);
            soldierStates.Add(SoldierState.Track, track);
            soldierStates.Add(SoldierState.Move, move);
            soldierStates.Add(SoldierState.Occupied, occupied);
            soldierStates.Add(SoldierState.Attack, attack);
            soldierStates.Add(SoldierState.Dead, dead);

            if (soldierSM == null)
            {
                soldierSM = new StateMachine<SoldierController>(this, soldierStates[SoldierState.Idle]);
            }
        }

        //void SetDefaultState()
        //{
        //    if (soldierSM == null)
        //    {
        //        soldierSM = new StateMachine<SoldierController>(this, soldierStates[SoldierState.Idle]);
        //    }
        //}

        public void ChangeState(SoldierState state)
        {
            soldierSM.SetState(soldierStates[state]);
        }
        #endregion

        
    }
}