using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPathPoint : MonoBehaviour
{

    [SerializeField] private AIPathPoint[] _connections = new AIPathPoint[0];
    [SerializeField] private float _width = 1f;

    public int Connections => _connections.Length;
    public AIPathPoint GetConnection(int index) => _connections[index];

    private void OnDrawGizmos()
    {
        foreach (var connection in _connections)
        {
            Vector3 start = transform.position;
            Vector3 end = connection.transform.position;

            // Calculate the midpoint
            Vector3 center = Vector3.Lerp(start, end, 0.5f);

            // Calculate direction and rotation
            Vector3 direction = (end - start).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

            // Save the current matrix
            Matrix4x4 oldMatrix = Gizmos.matrix;

            // Apply rotation and translation to the gizmos matrix
            Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);

            // Draw the cube along the line
            Gizmos.DrawCube(Vector3.zero, new Vector3(_width, 0.05f, Vector3.Distance(start, end)));

        }
    }


}
