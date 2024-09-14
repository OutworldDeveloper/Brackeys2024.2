using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPedistal : MonoBehaviour
{

    public event Action<Item> ItemPlaced;
    public event Action ItemRemoved;
    public event Action Updated;

    public bool ContainsItem => DisplayItem != null;
    public Item DisplayItem { get; private set; }

    private ItemModel _model;

    public void Place(Item item)
    {
        DisplayItem = item;
        Refresh();
        ItemPlaced?.Invoke(DisplayItem);
        Updated?.Invoke();
    }

    public Item Remove()
    {
        var item = DisplayItem;
        DisplayItem = null;
        Refresh();
        ItemRemoved?.Invoke();
        Updated?.Invoke();
        return item;
    }

    private void Refresh()
    {
        if (_model != null)
            Destroy(_model.gameObject);

        if (DisplayItem == null)
            return;

        _model = DisplayItem.Model.Instantiate();
        _model.transform.SetParent(transform, false);
        _model.EnableCollision(false);
        _model.EnableGlow(false);
    }

}
