using UnityEngine;

public sealed class ZombieFactory : MonoBehaviour
{

    [SerializeField] private Prefab<Zombie> _prefab;
    [SerializeField] private PlayerCharacter _playerCharacter;

    private LocationInfo[] _locations;

    private void Awake()
    {
        _locations = FindObjectsOfType<LocationInfo>();
    }

    public Zombie Spawn(Vector3 location)
    {
        var zombie = _prefab.Instantiate(location, Quaternion.identity);
        //zombie.Warp(location, direction);
        zombie.Setup(_playerCharacter, _locations);
        return zombie;
    }

}
