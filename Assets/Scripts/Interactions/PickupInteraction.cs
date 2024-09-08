using UnityEngine;

public class PickupInteraction : Interaction
{

    [SerializeField] private Item _item;
    [SerializeField] private int _amount = 1;

    private ItemStack _stack;
    private string _text;

    public override string Text => _text;

    private void Start()
    {
        _stack = new ItemStack(_item, _amount);
        _text = $"Pickup {_item.DisplayName}" + (_amount > 1 ? $" ({_amount})" : string.Empty);
    }

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return player.Inventory.CanAdd(_stack);
    }

    public override void Perform(PlayerCharacter player)
    {
        player.Inventory.TryAdd(_stack);
        gameObject.SetActive(false);
    }

}
