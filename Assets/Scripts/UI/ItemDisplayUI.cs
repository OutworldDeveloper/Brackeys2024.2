using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ItemDisplayUI : MonoBehaviour
{

    public event Action<Item> Selected;

    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _label;

    private Item _item;

    public void Init(Item item)
    {
        _item = item;
        _image.sprite = item.Sprite;
        _label.text = item.DisplayName;
    }

    public void Select()
    {
        Selected?.Invoke(_item);
    }

}
