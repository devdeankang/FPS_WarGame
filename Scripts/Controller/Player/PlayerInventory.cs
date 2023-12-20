using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WarGame
{
    public class PlayerInventory : MonoBehaviour
    {
        public ItemPickedUpInfoEvent PickedUpInfoEvent = new ItemPickedUpInfoEvent();

        List<BaseItem> itemsList = new List<BaseItem>();
        Dictionary<Type, BaseItem> items = new Dictionary<Type, BaseItem>();

        Dictionary<Type, ItemPickedUpEvent> events = new Dictionary<Type, ItemPickedUpEvent>();

        public class ItemPickedUpEvent : UnityEvent<BaseItem>
        {

        }

        public class ItemPickedUpInfoEvent : UnityEvent<int, BaseItem, BaseItem>
        {

        }

        public void AddItem(BaseItem item)
        {
            if (item == null)
                return;

            BaseItem baseItem;
            Type itemType = item.GetType();

            int addedCount = 0;
            if (items.TryGetValue(itemType, out baseItem))
            {
                addedCount = baseItem.Add(item.GetCurrentCount());
            }
            else
            {
                baseItem = item;
                addedCount = item.GetCurrentCount();
                items.Add(itemType, baseItem);
                itemsList.Add(baseItem);
            }

            PickedUpInfoEvent.Invoke(addedCount, item, baseItem);

            ItemPickedUpEvent ev;

            if (events.TryGetValue(itemType, out ev))
            {
                ev.Invoke(baseItem);
            }
        }

        public ItemPickedUpEvent GetPickedUpEvent<T>() where T : BaseItem
        {
            Type t = typeof(T);
            ItemPickedUpEvent ev;
            if(events.TryGetValue(t, out ev))
            {
                return ev;
            }
            else
            {
                ev = new ItemPickedUpEvent();
                events.Add(t, ev);

                return ev;
            }
        }

        public T GetItem<T>() where T : BaseItem
        {
            BaseItem item;

            if (items.TryGetValue(typeof(T), out item))
            {
                return item as T;
            }
            else
            {
                throw new ItemNotExistsException(string.Format("Item with type {0} is not exist", typeof(T).ToString()));
            }
        }

        public BaseItem GetItem(Type t)
        {
            BaseItem item;

            if (items.TryGetValue(t, out item))
            {
                return item;
            }
            else
            {
                throw new ItemNotExistsException(string.Format("Item with type {0} is not exist", t.ToString()));
            }
        }

        public int ConsumeItem<T>(int count) where T : BaseItem
        {
            BaseItem item;

            if (items.TryGetValue(typeof(T), out item))
            {
                return item.Consume(count);
            }
            else
            {
                throw new ItemNotExistsException(string.Format("Item with type {0} is not exist", typeof(T).ToString()));
            }
        }
    }

    public class ItemNotExistsException : Exception
    {
        public ItemNotExistsException(string message) : base(message)
        {
        }
    }

    [Serializable]
    public class BaseItem
    {
        public string ItemName;
        public int MaxCapacity = 0;
        public string IconName;

        public ItemChangedEvent ItemChanged = new ItemChangedEvent();

        [SerializeField]
        int currentCount;

        public BaseItem(string itemName)
        {
            ItemName = itemName;
        }

        public int Add(int count)
        {
            int delta = 0;
            int addedCount = currentCount + count;

            currentCount += count;
            currentCount = Mathf.Clamp(currentCount, 0, MaxCapacity);

            delta = addedCount - currentCount;

            Added(count);
            ItemChanged.Invoke();

            return count - delta;
        }

        public bool IsCanAdd()
        {
            return MaxCapacity == 0 || currentCount < MaxCapacity;
        }

        public int GetCurrentCount()
        {
            return currentCount;
        }

        public bool HasAny()
        {
            return currentCount > 0;
        }

        public int Consume(int count)
        {
            if (currentCount >= count)
            {
                currentCount -= count;
                Consumed(count);
                ItemChanged.Invoke();
                return count;
            }
            else
            {
                currentCount = 0;
                Consumed(currentCount);
                ItemChanged.Invoke();
                return currentCount;
            }
        }

        public virtual void Consumed(int count)
        {

        }

        public virtual void Added(int count)
        {

        }

    }

    public class ItemChangedEvent : UnityEvent
    {

    }
}