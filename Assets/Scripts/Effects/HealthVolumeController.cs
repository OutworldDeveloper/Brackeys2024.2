using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public sealed class HealthVolumeController : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _character;

    private Volume _volume;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
    }

    private void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        _character.Damaged += Refresh;
    }

    private void OnDisable()
    {
        _character.Damaged -= Refresh;
    }

    private void Refresh()
    {
        _volume.weight = 1 - _character.Health / _character.MaxHealth;
    }

}
