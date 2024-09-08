using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A 2D player controller script by Tarodev.
/// This script provides basic movement, jumping, and collision handling for a 2D character.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour, IPlayerController
{
   
    [SerializeField] private ScriptableStats _stats;
    public ScriptableStats Stats { get { return _stats; } }
    [SerializeField] private Animator _animator;
    public List<SpriteRenderer> PlayerParts = new List<SpriteRenderer>();
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private FrameInput _frameInput; // Stores input for the current frame.
    private Vector2 _frameVelocity; // Stores the player's velocity for the current frame.
    private bool _cachedQueryStartInColliders;

    // Gameplay properties.
    [SerializeField] private bool _canMove = true;
    public bool CanMove { get { return _canMove; } set { _canMove = value; } }
    private bool _canMoveFreely = true; 
    public float FinishTime { get; set; }


    #region Interface

    // Property to access the movement input for the current frame.
    public Vector2 FrameInput => _frameInput.Move;

    // Events that are triggered when the player lands or jumps.
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    #endregion

    private float _time; 

    // for tracking animation states
    private bool _isFalling;
    private bool _isRunning;
    private bool _isIdle;
    private bool _isLanding;
    private bool _isJumping;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Update()
    {
        _time += Time.deltaTime; 
        GatherInput();
        HandleJump(); 
    }


    private void FixedUpdate()
    {
        if (_canMove)
        {
            CheckCollisions();
            HandleGravity();
            ApplyMovement(); // Automatically move to the right
            HandleAnimations(); // Manage animations based on the player's state
        }
        else
        {
            _rb.velocity = Vector2.zero;
        }
    }

    private void GatherInput()
    {
        _frameInput = new FrameInput
        {
            JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
            JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
            Move = _canMove ? new Vector2(1, 0) : Vector2.zero 
        };

        // Record jump input time for buffered jump handling.
        if (_frameInput.JumpDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }
    }

    private void HandleAnimations()
    {


        // If grounded and not falling
        if (_grounded && _frameVelocity.y <= 0)
        {
            if (_isFalling)
            {
                _isFalling = false;
                _isLanding = true;
                _animator.SetBool("IsFalling", false);
                _animator.SetBool("IsLand", true);
            }

            if (_frameVelocity.x > 0)
            {
                // Player is running
                if (!_isRunning)
                {
                    _isRunning = true;
                    _isIdle = false;
                    _animator.SetBool("IsRunning", true);
                    _animator.SetBool("IsIdle", false);
                }
            }
            else
            {
                // Player is idle
                if (!_isIdle)
                {
                    _isRunning = false;
                    _isIdle = true;
                    _animator.SetBool("IsRunning", false);
                    _animator.SetBool("IsIdle", true);
                }
            }
        }

        // If player is jumping
        if (_isJumping)
        {
            _animator.SetBool("IsJump", true);
            _isJumping = false;
        }

        // Handle falling animation
        if (_rb.velocity.y < 0 && !_grounded)
        {
            if (!_isFalling)
            {
                _isFalling = true;
                _animator.SetBool("IsFalling", true);
                _animator.SetBool("IsJump", false);
            }
        }

        // Handle landing animation
        if (_grounded && _isLanding)
        {
            _isLanding = false;
            _animator.SetBool("IsLand", false);
        }
    }




    #region Collisions

    private float _frameLeftGrounded = float.MinValue; // Time when the player last left the ground.
    private bool _grounded; // Whether the player is currently grounded.

    private void CheckCollisions()
    {
        // Temporarily disable queries starting inside colliders.
        Physics2D.queriesStartInColliders = false;

        // Check for collisions with the ground and ceiling.
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

        // Prevent upward velocity when hitting the ceiling.
        if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

        // Handle landing on the ground.
        if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
        }
        // Handle leaving the ground.
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _frameLeftGrounded = _time;
            GroundedChanged?.Invoke(false, 0);
        }

        // Restore the default value of queriesStartInColliders.
        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    #endregion


    #region Jumping

    private bool _jumpToConsume; // Whether a jump input should be consumed.
    private bool _bufferedJumpUsable; // Whether a buffered jump can be used.
    private bool _endedJumpEarly; // Whether the jump was ended early.
    private bool _coyoteUsable; // Whether coyote time can be used.
    private float _timeJumpWasPressed; // Time when the jump button was pressed.

    // Whether a buffered jump is available.
    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
    // Whether coyote time can be used.
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

    private void HandleJump()
    {
        // End the jump early if the jump button is released.
        if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;

        // Check if jump input is available or buffered.
        if (_jumpToConsume || HasBufferedJump)
        {
            // Execute the jump if grounded or using coyote time.
            if (_grounded || CanUseCoyote)
            {
                ExecuteJump();
            }
            _jumpToConsume = false; // Reset jump consumption flag.
        }
    }


    private void ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _frameVelocity.y = _stats.JumpPower; // Set the vertical velocity to the jump power
        _isJumping = true;
        Jumped?.Invoke(); // Trigger the Jumped event
    }


    #endregion

    #region Horizontal

    private void HandleDirection()
    {
        // Decelerate to a stop if no movement input is detected.
        if (_frameInput.Move.x == 0)
        {
            var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        // Accelerate towards the input direction.
        else
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            Debug.Log(_frameVelocity.x);
        }
    }
    public void ChangeSpeedAndAcceleration(float speedMultiplier,float accelertionMulitplier)
    {
        _stats.MaxSpeed = speedMultiplier;
        _stats.Acceleration = accelertionMulitplier;
    }

    #endregion

    #region Gravity

    private void HandleGravity()
    {
        // Apply grounding force if grounded and not moving upwards.
        if (_grounded && _frameVelocity.y <= 0f)
        {
            _frameVelocity.y = _stats.GroundingForce;
        }
        else
        {
            // Apply gravity while in the air.
            var inAirGravity = _stats.FallAcceleration;
            if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    #endregion


    private void ApplyMovement()
    {
        _frameVelocity.x = _stats.MaxSpeed; // Always move to the right
        _rb.velocity = _frameVelocity;
    }

    // Sandbox use
    public void TogglePlayerMovement(bool enable)
    {
        _canMove = enable;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
    }
#endif
}

// Struct to store frame-specific input.
public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public Vector2 Move;
}

public interface IPlayerController
{
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    public Vector2 FrameInput { get; }
}
