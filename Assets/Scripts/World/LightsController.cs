using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightsController : MonoBehaviour
{

    [SerializeField] private Lamp[] _lamps = new Lamp[0];
    [SerializeField] private AudioSource _outSource;
    [SerializeField] private Sound _outSound;

    public void TurnOff()
    {
        foreach (var lamp in _lamps) lamp.TurnOff();
        _outSound.Play(_outSource);
        ScreenFade.FadeOutFor(1.5f);
    }

    public void TurnOn()
    {
        foreach (var lamp in _lamps) lamp.TurnOn();
    }

}
