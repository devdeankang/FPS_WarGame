using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WarGame
{
    public class PlayerItemsPickupController : MonoBehaviour
    {        
        public LayerMask ItemsLayerMask;
        public float HintsDistance = 5f;
        public float PickupDistance = 2f;
        [SerializeField] private Transform sphereCheckCenter;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private PlayerInventory playerInventory;
        public RectTransform ItemUITemplate;
        public AnimationCurve HintAlphaCurve;

        List<PickupableItem> itemsUIToDestroy = new List<PickupableItem>();
        Dictionary<PickupableItem, PickupableItemUI> showedItems = new Dictionary<PickupableItem, PickupableItemUI>();

        public PlayerInventory PlayerInventory
        {
            get
            {
                if (playerInventory == null)
                    playerInventory = GetComponent<PlayerInventory>();

                return playerInventory;
            }

            set
            {
                playerInventory = value;
            }
        }

        public Camera PlayerCamera
        {
            get
            {
                if(playerCamera == null)
                {
                    PlayerController controller = GameObject.FindObjectOfType<PlayerController>();
                    playerCamera = controller.playerCamera;
                }

                return playerCamera;
            }

            set
            {
                playerCamera = value;
            }
        }

        public Transform SphereCheckCenter
        {
            get
            {
                if (sphereCheckCenter == null)
                {
                    sphereCheckCenter = PlayerCamera.transform;
                }
                return sphereCheckCenter;
            }

            set
            {
                sphereCheckCenter = value;
            }
        }

        private void Update()
        {
            Collider[] colliders = Physics.OverlapSphere(SphereCheckCenter.position, HintsDistance, ItemsLayerMask, QueryTriggerInteraction.Collide);

            foreach(Collider c in colliders)
            {
                PickupableItem item = c.GetComponent<PickupableItem>();

                if(item != null && item.IsCanPickUp)
                {
                    float distance = Vector3.Distance(item.transform.position, SphereCheckCenter.position);
                    float dot = Vector3.Dot((item.transform.position - PlayerCamera.transform.position).normalized, PlayerCamera.transform.forward);

                    if (dot > 0)
                    {
                        if (!showedItems.ContainsKey(item))
                        {
                            showItem(item);
                        }
                        else
                        {
                            float distanceFraction = (distance - PickupDistance) / (HintsDistance - PickupDistance);
                            distanceFraction = Mathf.Clamp01(distanceFraction);
                            float alpha = HintAlphaCurve.Evaluate(1f - distanceFraction);

                            PickupableItemUI itemUI = showedItems[item];
                            itemUI.CanDestroy = false;

                            bool isPickupable = distance <= PickupDistance;

                            itemUI.SetPickupable(isPickupable);
                            itemUI.SetFraction(alpha);
                        }
                    }
                }
            }

            itemsUIToDestroy.Clear();
            foreach (PickupableItem item in showedItems.Keys)
            {
                PickupableItemUI itemUI = showedItems[item];

                if (itemUI.CanDestroy)
                {
                    itemsUIToDestroy.Add(item);
                }
                else
                {
                    updateItem(item, itemUI);
                }
            }

            foreach (PickupableItem item in itemsUIToDestroy)
            {
                hideItem(item);
            }
        }

        void showItem(PickupableItem item)
        {
            RectTransform itemUIRoot = Instantiate(ItemUITemplate, ItemUITemplate.parent);
            PickupableItemUI itemUI = new PickupableItemUI(itemUIRoot);

            showedItems.Add(item, itemUI);
            itemUI.SetFraction(0f);
            itemUI.SetPickupable(false);
        }

        void updateItem(PickupableItem item, PickupableItemUI itemUI)
        {
            Vector3 screenPosition = PlayerCamera.WorldToScreenPoint(item.GetComponent<Collider>().bounds.center);
            itemUI.SetScreenPosition(screenPosition);
            itemUI.CanDestroy = true;
        }

        void hideItem(PickupableItem item)
        {
            PickupableItemUI itemUI;

            if(showedItems.TryGetValue(item, out itemUI))
            {
                itemUI.DestroyUI();
            }

            showedItems.Remove(item);
        }

        [System.Serializable]
        public class PickupableItemUI
        {
            public bool CanDestroy = false;
            RectTransform root;

            public PickupableItemUI(RectTransform root)
            {
                this.root = root;
                this.root.gameObject.SetActive(true);
            }

            public void DestroyUI()
            {
                Destroy(root.gameObject);
            }

            public void SetPickupable(bool value)
            {
                root.GetChild(0).gameObject.SetActive(value);
            }

            public void SetFraction(float value)
            {
                root.GetComponent<CanvasGroup>().alpha = value;
            }

            public void SetScreenPosition(Vector2 position)
            {
                root.position = position;
            }
        }
    }
}