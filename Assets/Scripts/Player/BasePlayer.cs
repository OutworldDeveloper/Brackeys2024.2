using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePlayer : MonoBehaviour
{

    public event Action<Option<GameplayState>> StateChanged;

    [SerializeField] private Camera _mainCamera;
    [SerializeField] private RectTransform _panelsStack;
    [SerializeField] private Transform _backgroundHidder;
    [SerializeField] private AnimationCurve _cameraTransitionCurve;

    private readonly List<GameplayState> _stack = new List<GameplayState>();

    private CameraTransition _cameraTransition;
    private CameraState _currentState;
    private CameraState _lastCameraState;

    private readonly List<UI_Panel> _panelsToAdd = new List<UI_Panel>();

    public Option<GameplayState> ActiveGameplay => _stack.Count > 0 ? 
        Option<GameplayState>.Some(_stack[^1]) : 
        Option<GameplayState>.None();
    public bool IsStackEmpty => _stack.Count == 0;
    public TimeSince TimeSinceLastGameplayStateChange { get; private set; } = TimeSince.Never;

    protected virtual void Start() { }

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

        ActiveGameplay.Do(state => state.InputTick());
    }

    protected virtual void LateUpdate()
    {
        if (_cameraTransition == CameraTransition.Fade && TimeSinceLastGameplayStateChange < 0.2f)
            return;

        CameraState cameraState = GetCameraState();

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
        bool showCursor = ActiveGameplay.Map(state => state.ShowCursor, true);
        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;

        // Pause
        bool pauseGame = RequirePause();
        Time.timeScale = pauseGame ? 0f : 1f;
    }

    protected virtual bool HandleEscapeButton()
    {
        if (_stack.Count <= 1)
            return false;

        var state = ActiveGameplay.Deconstruct();

        if (state.CanRemoveAtWill() == false)
            return true;

        switch (state)
        {
            case Pawn pawn: RemovePawn(pawn); break;
            case UI_Panel panel: RemoveAndDestroyPanel(panel); break;
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

    public T OpenPanel<T>(Prefab<T> prefab) where T : UI_Panel
    {
        T panel = prefab.Instantiate();
        panel.transform.SetParent(_panelsStack, false);
        _panelsToAdd.Add(panel); // Delayed until next frame!
        return panel;
    }

    private void Push(GameplayState state)
    {
        ActiveGameplay.Do(state => state.OnLostPlayerControl());

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

    public void RemoveAndDestroyPanel(UI_Panel panel)
    {
        Remove(panel);
        Destroy(panel.gameObject);
    }

    private void Remove(GameplayState state)
    {
        if (_stack.Contains(state) == false)
            throw new Exception("Cannot remove state that is not present in the stack");

        bool removingActivePawn = state == ActiveGameplay.Deconstruct();

        if (removingActivePawn == true)
        {
            state.OnLostPlayerControl();
        }

        _stack.Remove(state);
        state.OnRemovedFromStack();

        if (removingActivePawn == true)
        {
            ActiveGameplay.Do(activeState => activeState.OnReceivePlayerControl());

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

    private CameraState GetCameraState()
    {
        for (int i = _stack.Count - 1; i >= 0; i--)
        {
            switch (_stack[i])
            {
                case UI_Panel panel:
                    var camera = panel.GetVirtualCamera();
                    if (camera != null)
                        return camera.State;
                    continue;
                case Pawn pawn:
                    return pawn.GetVirtualCamera().State;
            }
        }

        return default;
    }

}

public readonly struct Option<T> where T : class
{

    private readonly T _value;

    private Option(T value)
    {
        _value = value;
    }

    public static Option<T> Some(T value) => new Option<T>(value);
    public static Option<T> None() => new Option<T>(null);

    public void Do(Action<T> action)
    {
        if (_value != null)
            action.Invoke(_value);
    }

    public T Deconstruct() => _value;

    public T Reduce(T defaultValue) => 
        _value != null ? _value : defaultValue;

    public TResult Map<TResult>(Func<T, TResult> func, TResult defaultValue) => 
        _value != null ? func.Invoke(_value) : defaultValue;

}
