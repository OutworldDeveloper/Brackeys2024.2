using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public class UI_Panel : GameplayState
{

    [SerializeField] private bool _requirePause = true;
    [field: SerializeField] public bool HidePanelsBelow { get; private set; } = false;
    [field: SerializeField] public bool HideBackground { get; private set; } = true;

    private VirtualCamera _virtualCamera;
    private CameraTransition _cameraTransition;

    public override VirtualCamera GetVirtualCamera() => _virtualCamera;
    public override CameraTransition GetCameraTransition() => _cameraTransition;
    public override bool RequirePause => _requirePause;
    public override bool ShowCursor => true;

    public override bool CanRemoveAtWill() => true;

    public void CloseAndDestroy()
    {
        Player.RemoveAndDestroyPanel(this);
    }

    public void SetVirtualCamera(VirtualCamera camera, CameraTransition transition)
    {
        _virtualCamera = camera;
        _cameraTransition = transition;
    }
}
