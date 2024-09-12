using UnityEngine;

public sealed class ToggleDoorInteraction : Interaction
{

    [SerializeField] private Door _door;
    [SerializeField] private bool _canClose = true;

    public override string Text => _door.IsOpen ? "Close" : "Open";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        if (_door.IsOpen == true && _canClose == false)
            return false;

        return base.IsAvaliable(player);
    }

    public override void Perform(PlayerCharacter player)
    {
        if (_door.IsOpen == true && _canClose == true)
        {
            _door.Close();
            return;
        }

        if (_door.IsLocked == true && player.Inventory.IsEmpty == false)
        {
            //(player.Player as Player)?.OpenItemSelection(new KeySelector(_door));
            player.Player.OpenPanel(Panels.SelectionScreen).
                Setup(player.Inventory, new KeySelector(_door));
        }

        _door.TryOpen();
    }

    private sealed class KeySelector : ItemSelector
    {

        private readonly Door _door;

        public KeySelector(Door door)
        {
            _door = door;
        }

        public override bool CanAccept(Item item)
        {
            return item == _door.Key;
        }

        public override void Select(Inventory inventory, Item item)
        {
            inventory.RemoveItem(item);
            _door.TryUnlock(item);
            _door.TryOpen();
        }

        public override string GetRejectionReason(Item item)
        {
            return "Won't open";
        }

    }

}
