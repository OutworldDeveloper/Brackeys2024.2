using UnityEngine;

public class PickupInteraction : Interaction
{

    [SerializeField] private Item _item;

    private string _text;

    public override string Text => _text;

    private void Start()
    {
        _text = $"Pickup {_item.DisplayName}";
    }

    public override void Perform(PlayerCharacter player)
    {
        player.Inventory.AddItem(_item);
        Destroy(gameObject);
    }

}
