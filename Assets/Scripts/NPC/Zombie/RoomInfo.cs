using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomInfo : MonoBehaviour
{

    [SerializeField] private FlatVector _size;

    public FlatVector GetRandomWalkableSurface()
    {
        for (int i = 0; i < 30; i++)
        {

        }
        return transform.position.Flat();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, _size.WithY(0.05f));
    }

}
