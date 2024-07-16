using System;
using UnityEngine;

namespace TarodevController
{
    /// <summary>
    /// A 2D player controller script by Tarodev.
    /// This script provides basic movement, jumping, and collision handling for a 2D character.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(PickupSystem))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [SerializeField] private ScriptableStats _stats; // Reference to the player's stats stored in a ScriptableObject.
        private Rigidbody2D _rb; // Reference to the Rigidbody2D component for physics interactions.
        private CapsuleCollider2D _col; // Reference to the CapsuleCollider2D component for collision detection.
        private FrameInput _frameInput; // Stores input for the current frame.
        private Vector2 _frameVelocity; // Stores the player's velocity for the current frame.
        private bool _cachedQueryStartInColliders; // Caches the default value of Physics2D.queriesStartInColliders.

        #region Interface

        // Property to access the movement input for the current frame.
        public Vector2 FrameInput => _frameInput.Move;

        // Events that are triggered when the player lands or jumps.
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;

        #endregion

        private float _time; // Keeps track of elapsed time.

        private void Awake()
        {
            // Get references to the Rigidbody2D and CapsuleCollider2D components.
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();

            // Cache the default value of Physics2D.queriesStartInColliders.
            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        }

        private void Update()
        {
            _time += Time.deltaTime; // Increment the elapsed time.
            GatherInput(); // Gather input from the player.
        }

        private void GatherInput()
        {
            // Gather input from Unity's Input system.
            _frameInput = new FrameInput
            {
                JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C), // Jump button pressed.
                JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C), // Jump button held down.
                Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) // Movement input.
            };

            // Snap input to discrete values if SnapInput is enabled.
            if (_stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }

            // Record jump input time for buffered jump handling.
            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }
        }


        /// <summary>
        /// We're going to limit movement to only horizontally, we want the player
        /// to always move to the right direction continously without stopping, 
        /// we also want a flag to control whether he moves or not, for freezing the game before the game starts
        /// after he finishes, so we can manipuklate his position and movement.
        /// Some state system perhaps for animations, and custom stuff etc?
        /// </summary>
        private void FixedUpdate()
        {
            CheckCollisions(); // Check for collisions with the ground and ceiling.

            HandleJump(); // Handle jumping logic.
            HandleDirection(); // Handle horizontal movement logic.
            HandleGravity(); // Handle gravity and falling logic.

            ApplyMovement(); // Apply the computed velocity to the Rigidbody2D.
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

            // Return if there's no jump to consume and no buffered jump.
            if (!_jumpToConsume && !HasBufferedJump) return;

            // Execute the jump if grounded or using coyote time.
            if (_grounded || CanUseCoyote) ExecuteJump();

            _jumpToConsume = false; // Reset jump consumption flag.
        }

        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = _stats.JumpPower; // Set the vertical velocity to the jump power.
            Jumped?.Invoke(); // Trigger the Jumped event.
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
            }
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

        // Apply the computed velocity to the Rigidbody2D.
        private void ApplyMovement() => _rb.velocity = _frameVelocity;

#if UNITY_EDITOR
        // Validate the scriptable object assignment in the editor.
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

    // Interface to define the player controller events and properties.
    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public Vector2 FrameInput { get; }
    }
}
