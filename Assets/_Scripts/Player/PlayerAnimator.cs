using UnityEngine;
using Fusion;

/// <summary>
/// VERY primitive animator example.
/// </summary>
public class PlayerAnimator : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private Animator _anim;

    [Networked] private bool IsJumping { get; set; }
    [Networked] private float IdleSpeed { get; set; }
    [Networked] private bool IsGrounded { get; set; }
    [Networked] private bool IsRunning { get; set; }
    [Networked] private bool IsIdle { get; set; }


    [SerializeField] private SpriteRenderer _sprite;

    [Header("Settings")]
    [SerializeField, Range(1f, 3f)]
    private float _maxIdleSpeed = 2;

    [SerializeField] private float _maxTilt = 5;
    [SerializeField] private float _tiltSpeed = 20;

    [Header("Particles")][SerializeField] private ParticleSystem _jumpParticles;
    [SerializeField] private ParticleSystem _launchParticles;
    [SerializeField] private ParticleSystem _moveParticles;
    [SerializeField] private ParticleSystem _landParticles;

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip[] _footsteps;

    private AudioSource _source;
    private IPlayerController _player;
    private bool _grounded;
    private ParticleSystem.MinMaxGradient _currentGradient;

    private NetworkObject networkObject;
    

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _player = GetComponentInParent<IPlayerController>();
        networkObject = GetComponentInParent<NetworkObject>();
    }

    private void OnEnable()
    {
        _player.Jumped += OnJumped;
        _player.GroundedChanged += OnGroundedChanged;

        _moveParticles.Play();
    }

    private void OnDisable()
    {
        _player.Jumped -= OnJumped;
        _player.GroundedChanged -= OnGroundedChanged;

        _moveParticles.Stop();
    }

    private void Update()
    {
        if (_player == null) return;

        // Check if we have authority only for input handling; animations should be synced across all clients.
        if (HasStateAuthority)
        {
            DetectGroundColor();
            HandleSpriteFlip();
            HandleIdleSpeed();
            HandleCharacterTilt();
            HandleRunAnimation();
        }

        // Apply the synced states to the animation controller on all clients
        _anim.SetBool("IsJump", IsJumping);
        _anim.SetFloat(IdleSpeedKey, IdleSpeed);
        _anim.SetBool("IsRunning",IsRunning);
        _anim.SetBool("IsIdle", IsIdle);
        _anim.SetBool("IsLand", IsGrounded);
    }

    private void HandleSpriteFlip()
    {
        if (_player.FrameInput.x != 0) _sprite.flipX = _player.FrameInput.x < 0;
        
    }
    private void HandleRunAnimation()
    {
        if (Mathf.Abs(_player.FrameInput.x)>0)
        {
            IsRunning = true;
            IsIdle = false;
        }
        else
        {
            IsRunning = false;
            IsIdle = true;
        }
    }

    private void HandleIdleSpeed()
    {
        var inputStrength = Mathf.Abs(_player.FrameInput.x);
        _anim.SetFloat(IdleSpeedKey, Mathf.Lerp(1, _maxIdleSpeed, inputStrength));
        _moveParticles.transform.localScale = Vector3.MoveTowards(_moveParticles.transform.localScale, Vector3.one * inputStrength, 2 * Time.deltaTime);
    }

    private void HandleCharacterTilt()
    {
        var runningTilt = _grounded ? Quaternion.Euler(0, 0, _maxTilt * _player.FrameInput.x) : Quaternion.identity;
        _anim.transform.up = Vector3.RotateTowards(_anim.transform.up, runningTilt * Vector2.up, _tiltSpeed * Time.deltaTime, 0f);
    }

    private void OnJumped()
    {
        if (HasStateAuthority) // Check if this client has authority
        {
            IsJumping = true;
            IdleSpeed = _player.FrameInput.x;  // Example of syncing IdleSpeed
        }

        
        _anim.ResetTrigger(GroundedKey);


        if (_grounded) // Avoid coyote
        {
            SetColor(_jumpParticles);
            SetColor(_launchParticles);
            _jumpParticles.Play();
        }
    }

    private void OnFalling()
    {

    }

    private void OnGroundedChanged(bool grounded, float impact)
    {
        _grounded = grounded;
        if (HasStateAuthority) // Ensure only the authoritative client sets this
        {
            IsGrounded = grounded;
        }

       
        if (grounded)
        {
            Debug.Log("Grounded");
            DetectGroundColor();
            SetColor(_landParticles);

            _anim.SetTrigger(GroundedKey);
            _source.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);
            _moveParticles.Play();
            IsJumping = false;
            _landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
            _landParticles.Play();
        }
        else
        {
            _moveParticles.Stop();
        }
    }

    private void DetectGroundColor()
    {
        var hit = Physics2D.Raycast(transform.position, Vector3.down, 2);

        if (!hit || hit.collider.isTrigger || !hit.transform.TryGetComponent(out SpriteRenderer r)) return;
        var color = r.color;
        _currentGradient = new ParticleSystem.MinMaxGradient(color * 0.9f, color * 1.2f);
        SetColor(_moveParticles);
    }

    private void SetColor(ParticleSystem ps)
    {
        var main = ps.main;
        main.startColor = _currentGradient;
    }

    private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
    private static readonly int JumpKey = Animator.StringToHash("Jump");
}