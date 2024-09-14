using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public sealed class Player : BasePlayer
{

    public static bool IsFirstTime = true;

    [SerializeField] private PlayerCharacter _character;
    [SerializeField] private GameObject _hud;
    [SerializeField] private Prefab<UI_PauseMenu> _pauseMenu;
    [SerializeField] private Prefab<UI_Panel> _deathScreen;

    private void Awake()
    {
        _character.Damaged += OnCharacterDamaged;
        _character.Died += OnCharacterDied;
    }

    protected override void Start()
    {
        base.Start();

        if (IsFirstTime == false)
            return;

        IsFirstTime = false;

        //OpenPanel(Panels.ConfirmationScreen).
        //    Setup("Hello", 
        //    "Please use Tab instead of Esc since it's a WEBGL game. Use Shift to sneak.", 
        //    () => { }, false);
    }

    protected override GameplayState GetDefaultState()
    {
        return _character;
    }

    protected override bool HandleEscapeButton()
    {
        if (base.HandleEscapeButton() == true)
            return true;

        OpenPanel(_pauseMenu);
        return true;
    }

    private void OnCharacterDamaged()
    {
        //PawnStack.RemoveAll();
    }

    private void OnCharacterDied()
    {
        Delayed.Do(() => OpenPanel(_deathScreen), 1.75f);
    }

    protected override void UpdateState()
    {
        base.UpdateState();
        bool showHud = IsStackEmpty == true && _character.IsDead == false;
        _hud.SetActive(showHud);
    }

}

public abstract class BasePlayer : MonoBehaviour
{

    public event Action<GameplayState> StateChanged;

    [SerializeField] private Camera _mainCamera;
    [SerializeField] private RectTransform _panelsStack;
    [SerializeField] private Transform _backgroundHidder;
    [SerializeField] private AnimationCurve _cameraTransitionCurve;

    private GameplayState _defaultPawn;
    private readonly List<GameplayState> _stack = new List<GameplayState>();
    private CameraTransition _cameraTransition;

    private CameraState _currentState;
    private CameraState _lastCameraState;

    private readonly List<UI_Panel> _panelsToAdd = new List<UI_Panel>();

    public GameplayState ActiveGameplay => _stack.Count > 0 ? _stack[^1] : _defaultPawn;
    public bool IsStackEmpty => _stack.Count == 0;
    public TimeSince TimeSinceLastGameplayStateChange { get; private set; } = TimeSince.Never;

    protected abstract GameplayState GetDefaultState();

    protected virtual void Start()
    {
        _defaultPawn = GetDefaultState();
        _defaultPawn.OnAddedToStack(this);
        _defaultPawn.OnReceivePlayerControl();
        _cameraTransition = _defaultPawn.GetCameraTransition();
    }

    protected virtual void Update()
    {
        for (int i = 0; i < _panelsToAdd.Count; i++)
        {
            Push(_panelsToAdd[i]);
        }
        _panelsToAdd.Clear();
        //

        UpdateState();

        if (Input.GetKeyDown(KeyCode.Escape) == true || Input.GetKeyDown(KeyCode.Tab) == true)
            HandleEscapeButton();

        ActiveGameplay.InputTick();
    }

    protected virtual void LateUpdate()
    {
        if (_cameraTransition == CameraTransition.Fade && TimeSinceLastGameplayStateChange < 0.2f)
            return;

        CameraState cameraState = GetFirstCamera().State;

        if (_cameraTransition == CameraTransition.Move && TimeSinceLastGameplayStateChange < 0.4f)
        {
            float t = TimeSinceLastGameplayStateChange / 0.4f;
            t = _cameraTransitionCurve.Evaluate(t);
            cameraState = CameraState.Lerp(_lastCameraState, cameraState, t);
        }

        ApplyCameraState(cameraState);
    }

    private void ApplyCameraState(CameraState state)
    {
        _mainCamera.transform.SetPositionAndRotation(state.Position, state.Rotation);
        _mainCamera.fieldOfView = state.FieldOfView;
        _currentState = state;
    }

    protected virtual void UpdateState()
    {
        // Cursor
        bool showCursor = ActiveGameplay.ShowCursor == true;
        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;

        // Pause
        bool pauseGame = RequirePause();
        Time.timeScale = pauseGame ? 0f : 1f;
    }

    protected virtual bool HandleEscapeButton()
    {
        if (IsStackEmpty)
            return false;

        if (ActiveGameplay.CanRemoveAtWill() == false)
            return true;

        switch (ActiveGameplay)
        {
            case Pawn pawn:
                RemovePawn(pawn);
                break;
            case UI_Panel panel:
                DestroyPanel(panel);
                break;
        }

        return true;
    }

    protected virtual void OnActiveStateChanged()
    {
        StateChanged?.Invoke(ActiveGameplay);

        _lastCameraState = _currentState;

        if (_cameraTransition == CameraTransition.Fade)
            ScreenFade.FadeOutFor(0.6f);
    }

    public void AddPawn(Pawn pawn)
    {
        Push(pawn);
    }

    // Queue for next frame, only isntantiate for now?
    public T OpenPanel<T>(Prefab<T> prefab) where T : UI_Panel
    {
        T panel = prefab.Instantiate();
        panel.transform.SetParent(_panelsStack, false);
        _panelsToAdd.Add(panel); // Delayed until next frame!
        return panel;
    }

    private void Push(GameplayState state)
    {
        ActiveGameplay.OnLostPlayerControl();

        _stack.Add(state);
        state.OnAddedToStack(this);
        state.OnReceivePlayerControl();

        TimeSinceLastGameplayStateChange = TimeSince.Now();
        _cameraTransition = state.GetCameraTransition();
        OnActiveStateChanged();
        RefreshView();
    }

    public void RemovePawn(Pawn pawn)
    {
        Remove(pawn);
    }

    public void DestroyPanel(UI_Panel panel)
    {
        Remove(panel);
        Destroy(panel.gameObject);
    }

    private void Remove(GameplayState state)
    {
        bool removingActivePawn = state == ActiveGameplay;

        if (removingActivePawn == true)
        {
            state.OnLostPlayerControl();
        }

        _stack.Remove(state);
        state.OnRemovedFromStack();

        if (removingActivePawn == true)
        {
            ActiveGameplay.OnReceivePlayerControl();
            TimeSinceLastGameplayStateChange = TimeSince.Now();
            _cameraTransition = state.GetCameraTransition();
            OnActiveStateChanged();
        }

        RefreshView();
    }

    public bool RequirePause()
    {
        for (int i = 0; i < _stack.Count; i++)
        {
            if (_stack[i].RequirePause == true)
                return true;
        }

        return false;
    }

    private void RefreshView()
    {
        // Disable hidder
        _backgroundHidder.SetParent(null, false);
        _backgroundHidder.gameObject.SetActive(false);

        var toHide = new List<UI_Panel>(_stack.Count);

        // Not safe to do with pawns! With pause menu too!
        for (int i = 0; i < _stack.Count; i++)
        {
            if (_stack[i] is UI_Panel panel)
                toHide.Add(panel);
        }

        bool hidderSet = false;

        for (int i = _stack.Count - 1; i >= 0; i--)
        {
            if (_stack[i] is not UI_Panel inspectedPanel)
                continue;

            inspectedPanel.gameObject.SetActive(true);
            toHide.Remove(inspectedPanel);

            if (hidderSet == false && inspectedPanel.HideBackground == true)
            {
                int siblingIndex = inspectedPanel.transform.GetSiblingIndex();
                _backgroundHidder.SetParent(_panelsStack, false);
                _backgroundHidder.SetSiblingIndex(siblingIndex);
                _backgroundHidder.gameObject.SetActive(true);
                hidderSet = true;
            }

            // If a panel hides panels below, it basically acts like the last panel
            if (inspectedPanel.HidePanelsBelow == true)
                break;
        }

        // Actually disable hidden panels
        for (int i = 0; i < toHide.Count; i++)
        {
            toHide[i].gameObject.SetActive(false);
        }
    }

    private VirtualCamera GetFirstCamera()
    {
        for (int i = _stack.Count - 1; i >= 0; i--)
        {
            switch (_stack[i])
            {
                case UI_Panel panel:
                    var camera = panel.GetVirtualCamera();
                    if (camera != null)
                        return camera;
                    continue;
                case Pawn pawn:
                    return pawn.GetVirtualCamera();
            }
        }

        return _defaultPawn.GetVirtualCamera();
    }

}

public abstract class GameplayState : MonoBehaviour
{

    private readonly List<PawnAction> _actions = new List<PawnAction>();

    public virtual bool ShowCursor => false;
    public BasePlayer Player { get; private set; }
    public bool IsPossesed => Player != null && Player.ActiveGameplay == this;
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
