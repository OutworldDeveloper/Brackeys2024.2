using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ItemSpawner : MonoBehaviour
{

    [SerializeField] private Item _item;
    [SerializeField] private bool _spawnOnStart = true;

    private void Start()
    {
        if (_spawnOnStart)
            Spawn();
    }

    public ItemPickup Spawn()
    {
        return _item.Model.Instantiate(transform.position, transform.rotation).
            EnableCollision(true).
            EnableGlow(true).gameObject.AddComponent<ItemPickup>().Setup(_item);
    }

    private List<Renderer> _renderers = new();
    private List<MeshFilter> _meshFilters = new();

    private void OnDrawGizmos()
    {
        if (_item == null)
            return;

        Gizmos.color = _spawnOnStart ? Color.green : Color.yellow;

        // Get all components from the model, if not already done.
        _item.Model.Asset.GetComponentsInChildren(false, _renderers);
        _item.Model.Asset.GetComponentsInChildren(false, _meshFilters);

        // Iterate through all meshFilters and render each mesh.
        for (int i = 0; i < _meshFilters.Count; i++)
        {
            var meshFilter = _meshFilters[i];
            var mesh = meshFilter.sharedMesh;

            // Check if the mesh and its transform are valid, otherwise skip.
            if (mesh == null || meshFilter.transform == null)
                continue;

            // Get the position, rotation, and scale of the mesh relative to the world.
            // Create a matrix that places the mesh in the position, rotation, and scale of the current object.
            Matrix4x4 adjustedMatrix = transform.localToWorldMatrix * Matrix4x4.TRS(
                meshFilter.transform.localPosition,
                meshFilter.transform.localRotation,
                meshFilter.transform.localScale
            );

            // Draw the mesh using the correct transformation matrix.
            Gizmos.matrix = adjustedMatrix;
            Gizmos.DrawMesh(mesh, 0);
        }
    }

}
