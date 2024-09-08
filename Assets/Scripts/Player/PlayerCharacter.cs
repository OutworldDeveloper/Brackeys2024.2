using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(PlayerInteraction))]
public sealed class PlayerCharacter : Pawn
{

    public event Action Damaged;
    public event Action Died;

    [SerializeField] private Transform _head;
    [SerializeField] private PlayerInteraction _interactor;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private FloatParameter _mouseSensitivity;
    [SerializeField] private float _speed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _maxHealth = 5f;
    [SerializeField] private bool _allowJumping;
    [SerializeField] private bool _allowCrouching;
    [SerializeField] private float _crouchedCameraHeight = -0.75f;
    [SerializeField] private AnimationCurve _crouchAnimation;
    [SerializeField] private AnimationCurve _uncrouchAnimation;
    [SerializeField] private float _crouchAnimationDuration = 0.45f;
    [SerializeField] private LayerMask _uncrouchLayerMask;
    [SerializeField] private float _crouchedControllerSize = 1.25f;
    [SerializeField] private float _fieldOfView = 70f;

    [SerializeField] private KeyCode[] _interactionKeys;

    [SerializeField] private float _kneeOffset = 0.75f;
    [SerializeField] private float _swimPointOffset = 1.5f;

    private CharacterController _controller;
    private Vector3 _velocityXZ;
    private float _velocityY;
    private TimeSince _timeSinceLastDeath = TimeSince.Never;

    private readonly List<CharacterModifier> _modifiers = new List<CharacterModifier>();
    private TimeSince _timeSinceLastDamage = TimeSince.Never;
    private PlayerInput _currentInput;

    private float _defaultCameraHeight;
    private bool _isCrouching;
    private TimeSince _timeSinceLastPostureChange = TimeSince.Never;

    private int _lastStairsTouchFrame;
    private bool _isTouchingStairs;
    private readonly string _stairsTag = "Stairs";

    private float _currentCameraHeight;

    private float _cameraTargetRotX;
    private float _cameraTargetRotY;

    private Vector3 _headPosition;

    // For steps
    private Vector3 _delayedVelocity;

    private float _shakeStrenght;

    public PlayerInteraction Interactor => _interactor;
    public Inventory Inventory => _inventory;
    public bool IsDead { get; private set; }
    public float MaxHealth => _maxHealth;
    public float Health { get; private set; }
    public Vector3 HorizontalVelocity => _velocityXZ;
    public bool IsGrounded => _controller.isGrounded;
    public bool IsCrouching => _isCrouching;
    public Transform Head => _head;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        _defaultCameraHeight = _head.localPosition.y;
        _headPosition = _head.localPosition;

        Health = _maxHealth;

        ApplyModifier(new SpawnBlockModifier(), 0.4f);

        VirtualCamera.FieldOfView = _fieldOfView;
    }

    public override void InputTick()
    {
        _currentInput = GatherInput();

        for (int i = 0; i < _interactionKeys.Length; i++)
        {
            if (Input.GetKeyDown(_interactionKeys[i]) == false)
                continue;

            if (CanInteract() == false)
                continue;

            _interactor.TryPerform(i);
        }
    }

    private Vector3 GetCurrentShake()
    {
        return new Vector3()
        {
            x = (GetRemappedPerlinNoise1D(10f, 1000f) * 2f - 1f) * _shakeStrenght,
            y = (GetRemappedPerlinNoise1D(10f, 2000f) * 2f - 1f) * _shakeStrenght,
            z = (GetRemappedPerlinNoise1D(10f, 3000f) * 2f - 1f) * _shakeStrenght
        };
    }

    private float _stepsTimer;
    [SerializeField] private Sound _stepSound;
    [SerializeField] private AudioSource _stepSource;

    private void Update()
    {
        // Camera shake
        _shakeStrenght = Mathf.Lerp(_shakeStrenght, 0f, Time.deltaTime * 10f);

        // Steps
        _stepsTimer += _delayedVelocity.magnitude / 2f * 2f * Time.deltaTime;

        if (_stepsTimer > 1f)
        {
            _stepsTimer = 0f;
            if (_controller.isGrounded == true)
                _stepSound.Play(_stepSource);
        }

        UpdateModifiers();
        UpdateCrouching();

        // Field of view
        //VirtualCamera.FieldOfView = Mathf.Lerp(VirtualCamera.FieldOfView, _fieldOfView, Time.deltaTime * 5f);

        // Update head position
        if (_timeSinceLastPostureChange < _crouchAnimationDuration)
        {
            var previousHeight = _isCrouching ? _defaultCameraHeight : _crouchedCameraHeight;
            var targetHeight = _isCrouching ? _crouchedCameraHeight : _defaultCameraHeight;
            var t = _timeSinceLastPostureChange / _crouchAnimationDuration;
            var animationCurve = _isCrouching ? _crouchAnimation : _uncrouchAnimation;
            t = animationCurve.Evaluate(t);
            _head.localPosition = new Vector3()
            {
                x = _head.localPosition.x,
                y = Mathf.Lerp(previousHeight, targetHeight, t),
                z = _head.localPosition.z
            };
        }

        UpdateHealthRegeneration();
        UpdateRotation(_currentInput);
        UpdateMovement(_currentInput);

        // test Smooth camera Y
        _currentCameraHeight = Mathf.Lerp(_currentCameraHeight, _head.transform.position.y, 15f * Time.deltaTime);

        _currentInput = new PlayerInput();
    }

    private void LateUpdate()
    {
        if (_isTouchingStairs == true && Time.frameCount > _lastStairsTouchFrame)
        {
            _controller.stepOffset = 0.3f;
            _isTouchingStairs = false;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Mathf.Abs(hit.moveDirection.y) < 0.01f)
        {
            if (hit.gameObject.TryGetComponent<Rigidbody>(out var rb) == true)
            {
                rb.AddForce(_velocityXZ * 25f);
            }
        }

        if (hit.moveDirection.y > 0.01f && _velocityY > 0f)
            _velocityY = 0f;

        if (Mathf.Abs(hit.moveDirection.y) < 0.01f && hit.gameObject.CompareTag(_stairsTag) == true && _isCrouching == false)
        {
            _lastStairsTouchFrame = Time.frameCount;
            _isTouchingStairs = true;
            _controller.stepOffset = 1.5f;
        }
    }

    public void Warp(Vector3 position)
    {
        transform.position = position;
        Physics.SyncTransforms();
    }

    public void Kill()
    {
        _velocityXZ = Vector3.zero;
        IsDead = true;
        _timeSinceLastDeath = new TimeSince(Time.time);
        Died?.Invoke();
        GetComponent<Animator>().SetBool("dead", true);
        _modifiers.Clear();
    }

    public T ApplyModifier<T>(T modifier, float duration) where T : CharacterModifier
    {
        modifier.Init(this, duration);
        _modifiers.Add(modifier);
        return modifier;
    }

    public bool HasModifier<T>() where T : CharacterModifier
    {
        foreach (var modifier in _modifiers)
        {
            if (modifier is T)
                return true;
        }

        return false;
    }

    public void TryRemoveModifier(CharacterModifier modifier) 
    {
        _modifiers.Remove(modifier);
    }

    public void ApplyDamage(float damage, Vector3 direction)
    {
        if (IsDead == true)
            return;

        _timeSinceLastDamage = new TimeSince(Time.time);
        Health = Mathf.Max(0f, Health - damage);
        Damaged?.Invoke();

        if (Health <= 0f)
        {
            Kill();
        }
        else
        {
            GetComponent<Animator>().SetFloat("damage_direction_x", transform.InverseTransformDirection(direction).x);
            GetComponent<Animator>().SetFloat("damage_direction_z", transform.InverseTransformDirection(direction).z);
            GetComponent<Animator>().Play("damaged");
        }
    }

    private PlayerInput GatherInput()
    {
        var playerInput = new PlayerInput();

        playerInput.MouseX = Input.GetAxisRaw("Mouse X") * _mouseSensitivity.Value;
        playerInput.MouseY = Input.GetAxisRaw("Mouse Y") * _mouseSensitivity.Value;

        playerInput.Direction = new FlatVector()
        {
            x = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0,
            z = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0
        }.normalized;

        playerInput.WantsJump = Input.GetKeyDown(KeyCode.Space);
        playerInput.WantsCrouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);

        playerInput.SwimDirection = Input.GetKey(KeyCode.Space) ? 1 : Input.GetKey(KeyCode.LeftControl) ? -1 : 0;

        return playerInput;
    }

    private TimeSince _timeSinceStartAiming = TimeSince.Never;
    private TimeSince _timeSinceStopAiming = TimeSince.Never;

    private void UpdateCrouching()
    {
        if (_isCrouching == false)
        {
            if (_currentInput.WantsCrouch == true && CanCrouch() == true)
            {
                Crouch();
            }
        }
        else
        {
            var shouldUncrouch = _currentInput.WantsCrouch == false || CanCrouch() == false;

            if (shouldUncrouch == true && CanUncrouch())
            {
                Stand();
            }
        }
    }

    private void UpdateModifiers()
    {
        for (int i = _modifiers.Count - 1; i >= 0; i--)
        {
            var modifier = _modifiers[i];

            if (modifier.IsInfinite == false && modifier.TimeUntilExpires < 0)
            {
                _modifiers.RemoveAt(i);
            }
            else
            {
                modifier.Tick();
            }
        }
    }

    private void UpdateRotation(PlayerInput input)
    {
        _cameraTargetRotY += input.MouseX;
        _cameraTargetRotX -= input.MouseY;

        _cameraTargetRotX = Mathf.Clamp(_cameraTargetRotX, -70f, 70f);

        transform.eulerAngles = new Vector3(0f, _cameraTargetRotY, 0f);
        _head.localEulerAngles = new Vector3(_cameraTargetRotX, 0f, 0f) + GetCurrentShake();
    }

    private void UpdateMovement(PlayerInput input)
    {
        Vector3 desiredVelocity = CanWalk() ?
            transform.TransformDirection(input.Direction) * GetSpeed() :
            Vector3.zero;

        _velocityXZ = Vector3.MoveTowards(_velocityXZ, desiredVelocity, 25f * Time.deltaTime);

        if (IsInWater() == false)
        {
            if (_controller.isGrounded == true)
            {
                _velocityY = (input.WantsJump && CanJump()) ? _jumpForce : -9.8f;
            }
            else
            {
                _velocityY -= 9.8f * Time.deltaTime;
            }
        }
        else
        {
            float desiredVelocityY = input.SwimDirection * 2f - 0.2f;
            _velocityY = Mathf.MoveTowards(_velocityY, desiredVelocityY, 10f * Time.deltaTime);
        }

        Vector3 finalMove = new Vector3()
        {
            x = _velocityXZ.x,
            y = _velocityY,
            z = _velocityXZ.z,
        };

        finalMove *= Time.deltaTime;

        _controller.Move(finalMove);
    }

    private bool IsInWater() => Water.Level > transform.position.y + _swimPointOffset;

    private void UpdateHealthRegeneration()
    {
        if (CanRegnerateHealth() == false)
            return;

        Health = Mathf.Min(Health + Time.deltaTime, _maxHealth);
    }

    private void Crouch()
    {
        _isCrouching = true;
        _timeSinceLastPostureChange = new TimeSince(Time.time);

        // temp
        _controller.height = _crouchedControllerSize;
        _controller.center = new Vector3(_controller.center.x, _controller.height / 2f, _controller.center.z);
    }

    private void Stand()
    {
        _isCrouching = false;
        _timeSinceLastPostureChange = new TimeSince(Time.time);

        // temp
        _controller.height = 2f;
        _controller.center = new Vector3(_controller.center.x, _controller.height / 2f, _controller.center.z);
    }

    private bool CanInteract()
    {
        if (IsDead == true)
            return false;

        foreach (var modifier in _modifiers)
        {
            if (modifier.CanInteract() == false)
                return false;
        }

        return true;
    }

    private bool CanJump()
    {
        if (IsDead == true)
            return false;

        if (_isCrouching == true)
            return false;

        foreach (var modifier in _modifiers)
        {
            if (modifier.CanJump() == false)
                return false;
        }

        return _allowJumping;
    }

    private bool CanWalk()
    {
        return IsDead == false;
    }

    private float GetSpeed()
    {
        var baseSpeed = _speed;

        var multipler = 1f;

        foreach (var modifier in _modifiers)
        {
            var modifierMultipler = modifier.GetSpeedMultiplier();
            multipler = Mathf.Min(multipler, modifierMultipler);
        }

        multipler = Mathf.Max(0f, multipler);

        var crouchMultipler = _isCrouching ? 0.4f : 1f;
        var woundedMultipler = Health / _maxHealth < 0.3f ? 0.6f : 1f;

        return baseSpeed * multipler * crouchMultipler * woundedMultipler;
    }

    public bool CanCrouch()
    {
        if (_allowCrouching == false)
            return false;

        if (IsInWater() == true)
            return false;

        if (_controller.isGrounded == false)
            return false;

        foreach (var modifier in _modifiers)
        {
            if (modifier.CanCrouch() == false)
                return false;
        }

        return true && _timeSinceLastPostureChange > _crouchAnimationDuration && IsDead == false;
    }

    public bool CanUncrouch()
    {
        return 
            _timeSinceLastPostureChange > _crouchAnimationDuration && 
            Physics.CheckCapsule(
                transform.position + Vector3.up * _controller.radius,
                transform.position + Vector3.up * 2f - Vector3.up * _controller.radius,
                _controller.radius, _uncrouchLayerMask) == false;
    }

    public bool CanRegnerateHealth()
    {
        return IsDead == false && _timeSinceLastDamage > Mathf.Infinity;
    }

    private float GetRemappedPerlinNoise1D(float timeMultiplier, float offset)
    {
        return Mathf.PerlinNoise1D(Time.time * timeMultiplier + offset);
    }

    private struct PlayerInput
    {
        public float MouseX;
        public float MouseY;
        public FlatVector Direction;
        public bool WantsJump;
        public bool WantsCrouch;
        public float SwimDirection;

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + Vector3.up * _kneeOffset, 0.2f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + Vector3.up * _swimPointOffset, 0.2f);
    }

}

public abstract class CharacterModifier
{
    public PlayerCharacter Character { get; private set; }
    public TimeUntil TimeUntilExpires { get; private set; }
    public bool IsInfinite { get; private set; }

    public void Init(PlayerCharacter character, float duration)
    {
        Character = character;
        TimeUntilExpires = new TimeUntil(Time.time + duration);
        IsInfinite = duration < 0f;
    }

    public virtual float GetSpeedMultiplier() => 1f;
    public virtual bool CanInteract() => true;
    public virtual bool CanJump() => true;
    public virtual bool CanCrouch() => true;
    public virtual bool CanRotateCamera() => true;
    public virtual void Tick() { }
    public virtual bool OverrideLookDirection(out Vector3 direction)
    {
        direction = default;
        return false;
    }

}

public sealed class SpawnBlockModifier : CharacterModifier
{

    public override bool CanInteract()
    {
        return true;
    }

    public override bool CanJump()
    {
        return false;
    }

    public override bool CanCrouch()
    {
        return false;
    }

    public override float GetSpeedMultiplier()
    {
        return 0f;
    }

}
