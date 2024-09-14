using UnityEngine;

public sealed class Cage : MonoBehaviour
{

    [SerializeField] private Collider _collider;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound _openSound;

    public bool IsOpen { get; private set; }

    public void Open()
    {
        if (IsOpen)
            return;

        IsOpen = true;
        _collider.enabled = false;
        _openSound.Play(_audioSource);
    }

}