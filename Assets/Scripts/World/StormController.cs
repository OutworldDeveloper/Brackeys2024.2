using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class StormController : MonoBehaviour
{

    public event Action StateChanged;

    [SerializeField] private StormPicker _stormPicker;
    [SerializeField] private MinMax<float> _stormDelay = new(40, 60);
    [SerializeField] private Siren _siren;
    [SerializeField] private PlayerTrigger _startRoomTrigger;

    public IStormState State { get; private set; }
    public TimeSince TimeSinceLastStateStarted { get; private set; }

    private Queue<IStormState> _stateSequence = new Queue<IStormState>();

    private bool _isFirstSequence = true;

    private void Start()
    {
        StartNextState();
    }

    private void Update()
    {
        bool isFinished = State.Update(TimeSinceLastStateStarted);

        if (isFinished)
            StartNextState();
    }

    private void StartNextState()
    {
        if (_stateSequence.Count == 0)
        {
            _stateSequence = new Queue<IStormState>(GetNextSequence());
            Notification.ShowDebug($"New sequence: {_stateSequence.Count} elements");
        }

        if (State != null)
            State.OnFinished();

        State = _stateSequence.Dequeue();
        TimeSinceLastStateStarted = TimeSince.Now();
        State.OnStarted();

        StateChanged?.Invoke();
    }

    public IEnumerable<IStormState> GetNextSequence()
    {
        if (_isFirstSequence)
        {
            yield return new WaitForPlayerToMoveState(_startRoomTrigger);
            _isFirstSequence = false;
        }

        yield return new DelayState(Randomize.Float(_stormDelay), true);
        yield return new DelayState(4f, false);
        yield return new SirenState(_siren, 8f);
        yield return new DelayState(4f, false);
        yield return new StormState(_stormPicker.GetNextStorm());
        yield return new DelayState(6f, false);
    }

}

public abstract class IStormState
{
    public virtual bool IsMusicAllowed => false;
    public abstract bool Update(TimeSince timeSinceStart);
    public virtual void OnStarted() { }
    public virtual void OnFinished() { }

}

public sealed class WaitForPlayerToMoveState : IStormState
{

    private PlayerTrigger _trigger;
    public override bool IsMusicAllowed => true;

    public WaitForPlayerToMoveState(PlayerTrigger trigger)
    {
        _trigger = trigger;
    }

    public override bool Update(TimeSince timeSinceStart)
    {
        return timeSinceStart > 1.5f // Trigger delay fix
            && !_trigger.HasPlayerInside;
    }

}

public sealed class DelayState : IStormState
{
    public override bool IsMusicAllowed { get; }
    public float Duration { get; }

    public DelayState(float duration, bool isMusicAllowed)
    {
        Duration = duration;
        IsMusicAllowed = isMusicAllowed;
    }

    public override bool Update(TimeSince timeSinceStart)
    {
        return timeSinceStart > Duration;
    }

}

public sealed class StormState : IStormState
{
    public Storm Storm { get; }

    public StormState(Storm storm)
    {
        Storm = storm;
    }

    public override void OnStarted()
    {
        Storm.BeginStorm();
        Notification.ShowDebug($"Storm {Storm.GetType().Name} started!", 4f);
    }

    public override bool Update(TimeSince timeSinceStart)
    {
        return Storm.UpdateStorm();
    }

    public override void OnFinished()
    {
        Notification.ShowDebug($"Storm {Storm.GetType().Name} completed!", 4f);
    }

}

public sealed class SirenState : IStormState
{

    private readonly Siren _siren;
    public float Duration { get; }

    public SirenState(Siren siren, float duration)
    {
        _siren = siren;
        Duration = duration;
    }

    public override void OnStarted()
    {
        _siren.Play();
    }

    public override bool Update(TimeSince timeSinceStart)
    {
        return timeSinceStart > Duration;
    }

    public override void OnFinished()
    {
        _siren.Stop();
    }

}

public abstract class StormPicker : MonoBehaviour
{
    public abstract Storm GetNextStorm();

}

public abstract class Storm : MonoBehaviour
{
    protected TimeSince TimeSinceStarted { get; private set; }

    public void BeginStorm() 
    {
        TimeSinceStarted = new TimeSince(Time.time);
        OnStormStarted();
    }

    protected virtual void OnStormStarted() { }
    public abstract bool UpdateStorm();

}
