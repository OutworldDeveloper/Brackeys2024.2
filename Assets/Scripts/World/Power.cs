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

    [SerializeField] private GameObject _reflectionBoxesOn;
    [SerializeField] private GameObject _reflectionBoxesOff;

    [SerializeField] private TimedLever[] _powerLevers;

    private List<Lamp> _lamps = new List<Lamp>();
    private bool _isLightOn = true;
    private TimeUntil _timeUntilTurnOff;

    public bool IsPowerOn { get; private set; } = true;

    public void TurnOff()
    {
        IsPowerOn = false;
        _timeUntilTurnOff = new TimeUntil(Time.time + 0.75f);
        _outSound.Play(_outSource);
        ScreenFade.FadeOutFor(1.5f);

        foreach (var lever in _powerLevers)
        {
            lever.Deactivate();
        }

        if (Player.IsFirstTime)
            Delayed.Do(() => Notification.Show("I need to restore power", 3f), 1f);
    }

    [ContextMenu("Turn on")]
    public void TurnOn()
    {
        IsPowerOn = true;
        _isLightOn = true;
        UpdateLighting();
        _inSound.Play(_outSource);
        Restored?.Invoke();

        foreach (var lever in _powerLevers)
        {
            lever.Activate(true);
        }
    }

    private void Awake()
    {
        foreach (var lamp in FindObjectsOfType<Lamp>())
        {
            if (lamp.Ignore == false)
                _lamps.Add(lamp);
        }

        foreach (var lever in _powerLevers)
        {
            lever.Activate(false);
            lever.Activated += OnLeverActivated;
        }
    }

    private void Start()
    {
        UpdateLighting();
    }

    private void OnLeverActivated()
    {
        if (IsPowerOn == true)
            return;

        foreach (var lever in _powerLevers)
        {
            if (lever.IsActivated == false)
                return;
        }

        TurnOn();
    }

    private void Update()
    {
        if (_isLightOn == false)
            return;

        if (IsPowerOn == true)
            return;

        if (_timeUntilTurnOff > 0f)
            return;

        _isLightOn = false;
        UpdateLighting();
        Outage?.Invoke();
    }

    private void UpdateLighting()
    {
        _reflectionBoxesOn.SetActive(_isLightOn);
        _reflectionBoxesOff.SetActive(!_isLightOn);

        foreach (var lamp in _lamps)
        {
            bool shouldEnable = _isLightOn ?
                !lamp.IsInverted : lamp.IsInverted;

            if (shouldEnable)
                lamp.TurnOn();
            else
                lamp.TurnOff();
        }
    }

}
