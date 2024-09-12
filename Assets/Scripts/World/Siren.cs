using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Siren : MonoBehaviour
{

    [SerializeField] private Sound _sound;

    private AudioSource[] _sources;
    private bool _shouldPlay;

    private TimeSince _timeSinceLastPlay = TimeSince.Never;

    private void Awake()
    {
        _sources = GetComponentsInChildren<AudioSource>();
    }

    private void Update()
    {
        if (_shouldPlay == false || _timeSinceLastPlay < 3f)
            return;

        _timeSinceLastPlay = TimeSince.Now();
        PlayOnce();
    }

    public void Play()
    {
        _shouldPlay = true;
    }

    public void Stop()
    {
        _shouldPlay = false;
    }

    [ContextMenu("Play Once")]
    public void PlayOnce()
    {
        foreach (var source in _sources)
        {
            _sound.Play(source);
        }
    }

}
