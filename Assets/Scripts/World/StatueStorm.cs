using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class StatueStorm : Storm
{

    [SerializeField] private float _duration = 10f;
    [SerializeField] private Power _lightController;
    [SerializeField] private GameObject[] _statues;
    [SerializeField] private float _minDistance = 7f;
    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private AudioSource _dispawnAudioSource;

    private List<GameObject> _activeStatues = new List<GameObject>();

    private void Start()
    {
        DisableStatues();
    }

    protected override void OnStormStarted()
    {
        _lightController.TurnOff();

        const int MAX_STATUES = 1;
        const int MAX_ATTEMPTS = 50;

        int attempts = 0;
        while (_activeStatues.Count < MAX_STATUES && attempts < MAX_ATTEMPTS)
        {
            GameObject statue = _statues[Random.Range(0, _statues.Length)];

            if (IsAcceptableStatue(statue))
            {
                statue.SetActive(true);
                _activeStatues.Add(statue);
            }

            attempts++;
        }
    }

    public override bool UpdateStorm()
    {
        for (int i = _activeStatues.Count - 1; i >= 0; i--)
        {
            GameObject statue = _activeStatues[i];

            float playerDistance = Vector3.Distance(_player.transform.position, statue.transform.position);

            if (playerDistance < 3f)
            {
                statue.gameObject.SetActive(false);
                _activeStatues.RemoveAt(i);
                _dispawnAudioSource.Play();
            }
        }

        if (TimeSinceStarted < _duration)
            return false;

        _lightController.TurnOn();
        DisableStatues();
        return true;
    }

    private void DisableStatues()
    {
        foreach (var statue in _statues)
        {
            statue.SetActive(false);
        }

        _activeStatues.Clear();
    }

    private bool IsAcceptableStatue(GameObject statue)
    {
        float playerDistance = Vector3.Distance(_player.transform.position, statue.transform.position);

        if (playerDistance < 5f)
            return false;

        foreach (var other in _activeStatues)
        {
            float distance = Vector3.Distance(_player.transform.position, statue.transform.position);

            if (distance < _minDistance)
                return false;
        }

        return true;
    }

}
