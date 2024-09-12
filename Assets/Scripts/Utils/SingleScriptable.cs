using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SingleScriptable<T> : ScriptableObject where T : SingleScriptable<T>
{

    private static T _instance;

    public static T Instance => _instance == null ? Load() : _instance;

    private static T Load()
    {
        var path = typeof(T).Name;
        _instance = Resources.Load<T>(path);
        return _instance;
    }

}
