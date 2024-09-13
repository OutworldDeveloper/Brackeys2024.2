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

    private void OnEnable()
    {
        return;
        _sequeence = DOTween.Sequence().
            Append(_mainPanel.DOFade(1f, 0.75f).From(0f).SetEase(Ease.OutExpo)).
            Join(_buttonsPanel.DOScale(1f, 0.35f).From(0.9f).SetEase(Ease.OutExpo)).
            SetUpdate(true);

        int index = 0;
        foreach (Transform child in _buttonsParent)
        {
            if (child.gameObject.activeSelf == true)
                index++;
            else
                continue;

            _sequeence.Join(child.DOScale(1f, 0.14f).From(0.9f).SetDelay(index * 0.0065f));
        }
    }

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

}
