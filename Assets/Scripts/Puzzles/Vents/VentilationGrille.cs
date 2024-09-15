using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class VentilationGrille : MonoBehaviour
{

    [SerializeField] private GameObject _closedState;
    [SerializeField] private GameObject _openState;
    [SerializeField] private AudioSource _openAudio;

    public bool IsOpen { get; private set; } = false;

    private void Start()
    {
        _closedState.SetActive(true);
        _openState.SetActive(false);
    }

    public void Open()
    {
        if (IsOpen)
            return;

        _openAudio.Play();
        IsOpen = true;
        _closedState.SetActive(false);
        _openState.SetActive(true);
    }

}
