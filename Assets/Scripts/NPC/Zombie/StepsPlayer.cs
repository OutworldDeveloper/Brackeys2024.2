using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class StepsPlayer : MonoBehaviour
{

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound _heavySound;
    [SerializeField] private Sound _lightSound;

    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float _shakeStrenght = 5f;
    [SerializeField] private float _lightShakeStrenght = 2f;
    [SerializeField] private float _maxDistance;
    [SerializeField] private AnimationCurve _strenghtCurve;

    private Collider[] _results = new Collider[1];

    public void OnStep(int leg)
    {
        bool isHeavyStep = leg == 0 || leg == 1;

        Sound sound = isHeavyStep ? _lightSound : _heavySound;
        _heavySound.Play(_audioSource);

        int count = Physics.OverlapSphereNonAlloc(transform.position, _maxDistance, _results, _layerMask);

        if (count == 0)
            return;

        float distance = Vector3.Distance(transform.position, _results[0].transform.position);

        if (_results[0].gameObject.TryGetComponent(out PlayerCharacter character) == false)
            return;

        float shakeStrenght = (isHeavyStep ? _shakeStrenght : _lightShakeStrenght) * _strenghtCurve.Evaluate(1 - (distance / _maxDistance));
        character.ApplyShake(shakeStrenght);
    }

}
