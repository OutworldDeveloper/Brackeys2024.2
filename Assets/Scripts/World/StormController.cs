using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class StormController : MonoBehaviour
{

    [SerializeField] private Storm[] _storms;
    [SerializeField] private MinMax<float> _firstStormDelay = new(10, 10);
    [SerializeField] private MinMax<float> _stormDelay = new(40, 60);

    public TimeUntil _timeUntilNextStorm;

    private Storm _activeStorm;

    private void Start()
    {
        _timeUntilNextStorm = new TimeUntil(Time.time + Randomize.Float(_firstStormDelay));
    }

    private void Update()
    {
        if (_activeStorm != null)
        {
            if (_activeStorm.UpdateStorm())
            {
                Notification.ShowDebug($"Storm {_activeStorm.GetType().Name} completed!", 4f);
                _activeStorm = null;
                _timeUntilNextStorm = new TimeUntil(Time.time + Randomize.Float(_stormDelay));
            }

            return;
        }

        if (_timeUntilNextStorm > 0)
            return;

        _activeStorm = _storms[Randomize.Index(_storms.Length)];
        _activeStorm.BeginStorm();
        Notification.ShowDebug($"Storm {_activeStorm.GetType().Name} started!", 4f);
    }

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
