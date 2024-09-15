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

    [SerializeField] private Light _light;

    public bool IsPlayingSequence { get; private set; }
    public bool IsOn { get; private set; }
    public bool ContainsCassete { get; private set; }

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
        TurnOff();
    }

    private void Update()
    {
        if (IsOn == true && IsInWater() == true)
            TurnOff();

        float desiredVolume = _desiredNoiseVolume;

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

    public void InsertCassete()
    {
        ContainsCassete = true;
    }

    public void PlaySequence(bool playSound = true)
    {
        if (IsOn == false)
            return;

        if (ContainsCassete == false)
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
            PlaySequence(false);
        });

        if (playSound)
            _startSound.Play(_startAudioSource);
    }

    public void TurnOn()
    {
        if (IsInWater() == true)
            return;

        if (_power.IsPowerOn == false)
            return;

        if (IsOn == true)
            return;

        IsOn = true;

        _startSound.Play(_startAudioSource);

        GetComponent<MeshRenderer>().sharedMaterial = _material;
        _staticNoiseSource.volume = 0f;

        _light.enabled = true;

        PlaySequence();
    }

    public void TurnOff()
    {
        IsOn = false;
        IsPlayingSequence = false;
        GetComponent<MeshRenderer>().sharedMaterial = _offMaterial;
        _currentSequence.Kill();
        _light.enabled = false;
        _desiredNoiseVolume = 0f;
        _staticNoiseSource.volume = 0f;
    }

    public bool IsInWater()
    {
        return Water.Level > transform.position.y + 0.2f;
    }

    private void OnPowerOutage()
    {
        TurnOff();
    }

    private void OnPowerRestored() { }

    private Sequence Show(Texture2D texture) =>
        DOTween.Sequence(gameObject).
            AppendCallback(() => _material.SetTexture(_screenTextureID, texture)).
            Append(_material.DOFloat(_minNoiseValue, _noiseMultiplierID, _transitionDuration)).
            Join(DOTween.To(() => _desiredNoiseVolume, value => _desiredNoiseVolume = value, _minNoiseValue, _transitionDuration)).
            AppendInterval(_displayTime).
            Append(_material.DOFloat(1f, _noiseMultiplierID, _transitionDuration)).
            Join(DOTween.To(() => _desiredNoiseVolume, value => _desiredNoiseVolume = value, 1f, _transitionDuration));

}
