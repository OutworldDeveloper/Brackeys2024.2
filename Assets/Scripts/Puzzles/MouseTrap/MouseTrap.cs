using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class MouseTrap : MonoBehaviour
{

    public event Action RatTaken;

    [SerializeField] private GameObject _defaultModel;
    [SerializeField] private GameObject _triggeredModel;
    [SerializeField] private GameObject _breadModel;

    public bool HasBait { get; private set; }
    public bool HasMouse { get; private set; }
    public bool IsDisabled { get; private set; }

    private void Start()
    {
        UpdateModel();
    }

    public void PlaceBait()
    {
        HasBait = true;
        UpdateModel();
    }

    public void SpawnMouse()
    {
        Notification.ShowDebug("Rat spawned!");
        HasMouse = true;
        HasBait = false;
        UpdateModel();
    }

    public Item TakeRat()
    {
        HasMouse = false;
        UpdateModel();
        RatTaken?.Invoke();
        return Items.Get(Items.RAT_ID);
    }

    public void Disable()
    {
        IsDisabled = true;
    }

    private void UpdateModel()
    {
        _defaultModel.SetActive(!HasMouse);
        _triggeredModel.SetActive(HasMouse);
        _breadModel.SetActive(HasBait);
    }

}
