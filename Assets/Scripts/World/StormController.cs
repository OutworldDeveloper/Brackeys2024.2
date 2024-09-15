using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class StormController : MonoBehaviour
{

    public event Action StateChanged;

    [SerializeField] private StormPicker _stormPicker;
    [SerializeField] private MinMax<float> _firstStormDelay = new(40, 60);
    [SerializeField] private MinMax<float> _stormDelay = new(40, 60);
    [SerializeField] private Siren _siren;
    [SerializeField] private PlayerTrigger _startRoomTrigger;

    public IGameState State { get; private set; }
    public TimeSince TimeSinceLastStateStarted { get; private set; }

    private Queue<IGameState> _stateSequence = new Queue<IGameState>();

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
            _stateSequence = new Queue<IGameState>(GetNextSequence());
        }

        if (State != null)
            State.OnFinished();

        State = _stateSequence.Dequeue();
        TimeSinceLastStateStarted = TimeSince.Now();
        State.OnStarted();

        StateChanged?.Invoke();
    }

    public IEnumerable<IGameState> GetNextSequence()
    {
        if (_isFirstSequence)
        {
            yield return new WaitForPlayerToMoveState(_startRoomTrigger);
            _isFirstSequence = false;
        }

        float delay = Randomize.Float(_isFirstSequence ? _firstStormDelay : _stormDelay);
        yield return new DelayState(delay, true);
        yield return new DelayState(4f, false);
        yield return new SirenState(_siren, 8f);
        yield return new DelayState(4f, false);
        yield return new StormingState(_stormPicker.GetNextStorm());
        yield return new DelayState(6f, false);
    }

}

public abstract class IGameState
{
    public virtual bool IsMusicAllowed => false;
    public abstract bool Update(TimeSince timeSinceStart);
    public virtual void OnStarted() { }
    public virtual void OnFinished() { }

}

public sealed class WaitForPlayerToMoveState : IGameState
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

public sealed class DelayState : IGameState
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

public sealed class StormingState : IGameState
{
    public Storm Storm { get; }

    public StormingState(Storm storm)
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

public sealed class SirenState : IGameState
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
    protected TimeSince SinceStormStarted { get; private set; }
    protected TimeSince SinceStateStarted { get; private set; }

    private Queue<StormState> _sequence;
    private StormState _currentState;

    public void BeginStorm() 
    {
        SinceStormStarted = new TimeSince(Time.time);
        _sequence = new Queue<StormState>(CreateSequence());
    }

    public bool UpdateStorm()
    {
        if (_currentState == null)
        {
            if (_sequence.Count == 0)
                return true;

            _currentState = _sequence.Dequeue();
            SinceStateStarted = TimeSince.Now();
            _currentState.OnStarted();
        }

        bool isFinished = _currentState.Update(SinceStateStarted);

        if (isFinished == true)
        {
            _currentState.OnFinished();
            _currentState = null;
        }

        return false;
    }

    protected abstract IEnumerable<StormState> CreateSequence();

}

public abstract class StormState
{
    public virtual bool KillLights => false;
    public abstract bool Update(TimeSince sinceStart);
    public virtual void OnStarted() { }
    public virtual void OnFinished() { }

}

public sealed class WaitState : StormState
{

    private readonly float _duration;

    public WaitState(float duration)
    {
        _duration = duration;
    }

    public override bool Update(TimeSince sinceStart)
    {
        return sinceStart > _duration;
    }

}

public sealed class ActionState : StormState
{

    private readonly Action _action;

    public ActionState(Action action)
    {
        _action = action;
    }

    public override bool Update(TimeSince sinceStart)
    {
        _action();
        return true;
    }

}
