using UnityEngine;
using System.Collections;
using System;

namespace WarGame
{ 
    public class PistolPickupItem : PickupableItem
    {
        public override BaseItem GetItem()
        {
            PistolItem item = new PistolItem("Item_Pistol");
            item.Add(1);
            return item;
        }

        public override Type GetItemType()
        {
            return typeof(PistolItem);
        }
    }
}