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
        _player.StateChanged += Refresh;
        _parent.gameObject.SetActive(false);
    }

    private PawnAction _closePanelAction = new PawnAction("Back", KeyCode.Tab);

    private void Refresh(GameplayState active)
    {
        ClearDisplays();
        _parent.gameObject.SetActive(false);

        if (_player.ActiveGameplay.CanRemoveAtWill() && _player.IsStackEmpty == false)
        {
            var display = _displayPrefab.Instantiate();
            display.SetTarget(_closePanelAction);
            display.transform.SetParent(_parent, false);
            _currentDisplays.Add(display);

            _parent.gameObject.SetActive(true);
        }

        if (_player.ActiveGameplay.HasActions == false)
            return;

        _parent.gameObject.SetActive(true);

        foreach (var action in _player.ActiveGameplay.GetActions())
        {
            var display = _displayPrefab.Instantiate();
            display.SetTarget(action);
            display.transform.SetParent(_parent, false);

            _currentDisplays.Add(display);
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
