using UnityEngine;

public sealed class MouseManager : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _playerCharacter;

    private TimeSince _timeSinceLastCheck;
    private MouseTrap[] _mouseTraps;

    private void Awake()
    {
        _mouseTraps = FindObjectsOfType<MouseTrap>();
    }

    private void Start()
    {
        foreach (var mouseTrap in _mouseTraps)
            mouseTrap.RatTaken += OnRatTaken;
    }

    private void OnRatTaken()
    {
        foreach (var mouseTrap in _mouseTraps)
            mouseTrap.Disable();
    }

    private void Update()
    {
        if (_timeSinceLastCheck < 3f)
            return;

        _timeSinceLastCheck = TimeSince.Now();

        foreach (var mouseTrap in _mouseTraps)
        {
            if (mouseTrap.HasCheese == false)
                continue;

            float distance = Vector3.Distance(mouseTrap.transform.position, _playerCharacter.transform.position);

            if (distance < 5f)
                continue;

            FlatVector playerDirection =
                (mouseTrap.transform.position.Flat() - _playerCharacter.transform.position.Flat()).normalized;

            float dot = Vector3.Dot(playerDirection, _playerCharacter.transform.forward.Flat().normalized);
            bool ignoreDot = distance > 15f;

            if (dot > 0 && ignoreDot == false)
                continue;

            if (Randomize.Chance(65))
            {
                mouseTrap.SpawnMouse();
                return;
            }
        }
    }

}
