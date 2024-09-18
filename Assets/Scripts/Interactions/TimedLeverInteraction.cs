using UnityEngine;

public sealed class TimedLeverInteraction : Interaction
{

    public const string ACTIVATE = "Activate";
    public const string INSPECT = "Inspect";
    public const string TUTORIAL_TEXT = "There are 3 switches on the map. Reactivate all of them to restore the power if it goes out";

    public static bool HasInspected = false;

    [SerializeField] private TimedLever _timedLever;

    public override string Text => _timedLever.IsActivated ? INSPECT : ACTIVATE;

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return true;
    }

    public override void Perform(PlayerCharacter player)
    {
        if (_timedLever.IsActivated == false)
        {
            _timedLever.Activate(true);
            return;
        }

        if (HasInspected == true)
        {
            Notification.Show("Use these switches to restore power if it goes out");
            return;
        }

        player.Player.OpenPanel(Panels.GenericMenu).
            WithLabel("Tutorial").
            WithDescription(TUTORIAL_TEXT).
            WithCloseButton("Ok").
            WithClosability(true);

        HasInspected = true;
    }

}
