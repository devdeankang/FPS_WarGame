using UnityEngine;
using System.Collections;
using System;

namespace WarGame
{
    public class RiflePickupItem : PickupableItem
    {
        public override BaseItem GetItem()
        {
            RifleItem item = new RifleItem("Item_Rifle");
            item.Add(1);
            return item;
        }

        public override Type GetItemType()
        {
            return typeof(RifleItem);
        }
    }
}