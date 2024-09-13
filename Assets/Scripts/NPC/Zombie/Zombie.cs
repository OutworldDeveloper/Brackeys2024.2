using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

//[SelectionBase]
[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour
{
    public enum ThinkState
    {
        SelectLocation,
        InvestigatePoint,
        Engage,
        Escape,
    }

    public enum WalkState
    {
        Walk,
        Sprint,
        Escaping,
    }

    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _sprintSpeed = 3f;
    [SerializeField] private float _attackDistance = 2.25f;
    [SerializeField] private float _attackLandDistance = 1.5f;
    [SerializeField] private float _baseRotationSpeed = 2f;
    [SerializeField] private float _attackRotationSpeed = 1.6f;

    [SerializeField] private Animator _animator;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound _hitSound;
    [SerializeField] private AudioSource _roarSource;
    [SerializeField] private Sound _roarSound;

    [SerializeField] private Transform _lookTarget;

    [SerializeField] private Sensor _sensor;
    [SerializeField] private SoundsSensor _soundsSensor;

    [SerializeField] private MultiAimConstraint _headTargetingConstraint;
    [SerializeField] private MultiAimConstraint _bodyTargetingConstraint;

    [SerializeField] private AnimationCurve _rotationSlowCurve;

    [SerializeField] private float _timeToLostTarget = 2f;

    private NavMeshAgent _agent;
    private PlayerCharacter _player;
    private ActionsRunner<Zombie> _actionRunner;

    private TimeSince _timeSinceLastThink = TimeSince.Never;
    private TimeSince _timeSinceLastSprint = TimeSince.Never;
    private WalkState _walkState;

    private EnumState<ThinkState> _thinkState = new EnumState<ThinkState>();
    private EnumCall<ThinkState> _thinkCall;

    private LocationInfo[] _locations;

    private Vector3 _defaultTargetLocalPosition;

    private Vector3 _lastKnownPlayerLocation;
    private Vector3 _escapeLocation;

    private Vector3 _investigationPoint;

    private float TargetDistance => Vector3.Distance(transform.position, _player.transform.position);
    private float AngleToPlayer => FlatVector.Angle(transform.forward.Flat(), (_player.transform.position - transform.position).normalized.Flat());

    public void Setup(PlayerCharacter player, LocationInfo[] locations)
    {
        _player = player;
        _sensor.SetTarget(player.gameObject);
        _locations = locations;
    }

    public void Warp(Vector3 location, Vector3 direction)
    {
        GetComponent<NavMeshAgent>().Warp(location);
        transform.forward = direction;
    }

    public void Escape()
    {
        _thinkState.Set(ThinkState.Escape);
    }

    public void InvestigatePoint(Vector3 point)
    {
        _investigationPoint = point;
        _thinkState.Set(ThinkState.InvestigatePoint);
    }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;

        _thinkState.StateStarted.
            AddCallback(ThinkState.SelectLocation, OnRoamingStart).
            AddCallback(ThinkState.Escape, OnEscapeStart);

        _thinkCall = _thinkState.AddCall().
            AddCallback(ThinkState.InvestigatePoint, OnInvestigatePointThink).
            AddCallback(ThinkState.Engage, OnEngageThink).
            AddCallback(ThinkState.Escape, OnEscapeThink);

        _thinkState.StateEnded.
            AddCallback(ThinkState.Engage, OnEngageThinkExit);

        _soundsSensor.Perceived += OnSoundPerceived;
        _sensor.TargetSpotted += OnTargetSpotted;

        _actionRunner = new ActionsRunner<Zombie>(this);

        _defaultTargetLocalPosition = _lookTarget.localPosition;
    }

    private void Start() 
    {
        // This should fixe OnStart not being called
        _thinkState.Set(ThinkState.SelectLocation);
    }

    private void OnSoundPerceived(SoundEvent soundEvent)
    {
        if (_thinkState == ThinkState.Engage || _thinkState == ThinkState.Escape)
            return;

        InvestigatePoint(soundEvent.Position);
    }

    private void OnTargetSpotted()
    {
        if (_thinkState == ThinkState.Escape)
            return;

        _thinkState.Set(ThinkState.Engage);
    }

    private void Update()
    {
        _actionRunner.Update();

        UpdateVisualRotation();

        _animator.SetFloat("velocity", _agent.velocity.magnitude);

        if (_actionRunner.HasActiveAction)
        {
            _agent.speed = 0f;
            return;
        }

        Vector3 direction = (_agent.steeringTarget.Flat() - transform.position.Flat()).normalized;
        float dot = Vector3.Dot(transform.forward.Flat().normalized, direction);
        float speedModifier = Mathf.Clamp01(dot);
        speedModifier = _rotationSlowCurve.Evaluate(speedModifier);

        RotateTo(direction, _baseRotationSpeed);

        _agent.speed = GetSpeed() * speedModifier;

        if (_sensor.IsTargetVisible || _sensor.TimeSinceTargetLost < 1.5f)
            _lastKnownPlayerLocation = _player.transform.position;

        Debug.DrawRay(_lastKnownPlayerLocation, Vector3.up, Color.cyan);

        UpdateThink();
    }

    private void UpdateVisualRotation()
    {
        Vector3 targetLookPosition = ShouldLookAtPlayer() ?
            _player.transform.position + Vector3.up * 1.75f :
            transform.TransformPoint(_defaultTargetLocalPosition);

        _lookTarget.transform.position = Vector3.Lerp(_lookTarget.transform.position, targetLookPosition, 3f * Time.deltaTime);
    }

    private bool ShouldLookAtPlayer()
    {
        return _thinkState == ThinkState.Escape ? false : _sensor.IsTargetVisible;
    }

    private void UpdateThink()
    {
        if (_timeSinceLastThink < 0.1f)
            return;

        _timeSinceLastThink = TimeSince.Now();
        _thinkCall.Execute();
    }

    private void OnRoamingStart()
    {
        InvestigatePoint(SelectRoamingDestination());
    }

    private Vector3 SelectRoamingDestination()
    {
        var randomizer = new WeightedRandom<Vector3>(_locations.Length);

        foreach (var location in _locations)
        {
            float distance = Vector3.Distance(transform.position, location.transform.position);

            if (distance < 5f)
                continue;

            float playerDistance = Vector3.Distance(_player.transform.position, location.transform.position);

            float weight = 1 / (1 + playerDistance);

            randomizer.Add(location.transform.position, weight);
        }

        return randomizer.GetRandomItem();
    }

    private Vector3 SelectRoamingDestinatioWeighted()
    {
        List<Vector3> potentialDestinations = new List<Vector3>(_locations.Length);

        foreach (var roomInfo in _locations)
        {
            // TODO: Path then calculate path distance
            float locationDistance = Vector3.Distance(transform.position, roomInfo.transform.position);

            if (locationDistance < 7f)
                continue;

            potentialDestinations.Add(roomInfo.transform.position);
        }

        return potentialDestinations[Randomize.Index(potentialDestinations.Count)];
    }

    private void OnInvestigatePointThink()
    {
        _agent.stoppingDistance = 0f;
        _agent.SetDestination(_investigationPoint);

        if (_agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            Notification.ShowDebug("Path is invalid, return to roaming");
            _thinkState.Set(ThinkState.SelectLocation);
        }

        if (_agent.pathPending == false &&
            _agent.remainingDistance <= _agent.stoppingDistance &&
            (_agent.hasPath == false || _agent.velocity.sqrMagnitude == 0f))
        {
            Notification.ShowDebug("Investigation point reached, looking around now");
            _thinkState.Set(ThinkState.SelectLocation);
            _actionRunner.Run(new LookAround());
        }
    }

    private void OnEngageThink()
    {
        _walkState = WalkState.Sprint;

        if (_sensor.IsTargetVisible == false && _sensor.TimeSinceTargetLost > _timeToLostTarget)
        {
            InvestigatePoint(_lastKnownPlayerLocation);
            return;
        }

        _agent.stoppingDistance = _agent.radius + 0.5f;
        _agent.SetDestination(_player.transform.position);

        //if (_isSprinting == false && _timeSinceLastSprint > 14f && TargetDistance > 5f && Randomize.Chance(40))
        //{
        //    _timeSinceLastSprint = TimeSince.Now();
        //    //_isSprinting = true;
        //    return;
        //}
        //
        //// Костыль
        //if (_isSprinting == false && TargetDistance > 8.5f && Randomize.Chance(15))
        //{
        //    _timeSinceLastSprint = TimeSince.Now();
        //    //_isSprinting = true;
        //    return;
        //}
        ////
        //
        //if (_isSprinting == true)
        //{
        //    if (_timeSinceLastSprint > 2f || TargetDistance < 3.5f)
        //        _isSprinting = false;
        //}

        TryAttackIfMakesSense();
    }

    private void OnEngageThinkExit()
    {
        _walkState = WalkState.Walk;
    }

    private void OnEscapeStart()
    {
        _actionRunner.Run(new RoarAction());

        _escapeLocation = SelectEscapeLocation();
        _agent.SetDestination(_escapeLocation);

        _walkState = WalkState.Escaping;
    }

    private Vector3 SelectEscapeLocation()
    {
        Vector3 bestLocation = _locations[0].transform.position;
        float bestDistance = 0f;

        foreach (var roomInfo in _locations)
        {
            float distance = Vector3.Distance(_player.transform.position, roomInfo.transform.position);

            if (distance < bestDistance)
                continue;

            bestDistance = distance;
            bestLocation = roomInfo.transform.position;
        }

        return bestLocation;
    }

    private void OnEscapeThink()
    {
        if (_agent.remainingDistance < 1f)
        {
            Destroy(gameObject);
        }
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
        bool isInRange = TargetDistance < _attackDistance;
        bool chanceAttack = TargetDistance < 3.5f && Randomize.Chance(35);

        if (!isInRange && !chanceAttack)
            return false;

        _actionRunner.Run(new AttackAction());
        return true;
    }

    private void RotateTo(Vector3 direction, float speed)
    {
        transform.forward = Vector3.RotateTowards(transform.forward, direction, speed * Time.deltaTime, 0f);
    }

    private float GetSpeed() => _walkState switch
    {
        WalkState.Sprint => _sprintSpeed,
        WalkState.Escaping => 5f,
        _ => _speed,
    };

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;

        Vector3 direction = (_agent.steeringTarget.Flat() - transform.position.Flat()).normalized;
        float dot = Vector3.Dot(transform.forward.Flat().normalized, direction);
        float speedModifier = Mathf.Clamp01(dot);
        speedModifier = _rotationSlowCurve.Evaluate(speedModifier);

        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
            $"Think: {_thinkState.Current}, {_actionRunner}, SM: {speedModifier}");
#endif
    }

    private sealed class RoarAction : ActorAction<Zombie>
    {

        public const float DURATION = 3F;

        public override void OnStarted()
        {
            Owner._animator.Play("roar");
            Owner._roarSound.Play(Owner._roarSource);
        }

        public override bool Execute()
        {
            return TimeSinceStarted > DURATION;
        }

    }

    private sealed class AttackAction : ActorAction<Zombie>
    {

        public const float ATTACK_POINT = 0.3f;
        public const float BACKSWING_DURATION = 1f;

        private bool _isAttackPointReached = false;

        public override void OnStarted()
        {
            Notification.ShowDebug($"Attack started");
            Owner._animator.SetTrigger("attack");
        }

        public override bool Execute()
        {
            Owner.RotateTo((Owner._player.transform.position - Owner.transform.position).normalized, Owner._attackRotationSpeed);

            if (_isAttackPointReached == false && TimeSinceStarted > ATTACK_POINT)
            {
                _isAttackPointReached = true;
                OnAttackPoint();
            }

            return TimeSinceStarted > ATTACK_POINT + BACKSWING_DURATION;
        }

        public override void OnStopped()
        {
            //if (Randomize.Chance(5)) Owner._actionRunner.Run(new RoarAction());
        }

        private void OnAttackPoint()
        {
            float targetAngle = Owner.AngleToPlayer;

            Notification.ShowDebug($"Angle: {targetAngle}");

            if (targetAngle > 70f)
                return;

            float targetDistance = Owner.TargetDistance;

            if (targetDistance > Owner._attackLandDistance)
                return;

            Owner._player.ApplyDamage(1f, (Owner._player.transform.position - Owner.transform.position).normalized);
            Owner._hitSound.Play(Owner._audioSource);
            Owner._player.ApplyModifier(new HurtModifier(), 0.6f);
        }

        private sealed class HurtModifier : CharacterModifier
        {
            public override bool CanJump()
            {
                return false;
            }

            public override float GetSpeedMultiplier()
            {
                return 0.3f;
            }

        }

    }

    private sealed class LookAround : ActorAction<Zombie>
    {

        public const float DURATION = 2F;

        public override void OnStarted()
        {
            Owner._animator.SetTrigger("look_around");
        }

        public override bool Execute()
        {
            return TimeSinceStarted > DURATION;
        }

    }

}

public sealed class ActionsRunner<TOwner>
{

    private readonly TOwner _owner;
    private ActorAction<TOwner> _activeAction;

    public ActionsRunner(TOwner owner)
    {
        _owner = owner;
    }

    public bool HasActiveAction => _activeAction != null;

    public void Run(ActorAction<TOwner> action)
    {
        if (HasActiveAction == true)
        {
            _activeAction.OnStopped();
        }

        _activeAction = action;
        _activeAction.Setup(_owner);
        _activeAction.OnStarted();
    }

    public void Update()
    {
        if (HasActiveAction == false)
            return;

        if (_activeAction.Execute() == false)
            return;

        _activeAction.OnStopped();
        _activeAction = null;
    }

    public override string ToString()
    {
        return $"ActionRunner: {(HasActiveAction ? _activeAction.ToString() : "None")}";
    }

}

public abstract class ActorAction<TOwner>
{

    protected TOwner Owner { get; private set; }
    protected TimeSince TimeSinceStarted { get; private set; }

    public void Setup(TOwner owner)
    {
        Owner = owner;
        TimeSinceStarted = TimeSince.Now();
    }

    public virtual void OnStarted() { }
    public virtual void OnStopped() { }
    public abstract bool Execute();

}
