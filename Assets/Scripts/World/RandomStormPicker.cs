using System;
using UnityEngine;

public sealed class RandomStormPicker : StormPicker
{

    [SerializeField] private Storm[] _storms = Array.Empty<Storm>();

    public override Storm GetNextStorm()
    {
        return _storms[Randomize.Index(_storms.Length)];
    }

}
