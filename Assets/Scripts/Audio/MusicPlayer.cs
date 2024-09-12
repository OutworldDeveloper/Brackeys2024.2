using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{

    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioClip[] _music = new AudioClip[0];
    [SerializeField] private PlayerCharacter _playerCharacter;
    [SerializeField] private StormController _stormController;

    private bool _isNonMusicState;
    private bool _isPlayerDead;

    public bool ShouldPlay => !_isNonMusicState && !_isPlayerDead;

    private void Awake()
    {
        _stormController.StateChanged += OnStormStateChanged;
        _playerCharacter.Died += OnPlayerDied;
    }

    private void Update()
    {
        _musicSource.volume = Mathf.MoveTowards(_musicSource.volume, ShouldPlay ? 1 : 0, Time.deltaTime);
    }

    private void OnPlayerDied()
    {
        _isPlayerDead = true;
    }

    private void OnStormStateChanged()
    {
        _isNonMusicState = !_stormController.State.IsMusicAllowed;
    }

}
