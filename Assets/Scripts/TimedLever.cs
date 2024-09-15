using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System;

public sealed class TimedLever : MonoBehaviour
{

    public event Action Activated;

    [SerializeField] private Transform _rotator;
    [SerializeField] private Sound _activatedSound;
    [SerializeField] private Sound _deactivatedSound;
    [SerializeField] private AudioSource _audioSource;

    public bool IsActivated { get; private set; } = false;

    private Sequence _activeSequence;

    public void Activate(bool makeSound)
    {
        if (IsActivated == true)
            return;

        IsActivated = true;

        _activeSequence.Kill(true);
        _activeSequence = DOTween.Sequence(gameObject).
            Append(_rotator.DOLocalRotate(new Vector3(150f, 0f, 0f), 0.7f));

        if (makeSound)
            _activatedSound.Play(_audioSource);

        Activated?.Invoke();
    }

    public void Deactivate()
    {
        if (IsActivated == false)
            return;

        IsActivated = false;
        _deactivatedSound.Play(_audioSource);

        _activeSequence = DOTween.Sequence(gameObject).
            Append(_rotator.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.4f));
    }

}
