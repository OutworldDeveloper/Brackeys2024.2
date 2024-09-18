using UnityEngine;

[DefaultExecutionOrder(-1)]
public sealed class Player : BasePlayer
{

    public static bool IsFirstTime = true;
    public static bool ShowedSwimmingTutorial = false;

    [SerializeField] private PlayerCharacter _character;
    [SerializeField] private GameObject _hud;
    [SerializeField] private Prefab<UI_PauseMenu> _pauseMenu;
    [SerializeField] private Prefab<UI_Panel> _deathScreen;

    private void Awake()
    {
        _character.Damaged += OnCharacterDamaged;
        _character.Died += OnCharacterDied;
        _character.InWater += OnCharacterInWater;
    }

    private void OnCharacterInWater()
    {
        if (ShowedSwimmingTutorial == true)
            return;

        ShowedSwimmingTutorial = true;
        new Notification(4f).Append("Press").Append(KeyCode.Space, KeyCode.LeftShift).Append("to swim!").Show();
    }

    protected override void Start()
    {
        base.Start();

        AddPawn(_character);

        if (IsFirstTime == false)
            return;

        IsFirstTime = false;

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            const string WEBGL_INTRODUCTION =
                "Please use Tab instead of Esc to pause, as this is a WebGL game. Use Shift to sneak. " +
                "If you encounter any bugs, consider downloading the executable version.";

            OpenPanel(Panels.GenericMenu).
                WithLabel("Hello").
                WithDescription(WEBGL_INTRODUCTION).
                WithCloseButton();
        }
        else
        {
            new Notification(6f).Append("Use").Append(KeyCode.LeftShift).Append(" to sneak").Show();
        }
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
        bool showHud = ActiveGameplay.Map(active => active == _character, false);
        _hud.SetActive(showHud);
    }

}
