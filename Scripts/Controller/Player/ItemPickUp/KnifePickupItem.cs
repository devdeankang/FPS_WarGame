using UnityEngine;
using System.Collections;
using System;

namespace WarGame
{
    public class KnifePickupItem : PickupableItem
    {
        public override BaseItem GetItem()
        {
            KnifeItem item = new KnifeItem("Item_Knife");
            item.Add(1);
            return item;
        }

        public override Type GetItemType()
        {
            return typeof(KnifeItem);
        }
    }
}