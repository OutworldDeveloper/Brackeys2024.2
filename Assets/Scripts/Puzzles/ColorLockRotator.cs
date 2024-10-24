﻿using DG.Tweening;
using UnityEngine;

public sealed class ColorLockRotator : MonoBehaviour
{

    [SerializeField] private int _requiredDigit;
    [SerializeField] private int _digitsCount = 5;
    [SerializeField] private AudioSource _rotationAudio;

    public int CurrentDigit { get; private set; }
    public bool IsRightDigit => CurrentDigit == _requiredDigit;

    private void Start()
    {
        OnDigitUpdated(false);
    }

    public void RotateUp()
    {
        CurrentDigit--;

        if (CurrentDigit < 0)
            CurrentDigit = _digitsCount;

        OnDigitUpdated(true);
    }

    public void RotateDown()
    {
        CurrentDigit++;

        if (CurrentDigit > _digitsCount)
            CurrentDigit = 0;

        OnDigitUpdated(true);
    }

    public void SetDigit(int digit)
    {
        CurrentDigit = digit;
        OnDigitUpdated(false);
    }

    public void SetMaterial(Material material)
    {
        GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    private void OnDigitUpdated(bool animate)
    {
        float targetAngle = CurrentDigit * 60f;
        Vector3 targetEuler = new Vector3(targetAngle, 0f, 0f);

        if (animate == true)
            transform.DOLocalRotate(targetEuler, 0.2f);
        else
            transform.localEulerAngles = targetEuler;

        if (animate)
            _rotationAudio.Play();
    }

}
