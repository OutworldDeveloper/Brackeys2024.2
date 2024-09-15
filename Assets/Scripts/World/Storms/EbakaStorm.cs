using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class EbakaStorm : Storm
{

    [SerializeField] private ZombieFactory _zombieFactory;
    [SerializeField] private Transform[] _spawnPoints = Array.Empty<Transform>();
    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private float _forcedRemovalDelay = 25f;
    [SerializeField] private Power _lightsController;

    private Zombie _zombie;
    private bool _isFinished;

    protected override IEnumerable<StormState> CreateSequence()
    {
        yield return new DisablePower(_lightsController);
        yield return new EbakaRoam(_zombieFactory, SelectSpawnPoint, _forcedRemovalDelay, _lightsController);
    }

    private Vector3 SelectSpawnPoint()
    {
        Vector3 bestPoint = _spawnPoints[0].position;
        float bestDistance = 0f;

        foreach (var potentialPoint in _spawnPoints)
        {
            if (NavMesh.SamplePosition(potentialPoint.position, out var hit, 2f, NavMesh.AllAreas) == false)
                continue;

            float playerDistance = Vector3.Distance(potentialPoint.position, _player.transform.position);

            if (playerDistance > bestDistance)
            {
                bestDistance = playerDistance;
                bestPoint = potentialPoint.transform.position;
            }
        }

        return bestPoint;
    }

}

public sealed class EbakaRoam : StormState
{

    private readonly ZombieFactory _factory;
    private readonly Func<Vector3> _spawnPointSelector;
    private readonly float _forcedRemovalDelay;
    private readonly Power _power;

    private Zombie _zombie;
    private bool _isEscaping = false;
    private TimeSince _sinceEscapingStarted;

    private bool ShouldEscape => _power.IsPowerOn;

    public EbakaRoam(ZombieFactory factory, Func<Vector3> spawnPointSelector, float forcedRemovalDelay, Power power)
    {
        _factory = factory;
        _spawnPointSelector = spawnPointSelector;
        _forcedRemovalDelay = forcedRemovalDelay;
        _power = power;
    }

    public override void OnStarted()
    {
        Vector3 point = _spawnPointSelector();
        _zombie = _factory.Spawn(point);
        //Debug.DrawRay(point, Vector3.up * 100f, Color.magenta, 1000000f);
    }

    public override bool Update(TimeSince sinceStart)
    {
        if (_isEscaping == false && ShouldEscape == true)
        {
            _isEscaping = true;
            _zombie.Escape();
            Notification.ShowDebug("Zombie escape!");
            _sinceEscapingStarted = TimeSince.Now();
        }

        if (_isEscaping == true && _sinceEscapingStarted > _forcedRemovalDelay)
        {
            GameObject.Destroy(_zombie.gameObject);
            return true;
        }

        return _zombie == null;
    }

}
