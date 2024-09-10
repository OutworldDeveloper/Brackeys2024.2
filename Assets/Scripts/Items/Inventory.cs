using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(Order.BEFORE_PLAYER_CHARACTER)]
public class Inventory : MonoBehaviour
{

    public event Action<Item> ItemAdded;
    public event Action<Item> ItemRemoved;

    private readonly List<Item> _items = new List<Item>();

    public Item[] Items => _items.ToArray();
    public bool IsEmpty => _items.Count == 0;

    public bool HasItem(Item item)
    {
        foreach (var checkItem in _items)
        {
            if (checkItem == item)
                return true;
        }

        return false;
    }

    public void AddItem(Item item)
    {
        _items.Add(item);
        ItemAdded?.Invoke(item);
    }

    public void RemoveItem(Item item)
    {
        _items.Remove(item);
        ItemRemoved?.Invoke(item);
    }

}
