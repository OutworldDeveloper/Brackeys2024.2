using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(Order.BEFORE_PLAYER_CHARACTER)]
public class Inventory : MonoBehaviour, IEnumerable<Item>
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

    public void AddItem(Item item, bool anounce = true)
    {
        _items.Add(item);
        if (anounce)
            ItemAdded?.Invoke(item);
    }

    public void RemoveItem(Item item)
    {
        _items.Remove(item);
        ItemRemoved?.Invoke(item);
    }

    public IEnumerator<Item> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

}
