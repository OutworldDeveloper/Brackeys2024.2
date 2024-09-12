using UnityEngine;

public sealed class SingleStormPicker : StormPicker
{

    [SerializeField] private Storm _storm;

    public override Storm GetNextStorm()
    {
        return _storm;
    }

}
