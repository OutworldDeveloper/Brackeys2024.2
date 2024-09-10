using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public sealed class UI_InventorySelectScreen : UI_Panel
{

    [SerializeField] private Transform _layoutParent;
    [SerializeField] private ItemDisplayUI _itemDisplayPrefab;

    private Inventory _inventory;
    private ItemSelector _itemSelector;

    public void Setup(Inventory inventory, ItemSelector itemSelector)
    {
        _inventory = inventory;
        _itemSelector = itemSelector;

        foreach (var item in _inventory.Items)
        {
            var display = Instantiate(_itemDisplayPrefab, _layoutParent, false);
            display.Init(item);
            display.Selected += TrySelect;
        }
    }

    private void TrySelect(Item item)
    {
        if (_itemSelector.CanAccept(item) == false)
        {
            Notification.Show(_itemSelector.GetRejectionReason(item), 1.25f);
            return;
        }

        _itemSelector.Select(_inventory, item);
        CloseAndDestroy();
    }

}

public abstract class ItemSelector
{

    public const string DEFAULT_REJECTION_TEXT = "Won't work";

    public abstract bool CanAccept(Item item);
    public abstract void Select(Inventory inventory, Item item);
    public virtual string GetRejectionReason(Item item) => DEFAULT_REJECTION_TEXT;

}
