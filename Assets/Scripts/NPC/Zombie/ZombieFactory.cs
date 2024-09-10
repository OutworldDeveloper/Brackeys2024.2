using UnityEngine;

public sealed class ZombieFactory : MonoBehaviour
{

    [SerializeField] private Prefab<Zombie> _prefab;
    [SerializeField] private PlayerCharacter _playerCharacter;
    [SerializeField] private RoomInfo[] _rooms;

    public Zombie Spawn(Vector3 location, Vector3 direction)
    {
        var zombie = _prefab.Instantiate(location, Quaternion.LookRotation(direction, Vector3.up));
        zombie.Warp(location, direction);
        zombie.Setup(_playerCharacter, _rooms);
        return zombie;
    }

}
