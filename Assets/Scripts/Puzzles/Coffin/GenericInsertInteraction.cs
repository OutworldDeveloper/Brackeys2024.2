using UnityEngine;
using System.Linq;

public sealed class GenericInsertInteraction : Interaction
{

    [SerializeField] private string _verb = "Insert";
    [SerializeField] private ItemPedistal _pedistal;
    [SerializeField] private ExceptionsMode _mode = ExceptionsMode.WhiteList; // ExceptionsMode: WhiteList or BlackList
    [SerializeField] private Item[] _allowedItems = new Item[0]; // Exceptions
    [SerializeField] private VirtualCamera _virtualCamera;

    public override string Text => _verb;

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _pedistal.DisplayItem == null;
    }

    public override void Perform(PlayerCharacter player)
    {
        if (player.Inventory.IsEmpty)
            Notification.Show($"I have nothing to {_verb.ToLower()}");
        else
            player.Player.OpenPanel(Panels.SelectionScreen).
                Setup(player.Inventory, new InsertItemSelector(_pedistal, _mode, _allowedItems)).
                SetVirtualCamera(_virtualCamera, CameraTransition.Move);
    }

    private sealed class InsertItemSelector : ItemSelector
    {

        private readonly ItemPedistal _pedistal;
        private ExceptionsMode _mode;
        private readonly Item[] _exceptions;

        public InsertItemSelector(ItemPedistal pedistal, ExceptionsMode mode, Item[] exceptions)
        {
            _pedistal = pedistal;
            _mode = mode;
            _exceptions = exceptions;
        }

        public override bool CanAccept(Item item)
        {
            return IsAllowed(item);
        }

        public override void Select(Inventory inventory, Item item)
        {
            inventory.RemoveItem(item);
            _pedistal.Place(item);
        }

        private bool IsAllowed(Item item)
        {
            if (_mode == ExceptionsMode.WhiteList)
                return _exceptions.Contains(item);

            return !_exceptions.Contains(item);
        }

    }

    private enum ExceptionsMode
    {
        WhiteList,
        BlackList,
    }

}
