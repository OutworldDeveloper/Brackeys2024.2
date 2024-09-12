using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPedistal : MonoBehaviour
{

    public Item DisplayItem { get; private set; }

    private Transform _model;

    public void Show(Item item)
    {
        DisplayItem = item;
        Refresh();
    }

    public Item Remove()
    {
        var item = DisplayItem;
        DisplayItem = null;
        Refresh();
        return item;
    }

    private void Refresh()
    {
        if (_model != null)
            Destroy(_model.gameObject);

        if (DisplayItem == null)
            return;

        _model = DisplayItem.Model.Instantiate(transform.position, transform.forward);
        _model.SetParent(transform, false);
    }

}
