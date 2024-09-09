using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

[SelectionBase]
[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour
{

    public enum Action
    {
        None,
        Attack,
        Roar,
        Jump,
        PostJumpDelay,
    }

    public enum ThinkState
    {
        CheckRooms,
        InvestigateSound,
        Follow,
        Engage,
        ReturnHome,
    }

    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _attackDistance = 2.25f;
    [SerializeField] private float _attackLandDistance = 1.5f;

    [SerializeField] private Animator _animator;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound _hitSound;
    [SerializeField] private Sound _roarSound;

    [SerializeField] private Transform _lookTarget;

    [SerializeField] private Collider _playerBlocker;

    [SerializeField] private Sensor _sensor;
    [SerializeField] private SoundsSensor _soundsSensor;

    [SerializeField] private float _toPlayerMltp = 0.3f;
    [SerializeField] private float _forwardMltp = 0.5f;

    [SerializeField] private MultiAimConstraint _headTargetingConstraint;
    [SerializeField] private MultiAimConstraint _bodyTargetingConstraint;

    [SerializeField] private AnimationCurve _jumpCurve;

    private NavMeshAgent _agent;
    private PlayerCharacter _target;

    private EnumState<Action> _currentAction = new EnumState<Action>();
    private EnumCall<Action> _updateCall;

    private bool _isAttackPointReached;

    private TimeSince _timeSinceLastThink = TimeSince.Never;

    private TimeSince _timeSinceLastSprint = TimeSince.Never;
    private bool _isSprinting;

    private TimeSince _timeSinceLastDirectionChange = TimeSince.Never;

    private TimeSince _timeSinceTargetLost;

    private EnumState<ThinkState> _thinkState = new EnumState<ThinkState>();
    private EnumCall<ThinkState> _thinkCall;

    public bool HasTarget => _target != null;
    private float TargetDistance => Vector3.Distance(transform.position, _target.transform.position);
    private float TargetAngle => FlatVector.Angle(transform.forward.Flat(), (_target.transform.position - transform.position).normalized.Flat());

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;

        _currentAction.StateStarted.
            AddCallback(Action.None, OnNoneActionStart).
            AddCallback(Action.Attack, OnAttackActionStart).
            AddCallback(Action.Roar, OnRoarActionStart).
            AddCallback(Action.Jump, OnJumpActionStart);

        _updateCall = _currentAction.AddCall().
            AddCallback(Action.None, OnNoneActionUpdate).
            AddCallback(Action.Attack, OnAttackActionUpdate).
            AddCallback(Action.Roar, OnRoarActionUpdate).
            AddCallback(Action.Jump, OnJumpActionUpdate).
            AddCallback(Action.PostJumpDelay, OnPostJumpDelayActionUpdate);

        _thinkCall = _thinkState.AddCall().
            AddCallback(ThinkState.CheckRooms, OnCheckRoomsThink).
            AddCallback(ThinkState.InvestigateSound, OnInvestigateSoundThink).
            AddCallback(ThinkState.Follow, OnFollowThink).
            AddCallback(ThinkState.Engage, OnEngageThink);

        _thinkState.StateEnded.
            AddCallback(ThinkState.Engage, OnEngageThinkExit);

        _soundsSensor.Perceived += OnSoundPerceived;
    }

    private void Start() { }

    public void SetTarget(PlayerCharacter target)
    {
        _target = target;
        _thinkState.Set(ThinkState.Follow);
    }

    private void OnSoundPerceived(SoundEvent soundEvent)
    {
        if (_thinkState == ThinkState.Follow || _thinkState == ThinkState.Engage || _thinkState == ThinkState.ReturnHome)
            return;

        _thinkState.Set(ThinkState.InvestigateSound);
    }

    private void Update()
    {
        if (HasTarget == true)
        {
            if (_sensor.HasTarget(_target.gameObject) == true)
            {
                _timeSinceTargetLost = TimeSince.Now();
            }

            if (_timeSinceTargetLost > 5f)
            {
                _target = null;
                _thinkState.Set(ThinkState.CheckRooms);
            }
        }

        //float targetWeight = _target == null ? 0f : _isSprinting == true ? 0f : 1f;
        //_headTargetingConstraint.weight = Mathf.Lerp(_headTargetingConstraint.weight, targetWeight, 4f * Time.deltaTime);
        //_bodyTargetingConstraint.weight = Mathf.Lerp(_bodyTargetingConstraint.weight, targetWeight * 0.45f, 4f * Time.deltaTime);

        //
        Vector3 targetLookPosition = _target != null ? 
            _target.transform.position + Vector3.up * 1.75f : 
            transform.position + Vector3.up * 1.75f + Vector3.forward;

        //_lookTarget.transform.position = Vector3.Lerp(_lookTarget.transform.position, targetLookPosition, 3f * Time.deltaTime);
        //

        _updateCall.Execute();

        _agent.speed = CanMove() ? GetSpeed() : 0f;

        if (_currentAction.Current != Action.None)
            return;

        if (_timeSinceLastThink < 0.1f)
            return;

        _timeSinceLastThink = TimeSince.Now();
        _thinkCall.Execute();
    }

    private void OnCheckRoomsThink()
    {
        TryFindTargetAndStartChase();
    }

    private void OnInvestigateSoundThink()
    {
        if (TryFindTargetAndStartChase() == true)
            return;

        if (Vector3.Distance(transform.position, _soundsSensor.LastEvent.Position) > 1f)
        {
            _agent.stoppingDistance = 0f;
            _agent.SetDestination(_soundsSensor.LastEvent.Position);
        }
        else
        {
            _thinkState.Set(ThinkState.CheckRooms);
        }
    }

    private void OnFollowThink()
    {
        if (TryAttackIfMakesSense() == true)
            return;

        if (TryEngage() == true)
            return;

        _agent.stoppingDistance = 1f;
        _agent.SetDestination(_target.transform.position);
    }

    private void OnEngageThink()
    {
        _agent.stoppingDistance = _agent.radius + 0.5f;
        _agent.SetDestination(_target.transform.position);

        if (_isSprinting == false && _timeSinceLastSprint > 14f && TargetDistance > 5f && Randomize.Chance(40))
        {
            _timeSinceLastSprint = TimeSince.Now();
            _isSprinting = true;
            return;
        }

        // Костыль
        if (_isSprinting == false && TargetDistance > 8.5f && Randomize.Chance(15))
        {
            _timeSinceLastSprint = TimeSince.Now();
            _isSprinting = true;
            return;
        }
        //

        if (_isSprinting == true)
        {
            if (_timeSinceLastSprint > 2f || TargetDistance < 3.5f)
                _isSprinting = false;
        }

        Try.Any(TryAttackIfMakesSense, TryJump);
    }

    private void OnEngageThinkExit()
    {
        _isSprinting = false;
    }

    private bool TryEngage()
    {
        if (Randomize.Chance(25) == true)
        {
            _thinkState.Set(ThinkState.Engage);
            return true;
        }

        if (TargetDistance < 3.5f &&
            _thinkState.TimeSinceLastChange > 0.4f)
        {
            _thinkState.Set(ThinkState.Engage);
            return true;
        }

        return false;
    }

    private bool TryAttackIfMakesSense()
    {
        if (TargetDistance < _attackDistance)
        {
            _currentAction.Set(Action.Attack);
            return true;
        }

        if (TargetDistance < 3.5f && Randomize.Chance(35))
        {
                _currentAction.Set(Action.Attack);
            return true;
        }

        return false;
    }

    private Vector3 _jumpPosition;

    private bool TryJump()
    {
        if (Randomize.Chance(20) == false)
            return false;

        int rays = 16;

        Vector3 bestSpot = transform.position;
        float bestScore = float.NegativeInfinity;
        float distance = 0f;

        for (int i = 0; i < rays; i++)
        {
            Vector3 rayDirection = Quaternion.AngleAxis(i * (360 / 16), Vector3.up) * transform.forward;

            NavMesh.Raycast(transform.position, transform.position + rayDirection * 3f, out NavMeshHit hit, _agent.areaMask);

            Debug.DrawRay(transform.position, rayDirection * hit.distance, hit.hit ? Color.red : Color.green, 1f);

            float score = hit.distance / 3f;

            score +=
                (Vector3.Dot(rayDirection, (_target.transform.position - transform.position).normalized) + 1f) / 2
                * _toPlayerMltp;

            score += Randomize.Float(0.0f, 0.4f); // Not sure

            if (score > bestScore)
            {
                bestScore = score;
                bestSpot = transform.position + rayDirection * hit.distance;
                distance = hit.distance;
            }
        }

        if (bestScore < 0f)
            return false;

        if (distance < 2.5f)
            return false;

        _jumpPosition = bestSpot;
        _currentAction.Set(Action.Jump);
        return true;
    }

    private bool TryFindTargetAndStartChase()
    {
        if (_sensor.HasTargets == false)
            return false;

        PlayerCharacter target = _sensor.GetFirstTarget<PlayerCharacter>();

        if (target == null)
            return false;

        SetTarget(target);
        return true;
    }

    private void OnNoneActionStart()
    {
        _agent.ResetPath();
        _isSprinting = false;
    }

    private void OnNoneActionUpdate()
    {
        RotateTo(_agent.velocity, 5f);

        _animator.SetFloat("velocity", _agent.velocity.magnitude);

        if (Vector3.Angle(transform.forward, _agent.velocity) > 40f)
            _agent.speed = 0f;
    }

    private void OnAttackActionStart()
    {
        _isAttackPointReached = false;
        _animator.CrossFade("attack", 0.1f);
    }

    private void OnAttackActionUpdate()
    {
        RotateTo((_target.transform.position - transform.position).normalized, 1.6f);

        if (_isAttackPointReached == false && _currentAction.TimeSinceLastChange > 1.1f / 1.5f)
        {
            _isAttackPointReached = true;
            OnAttackPoint();
        }

        if (_currentAction.TimeSinceLastChange > 2.15f / 1.5f)
        {
            if (Randomize.Int(0, 5) == 0)
                _currentAction.Set(Action.Roar);
            else
                _currentAction.Set(Action.None);
        }
    }

    private void OnAttackPoint()
    {
        float angle = FlatVector.Angle(
            transform.forward.Flat(),
            (_target.transform.position - transform.position).normalized.Flat());

        Notification.ShowDebug($"Angle: {angle}");

        if (angle > 70f)
            return;

        float targetDistance = Vector3.Distance(transform.position, _target.transform.position);

        if (targetDistance > _attackLandDistance)
            return;

        _target.ApplyDamage(1f, (_target.transform.position - transform.position).normalized);

        _hitSound.Play(_audioSource);
    }

    private void OnHurtActionStart()
    {
        _animator.Play("hurt");
    }

    private void OnHurtActionUpdate()
    {
        if (_currentAction.TimeSinceLastChange < 1.75f)
            return;

        _currentAction.Set(Action.None);
    }

    private void OnRoarActionStart()
    {
        _animator.Play("roar");
        _roarSound.Play(_audioSource);
    }

    private void OnRoarActionUpdate()
    {
        if (_currentAction.TimeSinceLastChange < 2.6f)
            return;

        _currentAction.Set(Action.None);
    }

    private Vector3 _jumpStartPosition;
    private bool _almostLanded;

    private void OnJumpActionStart()
    {
        _agent.ResetPath();
        _isSprinting = false;
        _animator.CrossFade("jump_init", 0.1f, 0);

        _jumpStartPosition = transform.position;

        _almostLanded = false;
    }

    private void OnJumpActionUpdate()
    {
        RotateTo(_jumpPosition - transform.position, 5f);

        const float prepDuration = 0.95f;
        float jumpDuration = Vector3.Distance(_jumpStartPosition, _jumpPosition) * 0.175f;

        if (_currentAction.TimeSinceLastChange > prepDuration + jumpDuration)
        {
            _currentAction.Set(Action.PostJumpDelay);
            return;
        }

        if (_currentAction.TimeSinceLastChange < prepDuration)
            return;

        if (_almostLanded == false && _currentAction.TimeSinceLastChange > prepDuration + jumpDuration - 0.3f)
        {
            _almostLanded = true;
            _animator.CrossFade("jump_land", 0.1f, 0);
        }

        float t = (_currentAction.TimeSinceLastChange - prepDuration) / jumpDuration;
        t = Mathf.SmoothStep(0f, 1f, t);
        //t = _jumpCurve.Evaluate(t);
        Vector3 position = Vector3.Lerp(_jumpStartPosition, _jumpPosition, t);
        _agent.Move(position - transform.position);
    }

    private void OnPostJumpDelayActionUpdate()
    {
        const float duration = 0.8f;

        if (_currentAction.TimeSinceLastChange < duration)
            return;

        _currentAction.Set(Action.None);
    }

    private void OnSleepingActionStart()
    {
        _agent.enabled = false;
        _playerBlocker.enabled = false;
        _animator.Play("sleeping");
    }

    private void OnWakingUpActionStart()
    {
        _agent.enabled = true;
        _playerBlocker.enabled = true;
        _animator.CrossFade("waking", 0.1f);
    }

    private void OnWakingUpActionUpdate()
    {
        if (_currentAction.TimeSinceLastChange > 1f)
            _currentAction.Set(Action.None);
    }

    private void RotateTo(Vector3 direction, float speed)
    {
        transform.forward = Vector3.RotateTowards(transform.forward, direction, speed * Time.deltaTime, 0f);
    }

    private bool CanMove()
    {
        return _currentAction.Current == Action.None;
    }

    private float GetSpeed()
    {
        if (_thinkState == ThinkState.InvestigateSound || 
            _thinkState == ThinkState.Follow)
            return 0.75f;

        return _isSprinting ? 5f : _speed;
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"{_thinkState.Current}");
#endif
    }

}

public abstract class LazySingleton<T> : MonoBehaviour where T : MonoBehaviour
{

    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject(nameof(T)).AddComponent<T>();
            }

            return _instance;
        }
    }

}

public static class Try
{
    public static bool This(params Func<bool>[] stack)
    {
        for (int i = 0; i < stack.Length; i++)
        {
            if (stack[i].Invoke() == true)
                return true;
        }

        return false;
    }

    public static bool Any(Func<bool> a, Func<bool> b)
    {
        if (a.Invoke() == true)
            return true;

        return b.Invoke();
    }

    public static bool Any(Func<bool> a, Func<bool> b, Func<bool> c)
    {
        if (Any(a, b) == true)
            return true;

        return c.Invoke();
    }

    public static bool Any(Func<bool> a, Func<bool> b, Func<bool> c, Func<bool> d)
    {
        if (Any(a, b, c) == true)
            return true;

        return d.Invoke();
    }

    public static bool Any(Func<bool> a, Func<bool> b, Func<bool> c, Func<bool> d, Func<bool> e)
    {
        if (Any(a, b, c, d) == true)
            return true;

        return e.Invoke();
    }

}
