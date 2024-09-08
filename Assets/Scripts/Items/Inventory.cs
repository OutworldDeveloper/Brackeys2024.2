using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(Order.BEFORE_PLAYER_CHARACTER)]
public class Inventory : MonoBehaviour
{

    public event Action Changed;

    [SerializeField] private int _slotsCount;

    private ItemSlot[] _slots;

    public int SlotsCount => _slots.Length;
    public ItemSlot this[int index] => _slots[index];

    private void Awake()
    {
        _slots = new ItemSlot[_slotsCount];

        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i] = CreateSlot(i);
            _slots[i].Changed += OnSlotChanged;
        }
    }

    protected virtual ItemSlot CreateSlot(int index)
    {
        return new ItemSlot(this, $"Inventory{index}");
    }

    private void OnSlotChanged(ItemSlot slot)
    {
        Changed?.Invoke();
    }

    public bool CanAdd(ItemStack stack)
    {
        foreach (var slot in _slots)
        {
            if (slot.CanAdd(stack) == true)
                return true;
        }

        return false;
    }

    public bool TryAdd(ItemStack stack)
    {
        foreach (var slot in _slots)
        {
            if (slot.TryAdd(stack) == true)
                return true;
        }
    
        return false;
    }

    public int GetAmountOf(Item item)
    {
        int count = 0;

        foreach (var slot in _slots)
        {
            if (slot.IsEmpty == true)
                continue;

            if (slot.Stack.Item == item)
                count += slot.Stack.Count;
        }

        return count;
    }

}
