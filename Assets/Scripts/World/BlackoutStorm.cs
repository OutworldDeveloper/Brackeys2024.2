using UnityEngine;

public sealed class BlackoutStorm : Storm
{

    [SerializeField] private float _duration = 10f;
    [SerializeField] private Light[] _lights;

    protected override void OnStormStarted()
    {
        SetLightsActive(false);
    }

    public override bool UpdateStorm()
    {
        if (TimeSinceStarted < _duration)
            return false;

        SetLightsActive(true);
        return true;
    }

    private void SetLightsActive(bool active)
    {
        foreach (var light in _lights)
        {
            light.enabled = active;
        }
    }

}
