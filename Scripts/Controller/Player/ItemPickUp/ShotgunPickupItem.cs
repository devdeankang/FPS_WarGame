using UnityEngine;
using System.Collections;
using System;

namespace WarGame
{
    public class ShotgunPickupItem : PickupableItem
    {
        public override BaseItem GetItem()
        {
            ShotgunItem item = new ShotgunItem("Item_Shotgun");
            item.Add(1);
            return item;
        }

        public override Type GetItemType()
        {
            return typeof(ShotgunItem);
        }
    }
}