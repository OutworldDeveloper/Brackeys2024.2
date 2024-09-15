using UnityEngine;

public sealed class WatchTVInteraction : Interaction
{

    public const string INTERACTION_TEXT = "Watch";

    [SerializeField] private TV _tv;
    [SerializeField] private CameraPawn _pawn;

    public override string Text => INTERACTION_TEXT;

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _tv.IsPlayingSequence == true;
    }

    public override void Perform(PlayerCharacter player)
    {
        player.Player.AddPawn(_pawn);
    }

}
