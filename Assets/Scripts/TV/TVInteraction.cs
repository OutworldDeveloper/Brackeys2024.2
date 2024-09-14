using UnityEngine;

public sealed class TVInteraction : Interaction
{

    [SerializeField] private TV _target;
    [SerializeField] private CameraPawn _pawn;

    public override string Text => "Watch";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _target.IsOn;
    }

    public override void Perform(PlayerCharacter player)
    {
        player.Player.AddPawn(_pawn);
    }

}
