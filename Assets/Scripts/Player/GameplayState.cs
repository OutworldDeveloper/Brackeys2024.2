using System.Collections.Generic;
using UnityEngine;

public abstract class GameplayState : MonoBehaviour
{

    private readonly List<PawnAction> _actions = new List<PawnAction>();

    public virtual bool ShowCursor => false;
    public BasePlayer Player { get; private set; }
    public bool IsActive => Player != null && Player.ActiveGameplay.Map(state => state == this, false);
    public bool HasActions => _actions.Count > 0;
    public virtual bool RequirePause => false;

    public abstract VirtualCamera GetVirtualCamera();
    public abstract CameraTransition GetCameraTransition();

    public virtual void OnAddedToStack(BasePlayer player)
    {
        Player = player;
    }

    public virtual void OnRemovedFromStack() 
    {
        Player = null;
    }

    public virtual void OnReceivePlayerControl() { }
    public virtual void OnLostPlayerControl() { }
    public virtual void InputTick() { }
    public virtual bool CanRemoveAtWill() => true;
    public PawnAction[] GetActions()
    {
        return _actions.ToArray();
    }

    protected void RegisterAction(PawnAction action)
    {
        _actions.Add(action);
    }

}
