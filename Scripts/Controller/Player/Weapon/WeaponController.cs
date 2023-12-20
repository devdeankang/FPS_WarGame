using System;
using UnityEngine;
using UnityEngine.Events;

namespace WarGame
{
    [System.Serializable]
    public class WeaponController
    {
        [System.Serializable]
        public struct HandsControllerInput
        {
            public bool MouseDown;
            public bool MouseHold;
            public bool MouseUp;

            public bool MouseSecondDown;
            public bool MouseSecondHold;
            public bool MouseSecondUp;

            public bool Reload;
            public bool Upgrade;
            public bool Flashlight;
        }

        #region StringParams

        [SerializeField] protected string deployedParameter = "Pistol";
        [SerializeField] protected string deployAnim = "Deploy";
        [SerializeField] protected string hideAnim = "Hide";
        [HideInInspector] public string poolsParam = "Pools";
        [HideInInspector] public string hitEffectPoolParam = "@HitEffect Pool";
        #endregion

        [HideInInspector] public Transform pools;        
        public KeyCode DeployHideKey;
        public UnityEvent DeployedEvent;
        public bool LockControlOnDeploy = true;
        public BaseItem HandsItem;
        [SerializeField] protected float hideCrossFade = 0.1f;
        protected GameObject handsRoot;
        protected Animator handsAnimator;
        public GameObject hitEffectPool;

        protected bool isDeployed;
        protected bool isCanControl = true;
        bool hasItem = false;

        protected HandsControllerInput lastInputData;
        protected HandsControllerInput currentInputData;
        protected PlayerInventory playerInventory;
        private UnityEvent pickedUp = new UnityEvent();


        public bool IsDeployed { get { return isDeployed; } }

        public bool IsCanControl { get { return isCanControl; } }

        public UnityEvent PickedUp
        {
            get
            {
                return pickedUp;
            }

            private set
            {
                pickedUp = value;
            }
        }

        public virtual void Init(GameObject root)
        {
            handsRoot = root;
            handsAnimator = handsRoot.GetComponent<Animator>();
            playerInventory = handsRoot.GetComponent<PlayerInventory>();
            HandsItem = CreateItem();
            HandsItem.ItemChanged.AddListener(ChangedItem);

            hasItem = HandsItem.GetCurrentCount() > 0;
            playerInventory.AddItem(HandsItem);
            HideProps();

            InitHitEffectPool();
        }

        void InitHitEffectPool()
        {
            if (pools == null)
                pools = GameObject.FindGameObjectWithTag(poolsParam).transform;

            if (hitEffectPool == null)
            {
                hitEffectPool = new GameObject();
                hitEffectPool.name = hitEffectPoolParam;
                hitEffectPool.transform.SetParent(pools);
            }
        }

        void ChangedItem()
        {
            bool newHasItem = HandsItem.GetCurrentCount() > 0;

            if(hasItem != newHasItem)
            {
                hasItem = newHasItem;
                if(hasItem)
                {
                    pickedUp.Invoke();
                }
            }
        }
                
        public virtual BaseItem CreateItem()
        {
            return new BaseItem("Item_Hands");
        }


        public virtual void Deploy()
        {
            if (handsAnimator == null)
            {
                throw new System.Exception("Hands animator null reference exception");
            }

            if (isDeployed)
            {
                return;
            }

            if (LockControlOnDeploy)
            {
                isCanControl = false;
            }
            handsAnimator.Play(deployAnim);
            SetDeployed(true);
        }
        
        public virtual void SetFullyDeployed()
        {
            isCanControl = true;
        }

        
        public virtual void SetDeployed(bool value)
        {
            isDeployed = value;
            handsAnimator.SetBool(deployedParameter, isDeployed);

            if (LockControlOnDeploy)
                isCanControl = false;

            if (value)
            {
                if (DeployedEvent != null)
                {
                    DeployedEvent.Invoke();
                }
            }
        }

        public virtual void Hide()
        {
            if (!isDeployed)
                return;

            handsAnimator.CrossFadeInFixedTime(hideAnim, 0.1f, 0);
            SetDeployed(false);
        }

        public virtual void HideImmediately()
        {
            handsAnimator.Play(hideAnim, 0, 1f);
            SetDeployed(false);
        }

        public virtual void Update(float deltaTime)
        {

        }

        public virtual void ApplyInput(HandsControllerInput inputData)
        {
            currentInputData = inputData;

            if (inputData.MouseDown)
                InputEvent(InputEventType.MouseDown);
            if (inputData.MouseHold)
                InputEvent(InputEventType.MouseHold);
            if (inputData.MouseUp)
                InputEvent(InputEventType.MouseUp);
            if (inputData.MouseSecondDown)
                InputEvent(InputEventType.MouseSecondDown);
            if (inputData.MouseSecondHold)
                InputEvent(InputEventType.MouseSecondHold);
            if (inputData.MouseSecondUp)
                InputEvent(InputEventType.MouseSecondUp);
            if (inputData.Upgrade)
                InputEvent(InputEventType.Upgrade);
            if (inputData.Reload)
                InputEvent(InputEventType.Reload);
            if (inputData.Flashlight)
                InputEvent(InputEventType.Flashlight);

            lastInputData = inputData;
        }

        public virtual void InputEvent(InputEventType et)
        {

        }

        public enum InputEventType
        {
            MouseDown,
            MouseHold,
            MouseUp,
            MouseSecondDown,
            MouseSecondHold,
            MouseSecondUp,
            Reload,
            Upgrade,
            Flashlight
        }

        public virtual void ShowProps()
        {

        }

        public virtual void HideProps()
        {

        }
    }

    public interface IAmmoItemGetter
    {
        BaseItem GetAmmoItem();
    }
}