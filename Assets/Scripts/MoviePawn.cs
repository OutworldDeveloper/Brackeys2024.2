using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoviePawn : Pawn
{

    [SerializeField] private float _duration = 2f;
    [SerializeField] private bool _removable;

    private TimeUntil _timeUntilUnpossess;

    public void SetDuration(float duration)
    {
        _duration = duration;
    }

    public void SetRemovability(bool removable)
    {
        _removable = removable;
    }

    public override bool CanRemoveAtWill()
    {
        return _removable;
    }

    public override void OnReceivePlayerControl()
    {
        base.OnReceivePlayerControl();
        _timeUntilUnpossess = new TimeUntil(Time.time + _duration);
    }

    private void Update()
    {
        if (IsActive == false)
            return;

        if (_timeUntilUnpossess > 0f)
            return;

        RemoveFromStack();
    }

}
