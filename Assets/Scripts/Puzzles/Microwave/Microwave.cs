using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Door))]
public class Microwave : DoorBlocker
{

    [SerializeField] private float _duration = 5f;
    [SerializeField] private ItemPedistal _itemPedistal;
    [SerializeField] private AudioSource _workSource;
    [SerializeField] private AudioSource _buttonSource;
    [SerializeField] private Sound _buttonSound;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Material _onMaterial;
    [SerializeField] private Power _lightController;

    private TimeSince _timeSinceLastStart = TimeSince.Never;
    private Material _offMaterial;

    public Door Door { get; private set; }
    public bool IsOn { get; private set; }
    public bool ContainsItem => _itemPedistal.DisplayItem != null;

    private void Awake()
    {
        Door = GetComponent<Door>();
        _offMaterial = _renderer.sharedMaterial;
        _lightController.Outage += OnPowerOutage;
        Door.RegisterBlocker(this);
    }

    private void Update()
    {
        if (IsOn == false)
            return;

        if (IsInWater() == true)
        {
            TurnOff();
            return;
        }

        if (_timeSinceLastStart < _duration)
            return;

        TurnOff();
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

        if (IsInWater() == true)
            return;

        if (_lightController.IsPowerOn == false)
        {
            Notification.Show("No power");
            return;
        }

        if (Door.IsOpen == true || Door.IsAnimating == true)
            return;

        IsOn = true;
        _timeSinceLastStart = TimeSince.Now();
        Door.Close();

        _workSource.Play();
        _renderer.sharedMaterial = _onMaterial;

        _buttonSound.Play(_buttonSource);

        _itemPedistal.transform.rotation = Quaternion.Euler(0f, Randomize.Float(0, 360), 0f);
    }

    public bool IsInWater()
    {
        return Water.Level > transform.position.y + 0.2f;
    }

    public void TurnOff()
    {
        IsOn = false;

        _workSource.Stop();
        _renderer.sharedMaterial = _offMaterial;

        if (_timeSinceLastStart < _duration)
            return;

        if (_itemPedistal.DisplayItem == null)
            return;

        if (_itemPedistal.DisplayItem.name == Items.RAT_ID)
            _itemPedistal.Place(Items.Get(Items.COOKED_RAT_ID));
    }

    public override string GetBlockReason()
    {
        return "I shouldn't open it while it works";
    }

    public override bool IsActive()
    {
        return IsOn;
    }

    private void OnPowerOutage()
    {
        TurnOff();
    }


}
