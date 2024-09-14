using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public sealed class MouseManager : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _playerCharacter;
    [SerializeField] private int _spawnChance = 100;
    [SerializeField] private int _maxRats = 3;

    private TimeSince _timeSinceLastCheck;
    private MouseTrap[] _mouseTraps;

    private int _ratsCaught;

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
        //foreach (var mouseTrap in _mouseTraps)
        //    mouseTrap.Disable();
    }

    private void Update()
    {
        if (_timeSinceLastCheck < 3f)
            return;

        _timeSinceLastCheck = TimeSince.Now();

        _mouseTraps.Shuffle();

        foreach (var mouseTrap in _mouseTraps)
        {
            if (_ratsCaught == _maxRats)
                return;

            if (mouseTrap.HasBait == false)
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

            if (Randomize.Chance(_spawnChance) == false)
                continue;

            mouseTrap.SpawnMouse();
            _ratsCaught++;
        }
    }

}

public static class ListExtensions
{

    private static System.Random _rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = _rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

}
