using UnityEngine;

public sealed class GenericTakeInteraction : Interaction
{

    [SerializeField] private ItemPedistal _pedistal;

    private int _lock = 0;

    public override string Text => "Take out";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _pedistal.DisplayItem != null && _lock == 0;
    }

    public override void Perform(PlayerCharacter player)
    {
        player.Inventory.AddItem(_pedistal.Remove());
    }

    public void Lock()
    {
        _lock++;
    }

    public void Unlock()
    {
        _lock--;
    }

}
