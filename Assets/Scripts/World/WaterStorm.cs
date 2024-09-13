using UnityEngine;

public sealed class WaterStorm : Storm
{

    [SerializeField] private float _maxWaterLevel = 3f;
    [SerializeField] private float _raiseDuration = 10f;
    [SerializeField] private float _holdDuration = 5f;
    [SerializeField] private float _returnDuration = 5f;
    [SerializeField] private LightsController _ligths;
    [SerializeField] private bool _disableLights;

    private State _currentState;

    protected override void OnStormStarted()
    {
        _currentState = State.Raise;

        if (_disableLights)
            _ligths.TurnOff();
    }

    public override bool UpdateStorm()
    {
        switch (_currentState)
        {
            case State.Raise:
                UpdateRaise();
                return false;

            case State.Hold:
                UpdateHold();
                return false;

            case State.Return:
                UpdateReturn();
                return false;

            default:
                if (_disableLights)
                    _ligths.TurnOn();
                return true;
        }
    }

    private void UpdateRaise()
    {
        float t = TimeSinceStarted / _raiseDuration;
        float waterLevel = Mathf.Lerp(Water.BaseLevel, _maxWaterLevel, t);
        Water.SetLevel(waterLevel);

        if (TimeSinceStarted > _raiseDuration)
        {
            _currentState = State.Hold;
            Notification.ShowDebug("Raise complete, holding");
        }
    }

    private void UpdateHold()
    {
        if (TimeSinceStarted > _raiseDuration + _holdDuration)
        {
            _currentState = State.Return;
            Notification.ShowDebug("Hold complete, returning");
        }
    }

    private void UpdateReturn()
    {
        float t = (TimeSinceStarted - _raiseDuration - _holdDuration) / _raiseDuration;
        float waterLevel = Mathf.Lerp(_maxWaterLevel, Water.BaseLevel, t);
        Water.SetLevel(waterLevel);

        if (TimeSinceStarted > _raiseDuration + _holdDuration + _returnDuration)
        {
            _currentState = State.Finish;
            Notification.ShowDebug("Return complete, storm finished");
        }
    }

    private enum State
    {
        Raise,
        Hold,
        Return,
        Finish,
    }

}
