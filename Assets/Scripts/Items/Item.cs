using Alchemy.Inspector;
using UnityEngine;

[CreateAssetMenu]
public class Item : ScriptableObject
{

    [field: SerializeField, FoldoutGroup(nameof(Item))] public string DisplayName { get; private set; }
    [field: SerializeField, FoldoutGroup(nameof(Item))] public Sprite Sprite { get; private set; }

}
