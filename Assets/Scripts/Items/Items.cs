using System.Collections.Generic;
using UnityEngine;

public static class Items
{

    public const string RAT_ID = "Rat";
    public const string COOKED_RAT_ID = "CookedRat";

    private static readonly Dictionary<string, Item> _items = new Dictionary<string, Item>();
    private static readonly List<Item> _all = new List<Item>();
                            
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void LoadItems()
    {
        foreach (var item in Resources.LoadAll<Item>(string.Empty))
        {
            _items.Add(item.name, item);
            _all.Add(item);
        }
    }

    public static Item Get(string id)
    {
        return _items[id];
    }

    public static Item[] GetAll()
    {
        return _all.ToArray();
    }

}
