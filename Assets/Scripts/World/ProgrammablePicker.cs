using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ProgrammablePicker : StormPicker
{

    [SerializeField] private WaterStorm _lightWaterStorm;
    [SerializeField] private WaterStorm _waterStorm;
    [SerializeField] private StatueStorm _statueStorm;
    [SerializeField] private BlackoutStorm _blackoutStorm;
    [SerializeField] private EbakaStorm _ebakaStorm;

    private IStormPicker _picker;

    [ContextMenu("Test")]
    private void Test()
    {
        var picker = CreatePicker();

        for (int i = 0; i < 15; i++)
        {
            Debug.Log(picker.GetNextStorm());
        }
    }

    private void Awake()
    {
        _picker = CreatePicker();
    }

    public override Storm GetNextStorm()
    {
        return _picker.GetNextStorm();
    }

    private IStormPicker CreatePicker()
    {
        return new SequencePicker(CreateInitialSequence(), CreateMainLoopPicker());
    }

    private IEnumerable<IStormPicker> CreateInitialSequence()
    {
        yield return new SinglePicker(_lightWaterStorm);
        yield return new SinglePicker(_blackoutStorm);
    }

    private IStormPicker CreateMainLoopPicker()
    {
        var weightedPicker = new WeightedRandomPicker();
        weightedPicker.Append(new SinglePicker(_waterStorm), 1f);
        weightedPicker.Append(CreateDarknessStormPicker(), 2f);
        return weightedPicker;
    }

    private IStormPicker CreateWaterStormPicker()
    {
        var weightedPicker = new WeightedRandomPicker();
        weightedPicker.Append(new SinglePicker(_waterStorm), 1.4f);
        weightedPicker.Append(new SinglePicker(_lightWaterStorm), 1f);
        return weightedPicker;
    }

    private IStormPicker CreateDarknessStormPicker()
    {
        return new WeightedRandomPicker()
            .Append(new SinglePicker(_blackoutStorm), 0.6f)
            .Append(new SinglePicker(_ebakaStorm), 1.5f)
            .Append(new SinglePicker(_statueStorm), 0.2f);
    }

}

public interface IStormPicker
{
    public Storm GetNextStorm();

}

public sealed class SinglePicker : IStormPicker
{

    private readonly Storm _storm;

    public SinglePicker(Storm storm)
    {
        _storm = storm;
    }

    public Storm GetNextStorm() => _storm;

}

public sealed class SequencePicker : IStormPicker
{

    private readonly Queue<IStormPicker> _sequence;
    private readonly IStormPicker _fallback;

    public SequencePicker(IEnumerable<IStormPicker> sequence, IStormPicker fallback)
    {
        _sequence = new Queue<IStormPicker>(sequence);
        _fallback = fallback;
    }

    public Storm GetNextStorm()
    {
        if (_sequence.Count == 0)
            return _fallback.GetNextStorm();

        return _sequence.Dequeue().GetNextStorm();
    }

}

public sealed class RandomNoRepeatsPicker : IStormPicker
{

    private readonly IStormPicker[] _options;
    private int _previousPick = -1;

    public RandomNoRepeatsPicker(IStormPicker a, IStormPicker b, params IStormPicker[] others)
    {
        _options = new IStormPicker[others.Length + 2];
        _options[0] = a;
        _options[1] = b;
        Array.Copy(others, 0, _options, 2, others.Length);
    }

    public Storm GetNextStorm()
    {
        int nextPick;
        do
            nextPick = Randomize.Index(_options.Length);
        while (nextPick == _previousPick);

        return _options[nextPick].GetNextStorm();
    }

}

public sealed class RandomPicker : IStormPicker
{

    private readonly IStormPicker[] _options;

    public RandomPicker(IStormPicker a, params IStormPicker[] others)
    {
        _options = new IStormPicker[others.Length + 1];
        _options[0] = a;
        Array.Copy(others, 0, _options, 1, others.Length);
    }

    public Storm GetNextStorm()
    {
        return _options[Randomize.Index(_options.Length)].GetNextStorm();
    }

}

public sealed class WeightedRandomPicker : IStormPicker
{

    private readonly WeightedRandom<IStormPicker> _options = new WeightedRandom<IStormPicker>(10);

    public WeightedRandomPicker Append(IStormPicker picker, float weight)
    {
        _options.Add(picker, weight);
        return this;
    }

    public Storm GetNextStorm()
    {
        return _options.GetRandomItem().GetNextStorm();
    }

}