using UnityEngine;
using System.Linq;

public sealed class GenericInsertInteraction : Interaction
{

    [SerializeField] private ItemPedistal _pedistal;
    [SerializeField] private Item[] _allowedItems = new Item[0];
    [SerializeField] private VirtualCamera _virtualCamera;

    public override string Text => "Insert";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _pedistal.DisplayItem == null;
    }

    public override void Perform(PlayerCharacter player)
    {
        if (player.Inventory.IsEmpty)
            Notification.Show("I have nothing to insert");
        else
            player.Player.OpenPanel(Panels.SelectionScreen).
                Setup(player.Inventory, new InsertItemSelector(_pedistal, _allowedItems)).
                SetVirtualCamera(_virtualCamera, CameraTransition.Move);
    }

    private sealed class InsertItemSelector : ItemSelector
    {

        private readonly ItemPedistal _pedistal;
        private readonly Item[] _allowedItems;

        public InsertItemSelector(ItemPedistal pedistal, Item[] allowedItems)
        {
            _pedistal = pedistal;
            _allowedItems = allowedItems;
        }

        public override bool CanAccept(Item item)
        {
            return _allowedItems.Contains(item);
        }

        public override void Select(Inventory inventory, Item item)
        {
            inventory.RemoveItem(item);
            _pedistal.Place(item);
        }

    }

}
