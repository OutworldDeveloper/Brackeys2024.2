using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public class UI_PawnActionsGuide : MonoBehaviour
{

    [SerializeField] private Player _player;
    [SerializeField] private Prefab<UI_PawnAction> _displayPrefab;
    [SerializeField] private Transform _parent;

    private readonly List<UI_PawnAction> _currentDisplays = new List<UI_PawnAction>();

    private void Awake()
    {
        _player.PawnStack.ActivePawnChanged += OnPawnChanged;
        _player.Panels.Changed += OnPanelsChanged;
        _parent.gameObject.SetActive(false);
    }

    private void OnPawnChanged(Pawn pawn)
    {
        Refresh();
    }

    private void OnPanelsChanged()
    {
        Refresh();
    }

    private PawnAction _closePanelAction = new PawnAction("Back", KeyCode.Escape);

    private void Refresh()
    {
        ClearDisplays();

        _parent.gameObject.SetActive(false);

        if (_player.Panels.HasActivePanel)
        {
            if (_player.Panels.Active.CanUserClose() == false)
                return;

            _parent.gameObject.SetActive(true);

            var display = _displayPrefab.Instantiate();
            display.SetTarget(_closePanelAction);
            display.transform.SetParent(_parent, false);

            _currentDisplays.Add(display);
        }
        else
        {
            if (_player.PawnStack.ActivePawn.HasActions == false)
                return;

            _parent.gameObject.SetActive(true);

            foreach (var action in _player.PawnStack.ActivePawn.GetActions())
            {
                var display = _displayPrefab.Instantiate();
                display.SetTarget(action);
                display.transform.SetParent(_parent, false);

                _currentDisplays.Add(display);
            }
        }
    }

    private void ClearDisplays()
    {
        foreach (var display in _currentDisplays)
        {
            Destroy(display.gameObject);
        }

        _currentDisplays.Clear();
    }

}
