using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Sensor : MonoBehaviour
{

    public event Action TargetSpotted;
    public event Action TargetLost;

    [SerializeField] private float _radius = 13f;
    [SerializeField] private float _detectRadius = 4f;
    [SerializeField] private float _angle = 80f;
    [SerializeField] private LayerMask _occlusionLayers;
    [SerializeField] private float _checkRate = 0.2f;
    [SerializeField] private float _checkOffsetY = 1.5f;

    private TimeSince _timeSinceLastCheck;
    private Transform _target;

    public bool IsTargetVisible { get; private set; }
    public TimeSince TimeSinceTargetSpotted { get; private set; }
    public TimeSince TimeSinceTargetLost { get; private set; }

    public void SetTarget(GameObject target)
    {
        _target = target.transform;
    }

    private void Update()
    {
        if (_timeSinceLastCheck < _checkRate)
            return;

        _timeSinceLastCheck = TimeSince.Now();
        bool wasTargetVisible = IsTargetVisible;
        IsTargetVisible = CheckVisibility();

        if (!wasTargetVisible && IsTargetVisible)
        {
            TimeSinceTargetSpotted = TimeSince.Now();
            TargetSpotted?.Invoke();
        }

        if (wasTargetVisible && !IsTargetVisible)
        {
            TimeSinceTargetLost = TimeSince.Now();
            TargetLost?.Invoke();
        }
    }

    private bool CheckVisibility()
    {
        Vector3 origin = transform.position;
        Vector3 destination = _target.transform.position;
        Vector3 direction = destination - origin;

        float distance = Vector3.Distance(origin, destination);

        float deltaAngle = Vector3.Angle(direction, transform.forward);
        if (deltaAngle > _angle && distance > _detectRadius)
            return false;

        origin.y += _checkOffsetY;
        destination.y = origin.y;

        bool isBlocked = Physics.Linecast(origin, destination, _occlusionLayers);

        Debug.DrawLine(origin, destination, isBlocked ? Color.red : Color.yellow, _checkRate);

        return !isBlocked;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, _radius);
        Gizmos.DrawWireSphere(transform.position, _detectRadius);
        Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(_angle / 2f, Vector3.up) * transform.forward * _radius);
        Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(-_angle / 2f, Vector3.up) * transform.forward * _radius);
    }

}
