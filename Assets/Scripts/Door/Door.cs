using System;
using UnityEngine;
using Alchemy.Inspector;
using DG.Tweening;
using System.Collections.Generic;

[HideScriptField]
public sealed class Door : MonoBehaviour
{

    public event Action SomeoneKnocked;
    public event Action Opening;
    public event Action Opened;
    public event Action Closed;
    public event Action Closing;

    [SerializeField, TabGroup("Animation")] private float _animationDuration = 1f;
    [SerializeField, TabGroup("Animation")] private float _openDelay;
    [SerializeField, TabGroup("Animation")] private AnimationCurve _openCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField, TabGroup("Animation")] private float _animationDurationClose = 1f;
    [SerializeField, TabGroup("Animation")] private float _closeDelay;
    [SerializeField, TabGroup("Animation")] private AnimationCurve _closeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField, TabGroup("Animation")] private DoorPart[] _parts;
    [SerializeField, TabGroup("Animation")] private DoorAnimation[] _animations;

    [SerializeField, TabGroup("Audio")] private AudioSource _audioSource;
    [SerializeField, TabGroup("Audio")] private Sound _openingSound;
    [SerializeField, TabGroup("Audio")] private Sound _openSound;
    [SerializeField, TabGroup("Audio")] private Sound _closeSound;
    [SerializeField, TabGroup("Audio")] private Sound _knockSound;
    [SerializeField, TabGroup("Audio")] private Sound _lockedSound;

    [Range(0f, 1f)]
    [SerializeField, TabGroup("Collision")] private float _collisionSynchTime;
    [SerializeField, TabGroup("Collision")] private Collider _collision;

    [SerializeField, TabGroup("Extra")] private Item _keyItem;
    [SerializeField, TabGroup("Extra")] private DoorBlocker[] _initialBlockers;

    private bool _isOpen;
    private bool _isAnimating;
    private bool _isCollisionSynched;
    private TimeSince _timeSinceAnimationStarted;
    private int _blockedTimes;
    private bool _isLockedByKey; 
    private TimeSince _timeSinceLastKnocked = new TimeSince(float.NegativeInfinity);
    private readonly List<DoorBlocker> _blockers = new List<DoorBlocker>();

    public bool IsOpen => _isOpen;
    public bool IsLocked => _isLockedByKey == true;
    public bool IsBlocked => _blockedTimes > 0;
    public bool IsAnimating => _isAnimating;
    public bool IsOpening => _isAnimating == true && _isOpen == false;
    public bool IsClosing => _isAnimating == true && _isOpen == true;

    public Item Key => _keyItem;

    private void Start()
    {
        foreach (var blocker in _initialBlockers)
        {
            RegisterBlocker(blocker);
        }

        SetT(_isOpen ? 1f : 0f);
        SynchCollision();

        _isLockedByKey = Key != null;
    }

    private void Update()
    {
        if (_isAnimating == false)
            return;

        float animationDuration = IsOpening ? _animationDuration : _animationDurationClose;
        float delay = IsOpening ? _openDelay : _closeDelay;

        if (_timeSinceAnimationStarted < delay)
            return;

        if (_timeSinceAnimationStarted > delay + animationDuration)
        {
            _isAnimating = false;
            _isOpen = !_isOpen;
            OnAnimationFinished();
        }
        else
        {
            float t = (_timeSinceAnimationStarted - delay) / animationDuration;
            OnAnimating(t);

            if (_isCollisionSynched == false && t > _collisionSynchTime)
            {
                _isCollisionSynched = true;
                SynchCollision();
            }
        }
    }

    public void Open()
    {
        if (_isAnimating == true)
            return;

        if (_isOpen == true)
            return;

        _timeSinceAnimationStarted = new TimeSince(Time.time);
        _isAnimating = true;
        _isCollisionSynched = false;

        OnAnimationStarted();

        Opening?.Invoke();

        AnimationEvent(DoorEvent.BeginOpening);

        _openingSound.Play(_audioSource);
    }

    public void Close()
    {
        if (_isAnimating == true)
            return;

        if (_isOpen == false)
            return;

        _timeSinceAnimationStarted = new TimeSince(Time.time);
        _isAnimating = true;
        _isCollisionSynched = false;

        OnAnimationStarted();

        Closing?.Invoke();
    }

    public bool TryUnlock(Item item)
    {
        if (item != _keyItem)
            return false;

        _isLockedByKey = false;
        return true;
    }

    public bool TryOpen()
    {
        if (_isAnimating == true)
            return false;

        if (_isOpen == true)
            return false;

        if (IsLocked == true)
        {
            _lockedSound.Play(_audioSource);
            Notification.Show($"Locked!");
            AnimationEvent(DoorEvent.FailedOpenAttempt);
            return false;
        }

        if (IsBlocked == true)
        {
            _lockedSound.Play(_audioSource);
            Notification.Show($"Locked!");
            AnimationEvent(DoorEvent.FailedOpenAttempt);
            return false;
        }

        foreach (var blocker in _blockers)
        {
            if (blocker.IsActive() == false)
                continue;

            Sound blockedSound = _lockedSound;

            if (blocker.HasCustomSound() == true)
                blockedSound = blocker.GetCustomSound();

            blockedSound.Play(_audioSource);
            Notification.Show(blocker.GetBlockReason());
            blocker.OnBlockedOpening();
            AnimationEvent(DoorEvent.FailedOpenAttempt);
            return false;
        }

        Open();
        return true;
    }

    public void Knock()
    {
        if (_timeSinceLastKnocked < 1f)
            return;

        _timeSinceLastKnocked = new TimeSince(Time.time);

        SomeoneKnocked?.Invoke();
        _knockSound.Play(_audioSource);
    }

    public void RegisterBlocker(DoorBlocker blocker)
    {
        _blockers.Add(blocker);
    }

    public void UnregisterBlocker(DoorBlocker blocker)
    {
        _blockers.Remove(blocker);
    }

    public void Block() // IBlocker that will provide blocked sound and reason
    {
        _blockedTimes++;
    }

    public void Unblock()
    {
        _blockedTimes--;
    }

    private void OnAnimating(float t)
    {
        AnimationCurve curve = IsOpening ? _openCurve : _closeCurve;
        t = curve.Evaluate(t);

        if (IsClosing == true)
            t = 1 - t;

        SetT(t);
    }

    private void OnAnimationStarted()
    {
        if (IsClosing == true)
        {
            Closing?.Invoke();
            AnimationEvent(DoorEvent.BeginClosing);
        }
        else
        {
            Opening?.Invoke();
            _openSound.Play(_audioSource);
            AnimationEvent(DoorEvent.BeginOpening);
        }
    }

    private void OnAnimationFinished()
    {
        if (_isOpen == true)
        {
            Opened?.Invoke();
            AnimationEvent(DoorEvent.Opened);
        }
        else
        {
            Closed?.Invoke();
            _closeSound.Play(_audioSource);
            AnimationEvent(DoorEvent.Closed);
        }
    }

    private void SetT(float t)
    {
        foreach (var part in _parts)
        {
            part.Set(t);
        }
    }

    private void AnimationEvent(DoorEvent e)
    {
        foreach (var animation in _animations)
        {
            animation.OnEvent(e);
        }
    }

    private void SynchCollision()
    {
        if (_collision == null)
            return;

        _collision.enabled = IsAnimating ? IsClosing : _isOpen;
    }

}

public abstract class DoorPart : MonoBehaviour
{
    public abstract void Set(float t);

}

public abstract class DoorAnimation : MonoBehaviour
{
    public abstract void OnEvent(DoorEvent e);

}

public enum DoorEvent
{
    FailedOpenAttempt,
    BeginOpening,
    Opened,
    BeginClosing,
    Closed,
}


public abstract class DoorBlocker : MonoBehaviour
{
    public virtual bool IsActive() => true;
    public virtual string GetBlockReason() => $"Locked!";
    public virtual bool HasCustomSound() => false;
    public virtual Sound GetCustomSound() => null;
    public virtual void OnBlockedOpening() { }

}
