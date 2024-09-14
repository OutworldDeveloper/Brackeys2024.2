using UnityEngine;

public sealed class MicrowaveInteraction : Interaction
{
    public override string Text => _microwave.ContainsItem ? "Take" : "Put";

    private Microwave _microwave;

    [SerializeField] private VirtualCamera _virtualCamera;

    private void Awake()
    {
        _microwave = GetComponentInParent<Microwave>();
    }

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return base.IsAvaliable(player);
    }

    public override void Perform(PlayerCharacter player)
    {
        if (_microwave.ContainsItem)
        {
            player.Inventory.AddItem(_microwave.TakeOut());
            return;
        }

        if (player.Inventory.IsEmpty)
            Notification.Show("I have nothing to put");
        else
            player.Player.OpenPanel(Panels.SelectionScreen).
                Setup(player.Inventory, new MicrowaveSelector(_microwave)).
                SetVirtualCamera(_virtualCamera, CameraTransition.Move);
    }

    private sealed class MicrowaveSelector : ItemSelector
    {

        private readonly Microwave _microwave;

        public MicrowaveSelector(Microwave microwave)
        {
            _microwave = microwave;
        }

        public override bool CanAccept(Item item)
        {
            return item.name != Items.COOKED_RAT_ID;
        }

        public override void Select(Inventory inventory, Item item)
        {
            inventory.RemoveItem(item);
            _microwave.InsertItem(item);
        }

        public override string GetRejectionReason(Item item)
        {
            if (item.name == Items.COOKED_RAT_ID)
                return "It's already cooked enough";

            return base.GetRejectionReason(item);
        }

    }

}
