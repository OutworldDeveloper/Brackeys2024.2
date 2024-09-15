using UnityEngine;

public sealed class Cage : MonoBehaviour
{

    [SerializeField] private Collider _collider;
    [SerializeField] private Door _door;

    public bool IsOpen { get; private set; }

    public void Open()
    {
        if (IsOpen)
            return;

        IsOpen = true;
        _collider.enabled = false;
        _door.Open();
    }

}