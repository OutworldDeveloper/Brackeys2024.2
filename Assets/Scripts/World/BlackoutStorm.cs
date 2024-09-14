using UnityEngine;

public sealed class BlackoutStorm : Storm
{

    [SerializeField] private float _duration = 10f;
    [SerializeField] private Power _lightController;

    protected override void OnStormStarted()
    {
        _lightController.TurnOff();
    }

    public override bool UpdateStorm()
    {
        if (TimeSinceStarted < _duration)
            return false;

        _lightController.TurnOn();
        return true;
    }

}
