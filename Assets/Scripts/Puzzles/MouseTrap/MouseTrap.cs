using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class MouseTrap : MonoBehaviour
{

    public event Action RatTaken;

    public bool HasCheese { get; private set; }
    public bool HasMouse { get; private set; }
    public bool IsDisabled { get; private set; }

    public void PlaceCheese()
    {
        HasCheese = true;
    }

    public void SpawnMouse()
    {
        Notification.ShowDebug("Rat spawned!");

        HasMouse = true;
        HasCheese = false;
    }

    public Item TakeRat()
    {
        HasMouse = false;
        RatTaken?.Invoke();
        return Items.Get(Items.RAT_ID);
    }

    public void Disable()
    {
        IsDisabled = true;
    }

}
