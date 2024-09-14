using UnityEngine;

[RequireComponent(typeof(MouseTrap))]
public sealed class MouseTrapInteraction : Interaction
{
    public override string Text => "Interact";

    [SerializeField] private VirtualCamera _camera;
    private MouseTrap _mouseTrap;

    private void Awake()
    {
        _mouseTrap = GetComponent<MouseTrap>();
    }

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _mouseTrap.IsDisabled == false;
    }

    public override void Perform(PlayerCharacter player)
    {
        if (_mouseTrap.HasMouse == true)
        {
            player.Inventory.AddItem(_mouseTrap.TakeRat());
            return;
        }

        if (_mouseTrap.HasBait == true)
        {
            Notification.Show("Now I just need to wait a little", 2f);
            return;
        }

        Notification.Show("Empty. Not rat, no bait", 1.5f);

        if (player.Inventory.IsEmpty == false)
            player.Player.OpenPanel(Panels.SelectionScreen).
                Setup(player.Inventory, new CheeseSelector(_mouseTrap)).
                SetVirtualCamera(_camera, CameraTransition.Move);
    }

    public sealed class CheeseSelector : ItemSelector
    {

        private readonly MouseTrap _mouseTrap;

        public CheeseSelector(MouseTrap mouseTrap)
        {
            _mouseTrap = mouseTrap;
        }

        public override bool CanAccept(Item item)
        {
            return item.name == "Bread" || item.name == "BreadStage2" || item.name == "BreadStage3";
        }

        public override void Select(Inventory inventory, Item item)
        {
            switch (item.name)
            {
                case "Bread":
                    inventory.AddItem(Items.Get("BreadStage2"), false);
                    break;
                case "BreadStage2":
                    inventory.AddItem(Items.Get("BreadStage3"), false);
                    break;
            }
            inventory.RemoveItem(item);
            _mouseTrap.PlaceBait();
        }

        public override string GetRejectionReason(Item item)
        {
            if (item.name == Items.RAT_ID || item.name == Items.COOKED_RAT_ID)
                return "No...";

            return "I don't think rats eat that";
        }

    }

}
