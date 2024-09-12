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
    public ContentState Content { get; private set; }
    public bool IsOn { get; private set; }

    public bool ContainsItem => Content switch
    {
        ContentState.Rat => true,
        ContentState.CookedRat => true,
        _ => false,
    };

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

        if (Content == ContentState.Rat)
            Content = ContentState.CookedRat;
    }

    public void InsertRat()
    {
        Content = ContentState.Rat;
    }

    public Item TakeOut()
    {
        switch (Content)
        {
            case ContentState.Rat:
                Content = ContentState.Empty;
                return Items.Get(Items.RAT_ID);
            case ContentState.CookedRat:
                Content = ContentState.Empty;
                return Items.Get(Items.COOKED_RAT_ID);
        }

        throw new Exception("Cannot take out an item from an empty microwave!");
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

    public enum ContentState
    {
        Empty,
        Rat,
        CookedRat,
    }

}
