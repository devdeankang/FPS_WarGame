using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool<T>
{
    public Dictionary<T, ItemPool<T>> ObjectsInPool = new Dictionary<T, ItemPool<T>>();

    private Func<T> func;

    public Pool(Func<T> inputObject, int size)
    {
        func = inputObject;
        ObjectsInPool = new Dictionary<T, ItemPool<T>>();
        for (int i = 0; i < size; i++)
        {
            CreateNewObject();
        }
    }

    public Pool(List<T> objectsPool)
    {
        foreach (var item in objectsPool)
        {
            var newObject = new ItemPool<T>();
            newObject.ItemObject = item;
            ObjectsInPool.Add(newObject.ItemObject, newObject);
        }
    }

    public T GetFromPool()
    {
        foreach (var item in ObjectsInPool)
        {
            if (item.Value.Free)
            {
                item.Value.Free = false;
                return item.Value.ItemObject;
            }
        }
        return CreateNewObject();
    }

    public void BackToPool(T item)
    {
        if (ObjectsInPool.ContainsKey(item))
        {
            ObjectsInPool[item].Free = true;
        }
    }

    private T CreateNewObject()
    {
        var newObject = new ItemPool<T>();
        newObject.ItemObject = func();
        ObjectsInPool.Add(newObject.ItemObject, newObject);
        return newObject.ItemObject;
    }
}

public class ItemPool<T>
{
    public T ItemObject;
    public bool Free = true;
}
