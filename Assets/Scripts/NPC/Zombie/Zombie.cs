using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

[SelectionBase]
[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour
{
    public enum ThinkState
    {
        None,
        Roaming,
        InvestigateSound,
        Follow,
        Engage,
        Escape,
    }

    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _attackDistance = 2.25f;
    [SerializeField] private float _attackLandDistance = 1.5f;

    [SerializeField] private Animator _animator;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound _hitSound;
    [SerializeField] private AudioSource _roarSource;
    [SerializeField] private Sound _roarSound;

    [SerializeField] private Transform _lookTarget;

    [SerializeField] private Sensor _sensor;
    [SerializeField] private SoundsSensor _soundsSensor;

    [SerializeField] private float _toPlayerMltp = 0.3f;
    [SerializeField] private float _forwardMltp = 0.5f;

    [SerializeField] private MultiAimConstraint _headTargetingConstraint;
    [SerializeField] private MultiAimConstraint _bodyTargetingConstraint;

    [SerializeField] private AnimationCurve _jumpCurve;

    private NavMeshAgent _agent;
    private PlayerCharacter _player;

    private TimeSince _timeSinceLastThink = TimeSince.Never;
    private TimeSince _timeSinceLastSprint = TimeSince.Never;
    private bool _isSprinting;

    private EnumState<ThinkState> _thinkState = new EnumState<ThinkState>();
    private EnumCall<ThinkState> _thinkCall;

    private RoomInfo[] _rooms;

    private ActionsRunner<Zombie> _actionRunner;

    private float TargetDistance => Vector3.Distance(transform.position, _player.transform.position);
    private float AngleToPlayer => FlatVector.Angle(transform.forward.Flat(), (_player.transform.position - transform.position).normalized.Flat());

    public void Setup(PlayerCharacter player, RoomInfo[] rooms)
    {
        _player = player;
        _sensor.SetTarget(player.gameObject);
        _rooms = rooms;
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

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;

        _thinkState.StateStarted.
            AddCallback(ThinkState.Roaming, OnRoamingStart).
            AddCallback(ThinkState.Escape, OnEscapeStart);

        _thinkCall = _thinkState.AddCall().
            AddCallback(ThinkState.Roaming, OnRoamingThink).
            AddCallback(ThinkState.InvestigateSound, OnInvestigateSoundThink).
            AddCallback(ThinkState.Follow, OnFollowThink).
            AddCallback(ThinkState.Engage, OnEngageThink).
            AddCallback(ThinkState.Escape, OnEscapeThink);

        _thinkState.StateEnded.
            AddCallback(ThinkState.Engage, OnEngageThinkExit);

        _soundsSensor.Perceived += OnSoundPerceived;
        _sensor.TargetSpotted += OnTargetSpotted;
        _sensor.TargetLost += OnTargetLost;

        _actionRunner = new ActionsRunner<Zombie>(this);
    }

    private void Start() 
    {
        // This should fixe OnStart not being called
        _thinkState.Set(ThinkState.Roaming);
    }

    private void OnSoundPerceived(SoundEvent soundEvent)
    {
        if (_thinkState == ThinkState.Follow || _thinkState == ThinkState.Engage || _thinkState == ThinkState.Escape)
            return;

        _thinkState.Set(ThinkState.InvestigateSound);
    }

    private void OnTargetSpotted()
    {
        if (_thinkState == ThinkState.Escape || _thinkState == ThinkState.Engage)
            return;

        _thinkState.Set(ThinkState.Follow);
    }

    private void OnTargetLost()
    {
        if (_thinkState == ThinkState.Escape)
            return;

        _thinkState.Set(ThinkState.Roaming);
    }

    private void Update()
    {
        _actionRunner.Update();

        //float targetWeight = _target == null ? 0f : _isSprinting == true ? 0f : 1f;
        //_headTargetingConstraint.weight = Mathf.Lerp(_headTargetingConstraint.weight, targetWeight, 4f * Time.deltaTime);
        //_bodyTargetingConstraint.weight = Mathf.Lerp(_bodyTargetingConstraint.weight, targetWeight * 0.45f, 4f * Time.deltaTime);

        //
        //Vector3 targetLookPosition = _target != null ? 
        //    _target.transform.position + Vector3.up * 1.75f : 
        //    transform.position + Vector3.up * 1.75f + Vector3.forward;

        //_lookTarget.transform.position = Vector3.Lerp(_lookTarget.transform.position, targetLookPosition, 3f * Time.deltaTime);

        _animator.SetFloat("velocity", _agent.velocity.magnitude);

        _agent.speed = _actionRunner.HasActiveAction ? 0f : GetSpeed();

        if (_actionRunner.HasActiveAction)
            return;

        RotateTo(_agent.velocity, 5f);

        float dot = Vector3.Dot(transform.forward, _agent.velocity);
        float speedModifier = Mathf.Pow(dot, 2);
        speedModifier = Mathf.Clamp(speedModifier, 0.7f, 1f);
        _agent.speed = _agent.speed * speedModifier;

        UpdateThink();
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
        Debug.Log("OnRoamingStart");
        var roamingDestination = SelectRoamingDestination();
        _agent.SetDestination(roamingDestination);
        Debug.Log(roamingDestination);
    }

    private Vector3 SelectRoamingDestination()
    {
        List<Vector3> potentialDestinations = new List<Vector3>(_rooms.Length);

        foreach (var roomInfo in _rooms)
        {
            // TODO: Path then calculate path distance
            float locationDistance = Vector3.Distance(transform.position, roomInfo.transform.position);

            if (locationDistance < 3f)
                continue;

            potentialDestinations.Add(roomInfo.transform.position);
        }

        return potentialDestinations[Randomize.Index(potentialDestinations.Count)];
    }

    private void OnRoamingThink()
    {
        if (Randomize.Chance(600))
            _actionRunner.Run(new RoarAction());

        if (Vector3.Distance(transform.position, _agent.destination) > 1.5f)
            return;

        OnRoamingStart();
        _actionRunner.Run(new LookAround());
    }

    private void OnInvestigateSoundThink()
    {
        _agent.stoppingDistance = 0f;
        _agent.SetDestination(_soundsSensor.LastEvent.Position);
        Debug.DrawRay(_agent.destination, Vector3.up, Color.magenta);

        if (Vector3.Distance(transform.position, _agent.destination) < 1f)
        {
            _thinkState.Set(ThinkState.Roaming);
            _actionRunner.Run(new LookAround());
        }
    }

    private void OnFollowThink()
    {
        if (Randomize.Chance(50) == true)
        {
            _actionRunner.Run(new RoarAction());
            return;
        }

        if (TryAttackIfMakesSense() == true)
            return;

        if (TryEngage() == true)
            return;

        _agent.stoppingDistance = 1f;
        _agent.SetDestination(_player.transform.position);
    }

    private void OnEngageThink()
    {
        _agent.stoppingDistance = _agent.radius + 0.5f;
        _agent.SetDestination(_player.transform.position);

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

    private Vector3 _escapeLocation;

    private void OnEscapeStart()
    {
        _actionRunner.Run(new RoarAction());

        _escapeLocation = SelectEscapeLocation();
        _agent.SetDestination(_escapeLocation);

        _isSprinting = true;
    }

    private Vector3 SelectEscapeLocation()
    {
        Vector3 bestLocation = _rooms[0].transform.position;
        float bestDistance = 0f;

        foreach (var roomInfo in _rooms)
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

    private bool TryJump()
    {
        return false;
        /*
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
                (Vector3.Dot(rayDirection, (_player.transform.position - transform.position).normalized) + 1f) / 2
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
        */
    }

    private void OnNoneActionStart()
    {
        _agent.ResetPath();
        _isSprinting = false;
    }

    private void RotateTo(Vector3 direction, float speed)
    {
        transform.forward = Vector3.RotateTowards(transform.forward, direction, speed * Time.deltaTime, 0f);
    }

    private float GetSpeed()
    {
        if (_thinkState == ThinkState.InvestigateSound || 
            _thinkState == ThinkState.Follow ||
            _thinkState == ThinkState.Roaming)
            return 0.75f;

        return _isSprinting ? 5f : _speed;
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
            $"Think: {_thinkState.Current}, {_actionRunner}");
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

        public const float ATTACK_POINT = 0.73333333333f;
        public const float BACKSWING_DURATION = 1.43333333333f;

        private bool _isAttackPointReached = false;

        public override void OnStarted()
        {
            Owner._animator.CrossFade("attack", 0.1f);
        }

        public override bool Execute()
        {
            Owner.RotateTo((Owner._player.transform.position - Owner.transform.position).normalized, 1.6f);

            if (_isAttackPointReached == false && TimeSinceStarted > ATTACK_POINT)
            {
                _isAttackPointReached = true;
                OnAttackPoint();
            }

            return TimeSinceStarted > ATTACK_POINT + BACKSWING_DURATION;
        }

        public override void OnStopped()
        {
            if (Randomize.Chance(5)) Owner._actionRunner.Run(new RoarAction());
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
        }

    }

    private sealed class Jump : ActorAction<Zombie>
    {

        public const float PREP_DURATION = 0.95f;
        public const float POST_JUMP_DURATION = 0.8f; // QUEUE ACTION INSTEAD

        private readonly Vector3 _targetPosition;
        private Vector3 _jumpStartPosition;
        private float _jumpDuration;
        private bool _almostLanded;

        public Jump(Vector3 position)
        {
            _targetPosition = position; 
        }

        public override void OnStarted()
        {
            Owner._animator.CrossFade("jump_init", 0.1f, 0);

            _jumpStartPosition = Owner.transform.position;
            _jumpDuration = Vector3.Distance(_jumpStartPosition, _targetPosition) * 0.175f;
        }

        public override bool Execute()
        {
            Owner.RotateTo(_targetPosition - Owner.transform.position, 5f);

            if (_almostLanded == false)
                return false;

            if (TimeSinceStarted > PREP_DURATION + _jumpDuration - 0.3f)
            {
                _almostLanded = true;
                Owner._animator.CrossFade("jump_land", 0.1f, 0);
            }

            float t = (TimeSinceStarted - PREP_DURATION) / _jumpDuration;
            t = Mathf.SmoothStep(0f, 1f, t);
            //t = _jumpCurve.Evaluate(t);
            Vector3 position = Vector3.Lerp(_jumpStartPosition, _targetPosition, t);
            Owner._agent.Move(position - Owner.transform.position);

            return TimeSinceStarted > _jumpDuration;
        }

    }

    private sealed class LookAround : ActorAction<Zombie>
    {

        public const float DURATION = 2F;

        public override void OnStarted()
        {
            Owner._animator.Play("roar");
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

public sealed class Thinker<TEnum, TOwner> where TEnum : struct, Enum
{
 
    private readonly Dictionary<TEnum, AIState<TEnum, TOwner>> _bindings = new Dictionary<TEnum, AIState<TEnum, TOwner>>();
    private readonly TOwner _owner;
    private bool _isFirstThink;

    public Thinker(TOwner owner)
    {
        _owner = owner;
    }

    public TEnum CurrentKey { get; private set; }
    public AIState<TEnum, TOwner> CurrentState => _bindings[CurrentKey];

    public Thinker<TEnum, TOwner> RegisterState(TEnum key, AIState<TEnum, TOwner> state)
    {
        if (_bindings.ContainsKey(key))
            throw new Exception($"Thinker already contains binding for {key}.");
        _bindings.Add(key, state);
        return this;
    }

    public void Think()
    {
        if (_isFirstThink)
        {
            _isFirstThink = false;
            CurrentState.OnEnterState(_owner);
        }

        var previousState = CurrentState;
        var previousKey = CurrentKey;
        CurrentKey = CurrentState != null ?
                 CurrentState.Think(_owner) :
                 CurrentKey;

        if (!Equals(CurrentKey, previousKey))
            CurrentState?.OnExitState(_owner);
    }

}

public abstract class AIState<TEnum, TOwner> where TEnum : struct, Enum
{
    public virtual void OnEnterState(TOwner owner) { }
    public virtual void OnExitState(TOwner owner) { }
    public abstract TEnum Think(TOwner owner);

}

public sealed class AITaskExecuter<TOwner>
{

    public delegate AITask<TOwner> TaskGetter();

    private readonly TOwner _owner;
    private readonly TaskGetter _taskGetter;
    private AITask<TOwner> _currentTask;
    private TimeSince _timeSinceLastThink = TimeSince.Never;

    public AITaskExecuter(TOwner owner, TaskGetter taskGetter)
    {
        _owner = owner;
        _taskGetter = taskGetter;
    }

    public bool HasTask => _currentTask != null;

    public void Run(AITask<TOwner> task)
    {
        _currentTask = task;
        _currentTask.Setup(_owner);
    }

    public void Update()
    {
        if (_timeSinceLastThink < 0.2)
            return;

        _timeSinceLastThink = TimeSince.Now();

        if (HasTask == false)
            _currentTask = _taskGetter();

        if (HasTask == false)
            return;

        _currentTask.Think();

        if (_currentTask.IsComplete())
            _currentTask = null;
    }

}

public abstract class AITask<TOwner>
{
    protected TOwner Owner { get; private set; }

    public void Setup(TOwner owner)
    {
        Owner = owner;
    }

    public virtual float GetSpeedMultiplier() => 1f;
    public abstract bool IsComplete();
    public abstract Vector3 GetLocation();
    public virtual void Think() { }

}

public sealed class InvestigateLocation : AITask<NewZombie>
{

    private readonly Vector3 _location;

    public InvestigateLocation(Vector3 location)
    {
        _location = location;
    }

    public override Vector3 GetLocation()
    {
        return _location;
    }

    public override bool IsComplete()
    {
        return Vector3.Distance(Owner.transform.position, _location) < 0.2f;
    }

}

public sealed class Follow : AITask<NewZombie>
{

    private readonly PlayerCharacter _target;

    public override Vector3 GetLocation()
    {
        return _target.transform.position;
    }

    public override bool IsComplete()
    {
        return _target.IsDead;
    }

    public override void Think()
    {
        Owner.TryAttack();
    }

}

public sealed class NewZombie : MonoBehaviour
{

    [SerializeField] private Sensor _visualSensor;
    [SerializeField] private SoundsSensor _soundSensor;

    private AITaskExecuter<NewZombie> _taskExecuter;

    private PlayerCharacter _player;
    private RoomInfo[] _rooms;

    private float TargetDistance => Vector3.Distance(transform.position, _player.transform.position);
    private float TargetAngle => FlatVector.Angle(transform.forward.Flat(), (_player.transform.position - transform.position).normalized.Flat());

    public void Setup(PlayerCharacter player, RoomInfo[] rooms)
    {
        _player = player;
        _visualSensor.SetTarget(player.gameObject);
        _rooms = rooms;
    }

    private void Awake()
    {
        _visualSensor.TargetSpotted += OnTargetSpotted;
        _soundSensor.Perceived += OnSoundPerceived;
        _taskExecuter = new AITaskExecuter<NewZombie>(this, GetNextTask);
    }

    private void OnTargetSpotted()
    {
        throw new NotImplementedException();
    }

    private void Update()
    {
        _taskExecuter.Update();
    }

    public void TryAttack()
    {

    }

    private void OnSoundPerceived(SoundEvent sound)
    {
        _taskExecuter.Run(new InvestigateLocation(sound.Position));
    }

    private AITask<NewZombie> GetNextTask()
    {
        //return new InvestigateLocation();
        return null;
    }

}
