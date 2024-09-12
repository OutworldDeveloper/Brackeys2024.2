public sealed class MicrowaveActivationInteraction : Interaction
{

    private Microwave _microwave;

    private void Awake()
    {
        _microwave = GetComponentInParent<Microwave>();
    }

    public override string Text => "Turn on";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return !_microwave.Door.IsOpen && !_microwave.IsOn;
    }

    public override void Perform(PlayerCharacter player)
    {
        _microwave.TurnOn();
    }

}
