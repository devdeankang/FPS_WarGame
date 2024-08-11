using UnityEngine;
using System.Collections;
using System;

namespace WarGame
{
    public class MedKitPickupItem : PickupableItem
    {
        public int Count;

        public override BaseItem GetItem()
        {
            MedkitItem item = new MedkitItem("MedKit");
            item.Add(Count);
            return item;
        }

        public override Type GetItemType()
        {
            return typeof(MedkitItem);
        }
    }
}