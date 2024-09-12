using System;
using System.Collections.Generic;
using UnityEngine;
// Split into sequence storm picker and variants picker?
public sealed class StoryStormPicker : StormPicker
{

    [SerializeField] private List<Storm> _introStorms;
    [SerializeField] private StormVariants[] _randomStorms;

    public override Storm GetNextStorm()
    {
        if (_introStorms.Count > 0)
        {
            Storm storm = _introStorms[0];
            _introStorms.RemoveAt(0);
            return storm;
        }

        return _randomStorms[Randomize.Index(_randomStorms.Length)].Pick();
    }

    [Serializable]
    private sealed class StormVariants
    {

        [SerializeField] private Storm[] _storms;

        public Storm Pick()
        {
            return _storms[Randomize.Index(_storms.Length)];
        }
    }

}
