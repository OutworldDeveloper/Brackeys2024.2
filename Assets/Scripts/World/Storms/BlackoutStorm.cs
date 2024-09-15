using System.Collections.Generic;
using UnityEngine;

public sealed class BlackoutStorm : Storm
{

    [SerializeField] private Power _lightController;

    protected override IEnumerable<StormState> CreateSequence()
    {
        yield return new DisablePower(_lightController);
        yield return new WaitForPower(_lightController);
    }

}

public sealed class WaitForPower : StormState
{

    private readonly Power _power;

    public WaitForPower(Power power)
    {
        _power = power;
    }

    public override bool Update(TimeSince sinceStart)
    {
        return _power.IsPowerOn;
    }

}

public sealed class DisablePower : StormState
{

    private readonly Power _power;

    public DisablePower(Power power)
    {
        _power = power;
    }

    public override bool Update(TimeSince timeSinceStart)
    {
        _power.TurnOff();
        return true;
    }

}

public sealed class EnablePower : StormState
{

    private readonly Power _power;

    public EnablePower(Power power)
    {
        _power = power;
    }

    public override bool Update(TimeSince timeSinceStart)
    {
        _power.TurnOn();
        return true;
    }

}
