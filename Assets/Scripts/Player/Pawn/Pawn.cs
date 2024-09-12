using System.Collections.Generic;
using UnityEngine;

public abstract class Pawn : GameplayState
{

    [SerializeField] private VirtualCamera _virtualCamera;
    [SerializeField] private CameraTransition _transition;

    protected VirtualCamera VirtualCamera => _virtualCamera;

    public override VirtualCamera GetVirtualCamera()
    {
        return _virtualCamera;
    }

    public override CameraTransition GetCameraTransition()
    {
        return _transition;
    }

    public void RemoveFromStack()
    {
        Player.RemovePawn(this);
    }

}

public enum CameraTransition
{
    None,
    Fade,
    Move
}