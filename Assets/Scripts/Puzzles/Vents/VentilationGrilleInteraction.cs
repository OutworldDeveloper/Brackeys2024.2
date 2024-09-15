using UnityEngine;

public sealed class VentilationGrilleInteraction : Interaction
{

    public const string OPEN = "Open";
    public const string SCREWDRIVER_ID = "Screwdriver";

    [SerializeField] private VentilationGrille _grille;
    [SerializeField] private AudioSource _failedAttemptAudio;

    public override string Text => OPEN;

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _grille.IsOpen == false;
    }

    public override void Perform(PlayerCharacter player)
    {
        if (player.Inventory.HasItem(SCREWDRIVER_ID) == false)
        {
            Notification.Show("I need a screwdriver to open this");
            _failedAttemptAudio.Play();
            return;
        }

        _failedAttemptAudio.Play();
        var itemSelector = new ScrewdriverSelector(_grille);
        player.Player.OpenPanel(Panels.SelectionScreen).Setup(player.Inventory, itemSelector);
    }

    private sealed class ScrewdriverSelector : ItemSelector
    {

        private readonly VentilationGrille _grille;

        public ScrewdriverSelector(VentilationGrille grille)
        {
            _grille = grille;
        }

        public override bool CanAccept(Item item)
        {
            return item.name == SCREWDRIVER_ID;
        }

        public override void Select(Inventory inventory, Item item)
        {
            _grille.Open();
        }

    }

}
