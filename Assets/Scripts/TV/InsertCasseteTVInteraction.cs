using UnityEngine;

public sealed class InsertCasseteTVInteraction : Interaction
{

    public const string INTERACTION_TEXT = "Play";

    [SerializeField] private TV _tv;

    public override string Text => INTERACTION_TEXT;

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _tv.IsPlayingSequence == false && _tv.IsOn == true;
    }

    public override void Perform(PlayerCharacter player)
    {
        if (player.Inventory.IsEmpty)
        {
            Notification.Show("I have nothing to play");
            return;
        }

        player.Player.OpenPanel(Panels.SelectionScreen).
            Setup(player.Inventory, new CasseteSelection(_tv));
    }

    private sealed class CasseteSelection : ItemSelector
    {

        private readonly TV _tv;

        public CasseteSelection(TV tv)
        {
            _tv = tv;
        }

        public override bool CanAccept(Item item)
        {
            return item.name == "Cassette";
        }

        public override void Select(Inventory inventory, Item item)
        {
            inventory.RemoveItem(item);
            _tv.InsertCassete();
            _tv.PlaySequence();
        }

        public override string GetRejectionReason(Item item)
        {
            if (item.name == Items.RAT_ID || item.name == Items.COOKED_RAT_ID)
                return "I don't think rats can be used this way";

            if (item.name == "Bread")
                return "Not sure why, but it doesn't work";

            return base.GetRejectionReason(item);
        }

    }

}
