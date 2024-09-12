using UnityEngine;

public sealed class CoffinKnockKnock : MonoBehaviour
{

    [SerializeField] private PlayerTrigger _roomTrigger;
    [SerializeField] private Sound _knockSound;
    [SerializeField] private AudioSource _source;
    [SerializeField] private MinMax<float> _knockDelay;

    private TimeUntil _timeUntilNextKnockAttempt;

    private void Start()
    {
        _timeUntilNextKnockAttempt = new TimeUntil(Time.time + Randomize.Float(_knockDelay));
    }

    private void Update()
    {
        if (_timeUntilNextKnockAttempt < 0)
        {
            _timeUntilNextKnockAttempt = new TimeUntil(Time.time + Randomize.Float(_knockDelay));

            if (_roomTrigger.HasPlayerInside)
                return;

            _knockSound.Play(_source);
        }
    }

}