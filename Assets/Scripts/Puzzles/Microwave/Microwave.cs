using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Door))]
public class Microwave : DoorBlocker
{

    [SerializeField] private float _duration = 5f;
    [SerializeField] private ItemPedistal _itemPedistal;

    private TimeSince _timeSinceLastStart = TimeSince.Never;

    public Door Door { get; private set; }
    public bool IsOn { get; private set; }
    public bool ContainsItem => _itemPedistal.DisplayItem != null;

    private void Awake()
    {
        Door = GetComponent<Door>();
    }

    private void Update()
    {
        if (IsOn == false)
            return;

        if (_timeSinceLastStart < _duration)
            return;

        IsOn = false;
        Door.UnregisterBlocker(this);

        if (_itemPedistal.DisplayItem == null)
            return;

        if (_itemPedistal.DisplayItem.name == Items.RAT_ID)
            _itemPedistal.Place(Items.Get(Items.COOKED_RAT_ID));
    }

    public void InsertItem(Item item)
    {
        _itemPedistal.Place(item);
    }

    public Item TakeOut()
    {
        if (ContainsItem == false)
            throw new Exception("Cannot take out an item from an empty microwave!");

        return _itemPedistal.Remove();
    }

    public void TurnOn()
    {
        if (IsOn == true)
            return;

        IsOn = true;
        _timeSinceLastStart = TimeSince.Now();
        Door.Close();
        Door.RegisterBlocker(this);
    }

    public override string GetBlockReason()
    {
        return "I shouldn't open it while it cooks";
    }

}
