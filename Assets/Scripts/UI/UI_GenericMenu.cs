using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(Order.UI)]
public class UI_GenericMenu : UI_Panel
{

    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private TextMeshProUGUI _descriptionLabel;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Prefab<Button> _buttonPrefab;
    [SerializeField] private RectTransform _buttonsParent;

    private bool _canClose = true;
    private bool _closeButton = false;

    private void Start()
    {
        _cancelButton.gameObject.SetActive(_closeButton);
        _cancelButton.onClick.AddListener(OnCanceled);
    }

    public UI_GenericMenu WithLabel(string label)
    {
        _label.text = label;
        return this;
    }

    public UI_GenericMenu WithDescription(string description)
    {
        _descriptionLabel.text = description;
        return this;
    }

    public UI_GenericMenu WithButton(string label, Action action)
    {
        var button = _buttonPrefab.Instantiate();
        button.transform.SetParent(_buttonsParent, false);
        button.name = $"{label} button";
        button.GetComponentInChildren<TextMeshProUGUI>().text = label;
        button.onClick.AddListener(action.Invoke);
        EnsureCloseButtonOrder();
        return this;
    }

    public UI_GenericMenu WithCloseButton(string label = "Cancel")
    {
        _cancelButton.GetComponentInChildren<TextMeshProUGUI>().text = label;
        _closeButton = true;
        EnsureCloseButtonOrder();
        return this;
    }

    public UI_GenericMenu WithClosability(bool closable)
    {
        _canClose = closable;
        return this;
    }

    private void OnCanceled()
    {
        CloseAndDestroy();
    }

    public override bool CanRemoveAtWill()
    {
        return _canClose;
    }

    private void EnsureCloseButtonOrder()
    {
        _cancelButton.transform.SetAsLastSibling();
    }

}
