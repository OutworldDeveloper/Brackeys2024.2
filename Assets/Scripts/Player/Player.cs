using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(-1)]
public sealed class Player : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _character;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _hud;
    [SerializeField] private Prefab<UI_PauseMenu> _pauseMenu;
    [SerializeField] private Prefab<UI_Panel> _deathScreen;
    [SerializeField] private Prefab<UI_InventorySelectScreen> _itemSelectionScreen;

    [SerializeField] private Volume _blurVolume;
    [SerializeField] private UI_PanelsManager _panels;

    [SerializeField] private AnimationCurve _cameraTransitionCurve;

    [SerializeField] private RectTransform _hudParent;

    private CameraState _currentState;
    private CameraState _lastCameraState;

    // Hud from other pawns (not character)
    private Transform _currentHud;

    public UI_PanelsManager Panels => _panels;
    public PawnStack PawnStack { get; private set; }

    private void Awake()
    {
        PawnStack = new PawnStack(this, _character);
        PawnStack.ActivePawnChanged += OnActivePawnChanged;
        _character.Damaged += OnCharacterDamaged;
        _character.Died += OnCharacterDied;
    }

    private void Update()
    {
        UpdateState();

        if (Input.GetKeyDown(KeyCode.Escape) == true || Input.GetKeyDown(KeyCode.Tab) == true)
            HandleEscapeButton();

        if (_panels.HasActivePanel == true)
        {
            _panels.Active.InputUpdate();
            return;
        }

        PawnStack.ActivePawn.InputTick();
    }

    private void LateUpdate()
    {
        if (PawnStack.CameraTransition == CameraTransition.Fade && PawnStack.TimeSinceLastActivePawnChange < 0.2f)
            return;

        CameraState cameraState = PawnStack.ActivePawn.GetCameraState();

        if (PawnStack.CameraTransition == CameraTransition.Move && PawnStack.TimeSinceLastActivePawnChange < 0.4f)
        {
            float t = PawnStack.TimeSinceLastActivePawnChange / 0.4f;
            t = _cameraTransitionCurve.Evaluate(t);
            cameraState = CameraState.Lerp(_lastCameraState, cameraState, t);
        }

        ApplyCameraState(cameraState);

        _blurVolume.enabled = PawnStack.ActivePawn.GetBlurStatus(out float targetBlurDistance);

        // Blur
        if (_blurVolume.profile.TryGet(out DepthOfField dof))
        {
            dof.focusDistance.Override(targetBlurDistance);
        }
    }

    private void HandleEscapeButton()
    {
        if (_panels.HasActivePanel == true)
        {
            _panels.TryCloseActivePanel();
            return;
        }

        if (PawnStack.IsStackEmpty == false && PawnStack.ActivePawn.CanRemoveAtWill() == true)
        {
            PawnStack.Remove(PawnStack.ActivePawn);
        }
        else
        {
            _panels.InstantiateAndOpenFrom(_pauseMenu);
        }
    }

    private void OnActivePawnChanged(Pawn activePawn)
    {
        _lastCameraState = _currentState;

        if (PawnStack.CameraTransition == CameraTransition.Fade)
            ScreenFade.FadeOutFor(0.6f);

        // Custom Huds
        if (_currentHud != null)
            Destroy(_currentHud.gameObject);

        _currentHud = PawnStack.ActivePawn.CreateHud();

        if (_currentHud != null)
            _currentHud.SetParent(_hudParent, false);
    }

    private void OnCharacterDamaged()
    {
        PawnStack.RemoveAll();
    }

    private void OnCharacterDied()
    {
        Delayed.Do(() => _panels.InstantiateAndOpenFrom(_deathScreen), 1.75f);
    }

    private void UpdateState()
    {
        // Hud
        bool showHud =
            _panels.HasActivePanel == false &&
            _character.IsDead == false &&
            PawnStack.IsStackEmpty == true;

        _hud.SetActive(showHud);

        // Cursor
        bool showCursor = _panels.HasActivePanel == true || PawnStack.ActivePawn.ShowCursor == true;

        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;

        // Pause
        bool pauseGame = _panels.RequirePause();

        Time.timeScale = pauseGame ? 0f : 1f;
    }

    private void ApplyCameraState(CameraState state)
    {
        _mainCamera.transform.SetPositionAndRotation(state.Position, state.Rotation);
        _mainCamera.fieldOfView = state.FieldOfView;

        _currentState = state;
    }

    public bool TryOpenItemSelection(ItemSelector selector)
    {
        if (_character.Inventory.IsEmpty)
            return false;

        var selectionScreen = Panels.InstantiateAndOpenFrom(_itemSelectionScreen);
        selectionScreen.Setup(_character.Inventory, selector);
        return true;
    }

}

public sealed class PawnStack
{

    public event Action<Pawn> ActivePawnChanged;

    private readonly Player _player;
    private readonly Pawn _defaultPawn;
    private readonly List<Pawn> _stack = new List<Pawn>();

    public PawnStack(Player player, Pawn defaultPawn)
    {
        _player = player;
        _defaultPawn = defaultPawn;
        _defaultPawn.OnAddedToStack(player);
        _defaultPawn.OnReceivePlayerControl();

        CameraTransition = defaultPawn.CameraTransition;
    }

    public Pawn ActivePawn => _stack.Count > 0 ? _stack[^1] : _defaultPawn;
    public bool IsStackEmpty => _stack.Count == 0;
    public TimeSince TimeSinceLastActivePawnChange { get; private set; } = TimeSince.Never;

    public CameraTransition CameraTransition { get; private set; }

    public void Push(Pawn pawn)
    {
        ActivePawn.OnLostPlayerControl();

        _stack.Add(pawn);
        pawn.OnAddedToStack(_player);
        pawn.OnReceivePlayerControl();

        TimeSinceLastActivePawnChange = TimeSince.Now();
        CameraTransition = pawn.CameraTransition;
        ActivePawnChanged?.Invoke(ActivePawn);
    }

    public void Remove(Pawn pawn)
    {
        bool removingActivePawn = pawn == ActivePawn;

        if (removingActivePawn == true)
        {
            pawn.OnLostPlayerControl();
        }

        _stack.Remove(pawn);
        pawn.OnRemovedFromStack(_player);

        if (removingActivePawn == true)
        {
            ActivePawn.OnReceivePlayerControl();
            TimeSinceLastActivePawnChange = TimeSince.Now();
            CameraTransition = pawn.CameraTransition;
            ActivePawnChanged?.Invoke(ActivePawn);
        }
    }

    public void RemoveAll()
    {
        if (IsStackEmpty == true)
            return;

        ActivePawn.OnLostPlayerControl();
        ActivePawn.OnRemovedFromStack(_player);

        for (int i = _stack.Count - 1; i >= 0; i--)
        {
            _stack[i].OnRemovedFromStack(_player);
            CameraTransition = _stack[i].CameraTransition; // Not sure if this is correct
        }

        ActivePawn.OnReceivePlayerControl();

        TimeSinceLastActivePawnChange = TimeSince.Now();
        ActivePawnChanged?.Invoke(ActivePawn);
    }

}
