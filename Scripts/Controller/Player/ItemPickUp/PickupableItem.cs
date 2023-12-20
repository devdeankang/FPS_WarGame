using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

namespace WarGame
{
    [RequireComponent(typeof(Collider))]
    public class PickupableItem : MonoBehaviour, IPlayerAction
    {
        public bool isAutoDestroyAfterPickUp = true;
        public bool isInfinity = false;
        public UnityEvent ItemPickedUp = new UnityEvent();
                
        bool isPickedUp;


        public virtual bool IsCanPickUp { get {  return true; } }

        public bool IsPickedUp { get { return isPickedUp; } }


        public BaseItem PickupItem()
        {
            ItemPickedUp.Invoke();
            PickUpEvent();
            if (isPickedUp && !isInfinity)
                return null;

            if(isAutoDestroyAfterPickUp)
                Destroy(gameObject);

            isPickedUp = true;
            return GetItem();
        }

        public virtual Type GetItemType()
        {
            return typeof(BaseItem);
        }

        public virtual void PickUpEvent()
        {

        }

        public void UseEnd()
        {

        }

        public void UseStart()
        {
            PickupItem();
        }

        public void UseUpdate()
        {

        }

        public virtual BaseItem GetItem()
        {
            return new BaseItem("Item_Null");
        }
    }
}