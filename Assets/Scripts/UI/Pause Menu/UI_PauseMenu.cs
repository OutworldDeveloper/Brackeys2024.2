using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(Order.UI)]
public sealed class UI_PauseMenu : UI_Panel
{

    [SerializeField] private CanvasGroup _mainPanel;
    [SerializeField] private RectTransform _buttonsPanel;
    [SerializeField] private Transform _buttonsParent;
    [SerializeField] private Prefab<UI_YesNoWindow> _yesNoWindow;
    [SerializeField] private Prefab<UI_Panel> _settingsPanel;

    private Sequence _sequeence;

    private void OnDisable()
    {
        _sequeence?.Kill();
    }

    public void ContinueButton()
    {
        CloseAndDestroy();
    }

    public void RestartButton()
    {
        Player.OpenPanel(_yesNoWindow).Setup("Restart?", string.Empty, () =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
    }

    public void SettingsButton()
    {
        Player.OpenPanel(_settingsPanel);
    }

    public void ExitButton()
    {
        Application.Quit();
    }

}
