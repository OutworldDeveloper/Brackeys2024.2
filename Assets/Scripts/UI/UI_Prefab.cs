using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class UI_Prefab<T> where T : Component
{

    [SerializeField] private Prefab<T> _prefab;
    [SerializeField] private RectTransform _parent;

    private readonly List<T> _spawned = new List<T>();

    public T Spawn()
    {
        var instance = _prefab.Instantiate();
        _spawned.Add(instance);
        instance.transform.SetParent(_parent, false);
        return instance;
    }

    public void DestroySpawned()
    {
        for (int i = 0; i < _spawned.Count; i++)
        {
            GameObject.Destroy(_spawned[i].gameObject);
        }
    }

}
