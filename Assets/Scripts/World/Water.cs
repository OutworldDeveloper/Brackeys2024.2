using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : Singleton<Water>
{
    public static float Level => Instance != null ? Instance.transform.position.y : float.NegativeInfinity;
    public static float BaseLevel => Instance != null ? Instance._baseLevel : float.NegativeInfinity;

    public static void SetLevel(float level)
    {
        if (Instance == null)
            return;

        Instance.transform.position = new Vector3(Instance.transform.position.x, level, Instance.transform.position.z);
    }

    private float _baseLevel;

    private void Start()
    {
        _baseLevel = transform.position.y;
    }

}

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null)
            throw new Exception($"There is more than one singleton of type {nameof(T)} present on the scene!");
        Instance = this as T;
    }

}

public abstract class PersistentSingleton<T> : MonoBehaviour where T : PersistentSingleton<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null)
            throw new Exception($"There is more than one singleton of type {nameof(T)} present on the scene!");
        Instance = this as T;
        DontDestroyOnLoad(Instance);
    }


}
