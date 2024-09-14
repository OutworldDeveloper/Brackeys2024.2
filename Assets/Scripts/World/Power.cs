using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Power : MonoBehaviour
{

    public event Action Outage;
    public event Action Restored;

    [SerializeField] private AudioSource _outSource;
    [SerializeField] private Sound _outSound;
    [SerializeField] private Sound _inSound;

    private List<Lamp> _lamps = new List<Lamp>();
    private bool _shouldTurnOff = false;
    private bool _isOn = true;
    private TimeUntil _timeUntilTurnOff;

    public bool IsPowerOn => _isOn;

    public void TurnOff()
    {
        _shouldTurnOff = true;
        _timeUntilTurnOff = new TimeUntil(Time.time + 0.75f);
        _outSound.Play(_outSource);
        ScreenFade.FadeOutFor(1.5f);
    }

    public void TurnOn()
    {
        _shouldTurnOff = false;
        _isOn = true;
        foreach (var lamp in _lamps) lamp.TurnOn();
        _inSound.Play(_outSource);
        Restored?.Invoke();
    }

    private void Awake()
    {
        foreach (var lamp in FindObjectsOfType<Lamp>())
        {
            if (lamp.DisabledDuringBlackout)
                _lamps.Add(lamp);
        }
    }

    private void Update()
    {
        if (_isOn == false)
            return;

        if (_shouldTurnOff == false)
            return;

        if (_timeUntilTurnOff > 0f)
            return;

        foreach (var lamp in _lamps) lamp.TurnOff();
        _isOn = false;
        Outage?.Invoke();
    }

}
