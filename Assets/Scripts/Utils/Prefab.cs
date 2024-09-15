using UnityEngine;

[System.Serializable]
public struct Prefab<T> where T : Component
{

    [SerializeField] private T _asset;

    public T Asset => _asset;

    public T Instantiate()
    {
        var instance = Object.Instantiate(_asset);
        instance.name = _asset.name;
        return instance;
    }

    public T Instantiate(Vector3 position, Quaternion rotation)
    {
        return GameObject.Instantiate(_asset, position, rotation);
    }

}