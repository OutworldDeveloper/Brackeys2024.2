using UnityEngine;

public sealed class CoffinInteraction : Interaction
{

    [SerializeField] private Prefab<UI_DialoguePlayer> _dialoguePlayer;

    public override string Text => "Inspect";

    public override void Perform(PlayerCharacter player)
    {
        player.Player.OpenPanel(_dialoguePlayer).Setup(GetComponentInParent<Coffin>());
    }

}
