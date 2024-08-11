using UnityEngine;
using System.Collections;
using System;

namespace WarGame
{
    public class PickupUIBlock : MonoBehaviour
    {
        public PickupUIIcon[] Icons;

        public PickupUIInfo PickupInfoTemplate;
        public RectTransform EmptyTemplate;

        [SerializeField]
        private PlayerInventory inventory;

        public PlayerInventory Inventory
        {
            get
            {
                if (inventory == null)
                    inventory = GameObject.FindObjectOfType<PlayerInventory>();

                return inventory;
            }

            set
            {
                inventory = value;
            }
        }

        private void Awake()
        {
            Inventory.PickedUpInfoEvent.AddListener(itemPickedUp);
        }

        private void itemPickedUp(int addedCount, BaseItem item, BaseItem baseItem)
        {
            if (item.GetCurrentCount() == 0)
                return;

            Sprite resultIcon = null;
            foreach(PickupUIIcon icon in Icons)
            {
                if(icon.IconName.Equals(baseItem.IconName))
                {
                    resultIcon = icon.Icon;
                    break;
                }
            }

            if(resultIcon != null)
            {
                RectTransform emptyClone = Instantiate(EmptyTemplate, EmptyTemplate.parent);
                emptyClone.gameObject.SetActive(true);

                PickupUIInfo info = Instantiate(PickupInfoTemplate, PickupInfoTemplate.transform.parent);
                info.GetComponent<UISmoothFollow>().Target = emptyClone;
                info.gameObject.SetActive(true);
                info.Show("+" + addedCount, resultIcon, () =>
                {
                    Destroy(emptyClone.gameObject);
                });
            }
        }
    }

    [System.Serializable]
    public class PickupUIIcon
    {
        public string IconName;
        public Sprite Icon;
    }
}