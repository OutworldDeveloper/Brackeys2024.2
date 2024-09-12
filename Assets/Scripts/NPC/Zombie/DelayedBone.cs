using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedBone : MonoBehaviour
{

    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _rotationSpeed = 10f;

    private Vector3 _offset;
    private Vector3 _currentPosition;
    private Vector3 DesiredPosition => transform.parent.TransformPoint(_offset);

    private Quaternion _rotationOffset;
    private Quaternion _currentRotation;
    private Quaternion DesiredRotation => transform.parent.rotation * _rotationOffset;

    private void Start()
    {
        _offset = transform.localPosition;
        _currentPosition = transform.position;

        _rotationOffset = Quaternion.Inverse(transform.parent.rotation) * transform.rotation;
        _currentRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        _currentPosition = Vector3.Lerp(_currentPosition, DesiredPosition, Time.deltaTime * _speed);
        transform.position = _currentPosition;

        _currentRotation = Quaternion.Slerp(_currentRotation, DesiredRotation, Time.deltaTime * _rotationSpeed);
        transform.rotation = _currentRotation;
    }

}
