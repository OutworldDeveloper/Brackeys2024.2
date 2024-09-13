using UnityEngine;

[CreateAssetMenu]
public class Item : ScriptableObject
{
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public Prefab<ItemModel> Model { get; private set; }

}
