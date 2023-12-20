using UnityEngine;
using System.Collections;
using System;

namespace WarGame
{
    public class PistolAmmoPickupItem : PickupableItem
    {
        public int Count;

        public override BaseItem GetItem()
        {
            PistolAmmoItem item = new PistolAmmoItem("Pistol_Ammo");
            item.Add(Count);
            return item;
        }

        public override Type GetItemType()
        {
            return typeof(PistolAmmoItem);
        }
    }
}