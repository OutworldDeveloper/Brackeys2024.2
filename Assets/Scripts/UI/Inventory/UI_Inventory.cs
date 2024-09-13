using System.Collections.Generic;
using UnityEngine;

public sealed class InventoryUI : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private ItemDisplayUI _itemDisplayPrefab;

    private readonly List<ItemDisplayUI> _displays = new List<ItemDisplayUI>();

    private void OnEnable()
    {
        _player.Inventory.ItemAdded += OnItemAdded;
        _player.Inventory.ItemRemoved += OnItemRemoved;
        Refresh();
    }

    private void OnDisable()
    {
        _player.Inventory.ItemAdded -= OnItemAdded;
        _player.Inventory.ItemRemoved -= OnItemRemoved;
    }

    private void OnItemAdded(Item item)
    {
        Refresh();
    }

    private void OnItemRemoved(Item item)
    {
        Refresh();
    }

    private void Refresh()
    {
        foreach (var display in _displays)
        {
            Destroy(display.gameObject);
        }

        _displays.Clear();

        foreach (var item in _player.Inventory)
        {
            var display = Instantiate(_itemDisplayPrefab, transform, false);
            display.Init(item);
            _displays.Add(display);
        }
    }


}