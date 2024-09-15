using System.Collections.Generic;
using UnityEngine;

public sealed class WaterStorm : Storm
{

    [SerializeField] private float _maxWaterLevel = 3f;
    [SerializeField] private float _raiseDuration = 10f;
    [SerializeField] private float _holdDuration = 5f;
    [SerializeField] private float _returnDuration = 5f;
    [SerializeField] private Power _ligths;
    [SerializeField] private bool _disableLights;

    protected override IEnumerable<StormState> CreateSequence()
    {
        yield return new LerpWaterState(Water.BaseLevel, _maxWaterLevel, _raiseDuration);
        yield return new WaitState(_holdDuration);
        yield return new LerpWaterState(_maxWaterLevel, Water.BaseLevel, _returnDuration);
    }

}

public sealed class LerpWaterState : StormState
{

    private readonly float _heightStart;
    private readonly float _heightEnd;
    private readonly float _duration;

    public LerpWaterState(float heightStart, float heightEnd, float duration)
    {
        _heightStart = heightStart;
        _heightEnd = heightEnd;
        _duration = duration;
    }

    public override bool Update(TimeSince sinceStart)
    {
        float t = sinceStart / _duration;
        float waterLevel = Mathf.Lerp(_heightStart, _heightEnd, t);
        Water.SetLevel(waterLevel);
        return sinceStart > _duration;
    }

}
