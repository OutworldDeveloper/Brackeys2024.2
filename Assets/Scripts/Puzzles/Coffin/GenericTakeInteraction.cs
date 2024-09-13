using UnityEngine;

public sealed class GenericTakeInteraction : Interaction
{

    [SerializeField] private ItemPedistal _pedistal;

    public override string Text => "Take out";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _pedistal.DisplayItem != null;
    }

    public override void Perform(PlayerCharacter player)
    {
        player.Inventory.AddItem(_pedistal.Remove());
    }

}
