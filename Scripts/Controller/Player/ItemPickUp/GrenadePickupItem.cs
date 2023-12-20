using UnityEngine;
using System.Collections;
using System;

namespace WarGame
{
    public class GrenadePickupItem : PickupableItem
    {
        public override BaseItem GetItem()
        {
            GrenadeItem item = new GrenadeItem("Item_Grenade");
            item.Add(1);
            return item;
        }

        public override Type GetItemType()
        {
            return typeof(GrenadeItem);
        }
    }
}