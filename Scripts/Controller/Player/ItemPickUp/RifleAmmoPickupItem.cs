using UnityEngine;
using System.Collections;
using System;

namespace WarGame
{
    public class RifleAmmoPickupItem : PickupableItem
    {
        public int Count = 100;
        public Animator BoxAnimator;

        public override bool IsCanPickUp
        {
            get
            {
                return BoxAnimator.GetInteger("State") < 2;
            }
        }

        public override BaseItem GetItem()
        {
            RifleAmmoItem item = new RifleAmmoItem("Item_Rifle_Ammo");
            item.Add(Count);
            return item;
        }

        public override Type GetItemType()
        {
            return typeof(RifleAmmoItem);
        }

        public override void PickUpEvent()
        {
            base.PickUpEvent();

            if (IsPickedUp)
            {
                BoxAnimator.SetInteger("State", 2);
            }
            else
            {
                BoxAnimator.SetInteger("State", 1);
            }
        }
    }
}