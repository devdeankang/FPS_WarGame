using UnityEngine;
using System.Collections;
using System;

namespace WarGame
{
    public class ShotgunAmmoPickupItem : PickupableItem
    {
        public int Count;

        public override BaseItem GetItem()
        {
            ShotgunAmmoItem item = new ShotgunAmmoItem("Item_Shotgun_Ammo");
            item.Add(Count);
            return item;
        }

        public override Type GetItemType()
        {
            return typeof(ShotgunAmmoItem);
        }
    }
}