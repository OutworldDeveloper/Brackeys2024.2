using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finale : MonoBehaviour
{
    
    [SerializeField] private ItemPedistal[] _pedistals;
    [SerializeField] private Door _finalDoor;

    private void Awake()
    {
        foreach (var pedistal in _pedistals)
        {
            pedistal.Updated += OnPedistalUpdated;
        }
    }

    private void Start()
    {
        _finalDoor.Block();
    }

    private void OnPedistalUpdated()
    {
        foreach (var pedistal in _pedistals)
        {
            if (pedistal.DisplayItem == null)
                return;
        }

        _finalDoor.Unblock();
        _finalDoor.Open();
    }

}
