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

        if (_mouseTrap.HasCheese == true)
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
            return item.name == Items.CHEESE_ID;
        }

        public override void Select(Inventory inventory, Item item)
        {
            inventory.RemoveItem(item);
            _mouseTrap.PlaceCheese();
        }

        public override string GetRejectionReason(Item item)
        {
            return "I don't think rats eat that";
        }

    }

}
