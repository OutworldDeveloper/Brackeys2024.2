using UnityEngine;

public sealed class CoffinKnockKnock : MonoBehaviour
{

    [SerializeField] private PlayerTrigger _roomTrigger;
    [SerializeField] private Sound _knockSound;
    [SerializeField] private AudioSource _source;
    [SerializeField] private MinMax<float> _knockDelay;
    [SerializeField] private Door _door;

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

            if (_door.IsOpen || _door.IsAnimating)
                return;

            if (Water.Level > transform.position.y + 0.3f)
                return;

            _knockSound.Play(_source);
        }
    }

}