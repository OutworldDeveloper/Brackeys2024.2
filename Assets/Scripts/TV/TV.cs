using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public sealed class TV : MonoBehaviour
{

    private readonly int _screenTextureID = Shader.PropertyToID("_ScreenTex");
    private readonly int _noiseMultiplierID = Shader.PropertyToID("_NoiseMultiplier");

    [SerializeField] private Material _material;
    [SerializeField] private Material _offMaterial;
    [SerializeField] private AudioSource _staticNoiseSource;
    [SerializeField] private float _delay = 0.5f;
    [SerializeField] private float _displayTime = 1f;
    [SerializeField] private float _transitionDuration = 1f;
    [SerializeField] private float _minNoiseValue = 0.6f;
    [SerializeField] private PlayerTrigger _roomTrigger;
    [SerializeField] private Sound _startSound;
    [SerializeField] private AudioSource _startAudioSource;

    [SerializeField] private Texture2D[] _textureSequence = new Texture2D[0];
    [SerializeField] private bool _ignoreRoomTrigger;
    [SerializeField] private Power _power;

    [SerializeField] private bool _autoPowerOn;

    [SerializeField] private Light _light;

    public bool IsPlayingSequence { get; private set; }
    public bool IsOn { get; private set; }

    private float _desiredNoiseVolume = 0f;
    private Sequence _currentSequence;

    private void Awake()
    {
        _power.Outage += OnPowerOutage;
        _power.Restored += OnPowerRestored;
    }

    private void Start()
    {
        _material.SetFloat(_noiseMultiplierID, 1f);
        _desiredNoiseVolume = 1f;

        if (_autoPowerOn)
            TurnOn();
        else
            TurnOff();
    }

    private void Update()
    {
        float desiredVolume = _roomTrigger.PlayerInside == true ? _desiredNoiseVolume : 0f;
        desiredVolume = _ignoreRoomTrigger == true ? _desiredNoiseVolume : desiredVolume;

        if (IsOn == false)
            desiredVolume = 0f;

        if (_staticNoiseSource.volume < desiredVolume)
        {
            _staticNoiseSource.volume += Time.deltaTime;
        }
        else
        {
            _staticNoiseSource.volume -= Time.deltaTime;
        }
    }

    public void StartSequence()
    {
        if (IsOn == false)
            return;

        if (IsPlayingSequence == true)
            return;

        IsPlayingSequence = true;

        var sequence = DOTween.Sequence();

        foreach (var texture in _textureSequence)
        {
            sequence.Append(Show(texture));
            sequence.AppendInterval(_delay);
        }

        sequence.OnComplete(() => 
        {
            IsPlayingSequence = false;
            StartSequence();
        });
    }

    public void TurnOn()
    {
        if (_power.IsPowerOn == false)
        {
            Notification.Show("No power");
            return;
        }

        if (IsOn == true)
            return;

        IsOn = true;

        _startSound.Play(_startAudioSource);

        GetComponent<MeshRenderer>().sharedMaterial = _material;
        _staticNoiseSource.volume = 0f;

        _light.enabled = true;

        StartSequence();
    }

    public void TurnOff()
    {
        IsOn = false;
        IsPlayingSequence = false;
        GetComponent<MeshRenderer>().sharedMaterial = _offMaterial;
        _currentSequence.Kill();
        _light.enabled = false;
    }

    private void OnPowerOutage()
    {
        TurnOff();
    }

    private void OnPowerRestored()
    {
        if (_autoPowerOn)
            TurnOn();
    }

    private Sequence Show(Texture2D texture) =>
        DOTween.Sequence(gameObject).
            AppendCallback(() => _material.SetTexture(_screenTextureID, texture)).
            Append(_material.DOFloat(_minNoiseValue, _noiseMultiplierID, _transitionDuration)).
            Join(DOTween.To(() => _desiredNoiseVolume, value => _desiredNoiseVolume = value, _minNoiseValue, _transitionDuration)).
            AppendInterval(_displayTime).
            Append(_material.DOFloat(1f, _noiseMultiplierID, _transitionDuration)).
            Join(DOTween.To(() => _desiredNoiseVolume, value => _desiredNoiseVolume = value, 1f, _transitionDuration));

}
