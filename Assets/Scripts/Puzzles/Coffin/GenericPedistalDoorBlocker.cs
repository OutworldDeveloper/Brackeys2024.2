using UnityEngine;

public sealed class GenericPedistalDoorBlocker : DoorBlocker
{

    [SerializeField] private ItemPedistal _itemPedistal;
    [SerializeField] private bool _useCustomText;
    [SerializeField] private string _customText;

    public override bool IsActive()
    {
        return _itemPedistal.DisplayItem == null;
    }

    public override string GetBlockReason()
    {
        return _useCustomText ? _customText : base.GetBlockReason();
    }

}
