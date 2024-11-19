using UnityEngine;

/// <summary>
/// Handles animations, particles, and effects for the player.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _anim;
    [SerializeField] private SpriteRenderer _sprite;

    [Header("Tilt Settings")]
    [SerializeField, Range(1f, 3f)] private float _maxIdleSpeed = 2f;
    [SerializeField] private float _maxTilt = 10f;
    [SerializeField] private float _tiltSpeed = 20f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem _jumpParticles;
    [SerializeField] private ParticleSystem _launchParticles;
    [SerializeField] private ParticleSystem _moveParticles;
    [SerializeField] private ParticleSystem _landParticles;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] _footsteps;

    private AudioSource _audioSource;
    private Controller _player;
    private bool _grounded;
    private ParticleSystem.MinMaxGradient _currentGradient;

    private static readonly int IsJumpKey = Animator.StringToHash("IsJump");
    private static readonly int IsFallingKey = Animator.StringToHash("IsFalling");
    private static readonly int IsRunningKey = Animator.StringToHash("IsRunning");
    private static readonly int IsIdleKey = Animator.StringToHash("IsIdle");
    private static readonly int IsLandKey = Animator.StringToHash("IsLand");

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _player = GetComponentInParent<Controller>();

        if (_player == null)
        {
            Debug.LogError("PlayerController not found! Make sure this script is attached to the player object.");
        }
    }

    private void OnEnable()
    {
        _player.OnJump += HandleJump;
        _player.OnGroundedChanged += HandleGroundedChanged;

        _moveParticles.Play();
    }

    private void OnDisable()
    {
        _player.OnJump -= HandleJump;
        _player.OnGroundedChanged -= HandleGroundedChanged;

        _moveParticles.Stop();
    }

    private void Update()
    {
        if (_player == null) return;

        DetectGroundColor();
        //HandleSpriteFlip();
        HandleCharacterTilt();
    }

    private void HandleCharacterTilt()
    {
        // Tilt the character based on movement direction
        float tiltAngle = _grounded ? _maxTilt * _player.HorizontalInput : 0f;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, tiltAngle);
        _anim.transform.rotation = Quaternion.RotateTowards(_anim.transform.rotation, targetRotation, _tiltSpeed * Time.deltaTime);
    }

    private void HandleJump()
    {
        _anim.SetBool(IsJumpKey, true);
        _anim.SetBool(IsLandKey, false);

        if (_grounded)
        {
            PlayParticles(_jumpParticles);
            PlayParticles(_launchParticles);
        }
    }

    private void HandleGroundedChanged(bool grounded, float impact)
    {
        _grounded = grounded;

        if (grounded)
        {
            DetectGroundColor();
            PlayParticles(_landParticles);
            _anim.SetBool(IsJumpKey, false);
            _anim.SetBool(IsLandKey, true);
            _moveParticles.Play();

            // Footstep sound
            if (_footsteps.Length > 0)
            {
                _audioSource.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);
            }
        }
        else
        {
            _anim.SetBool(IsLandKey, false);
            _moveParticles.Stop();
        }
    }

    private void DetectGroundColor()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 2f);

        if (hit.collider != null && !hit.collider.isTrigger && hit.collider.TryGetComponent(out SpriteRenderer groundSprite))
        {
            Color groundColor = groundSprite.color;
            _currentGradient = new ParticleSystem.MinMaxGradient(groundColor * 0.9f, groundColor * 1.2f);
            ApplyParticleColor(_moveParticles);
        }
    }

    private void ApplyParticleColor(ParticleSystem particleSystem)
    {
        var mainModule = particleSystem.main;
        mainModule.startColor = _currentGradient;
    }

    private void PlayParticles(ParticleSystem particleSystem)
    {
        ApplyParticleColor(particleSystem);
        particleSystem.Play();
    }
}
