using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsertOnStart : MonoBehaviour
{

    [SerializeField] private Item _item;

    private void Start()
    {
        if (TryGetComponent(out ItemPedistal pedistal) == false)
            return;

        pedistal.Place(_item);
    }

}
