using UnityEngine;

public sealed class Plate : MonoBehaviour
{
    [field: SerializeField] public ItemPedistal Pedistal { get; private set; }
    [field: SerializeField] public GenericTakeInteraction Interaction { get; private set; }

}
