using UnityEngine;

public sealed class TurnOnTVInteraction : Interaction
{

    public const string INTERACTION_TEXT = "Turn On";

    [SerializeField] private TV _tv;

    public override string Text => INTERACTION_TEXT;

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _tv.IsOn == false;
    }

    public override void Perform(PlayerCharacter player)
    {
        _tv.TurnOn();
    }

}
