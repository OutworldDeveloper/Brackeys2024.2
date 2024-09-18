using TMPro;
using UnityEngine;

public sealed class UI_PlayerCard : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _readyStatus;
    [SerializeField] private RectTransform _playerVisuals;
    [SerializeField] private RectTransform _waitingVisuals;

    public void Show(string name, bool isReady)
    {
        _name.text = name;
        _readyStatus.text = isReady ? "Ready" : "Not ready";

        _playerVisuals.gameObject.SetActive(true);
        _waitingVisuals.gameObject.SetActive(false);
    }

    public void Hide()
    {
        _playerVisuals.gameObject.SetActive(false);
        _waitingVisuals.gameObject.SetActive(true);
    }

}
