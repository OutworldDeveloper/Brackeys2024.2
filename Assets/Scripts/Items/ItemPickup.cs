public class ItemPickup : Interaction
{
    public override string Text => _text;
    public Item Item { get; private set; }

    private string _text;

    public ItemPickup Setup(Item item)
    {
        Item = item;
        _text = $"Pickup {Item.DisplayName}";
        return this;
    }

    public override void Perform(PlayerCharacter player)
    {
        player.Inventory.AddItem(Item);
        Destroy(gameObject);
    }

}
