using UnityEngine;

public sealed class CoffinInteraction : Interaction
{

    [SerializeField] private DialoguePlayer _coffin;

    public override string Text => "Inspect";

    public override void Perform(PlayerCharacter player)
    {
        player.Player.PawnStack.Push(_coffin);
    }

}
