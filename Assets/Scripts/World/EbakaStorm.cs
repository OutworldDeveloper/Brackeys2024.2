﻿using System;
using UnityEngine;

public sealed class EbakaStorm : Storm
{

    [SerializeField] private ZombieFactory _zombieFactory;
    [SerializeField] private Transform[] _spawnPoints = Array.Empty<Transform>();
    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private float _minDuration = 90f;

    private Zombie _zombie;
    private bool _isFinished;

    protected override void OnStormStarted()
    {
        _zombie = _zombieFactory.Spawn(SelectSpawnPoint(), Vector3.forward);
    }

    public override bool UpdateStorm()
    {
        if (_isFinished == false && TimeSinceStarted > _minDuration)
        {
            _isFinished = true;
            _zombie.Escape();
            Notification.ShowDebug("Zombie escape!");
        }

        return _zombie == null;
    }

    private Vector3 SelectSpawnPoint()
    {
        Vector3 bestPoint = _spawnPoints[0].position;
        float bestDistance = 0f;

        foreach (var potentialPoint in _spawnPoints)
        {
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