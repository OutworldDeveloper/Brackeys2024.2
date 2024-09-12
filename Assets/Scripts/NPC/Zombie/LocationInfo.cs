using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LocationInfo : MonoBehaviour
{

    [SerializeField] private FlatVector _size;

    private Vector3 _debugChosen;

    [ContextMenu("Test")]
    public void Test()
    {
        _debugChosen = GetRandomWalkableSurface();
    }

    public Vector3 GetRandomWalkableSurface()
    {
        for (int i = 0; i < 10; i++)
        {
            FlatVector candidate = new FlatVector()
            {
                x = transform.position.x + Randomize.Float(0, _size.x) - _size.x / 2,
                z = transform.position.z + Randomize.Float(0, _size.z) - _size.z / 2
            };

            var result = NavMesh.SamplePosition(candidate.WithY(transform.position.y), out var hit, 1.5f, NavMesh.AllAreas);

            if (result == false)
                continue;

            return hit.position;
        }

        return transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, _size.WithY(0.05f));

        Gizmos.DrawSphere(_debugChosen, 1f);
    }

}
