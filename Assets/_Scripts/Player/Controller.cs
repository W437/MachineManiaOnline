using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
public class Controller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats stats; // ScriptableObject for stats
    [SerializeField] private Animator animator; // Optional: For animations
    [SerializeField] private CapsuleCollider2D bodyCollider; // Collider for player body

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isBlockedByWall;
    private bool isSliding;


    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheckPoint; // Empty GameObject at the player’s feet
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer; // Layer to identify ground

    [Header("Wall Detection")]
    [SerializeField] private Transform wallCheckPoint; // Empty GameObject to the side of the player
    [SerializeField] private float wallCheckDistance = 0.1f;

    [Header("Sliding")]
    [SerializeField] private Vector2 slideColliderSize = new Vector2(1f, 0.5f); // Size of collider during slide
    [SerializeField] private float slideSpeedMultiplier = 1.5f; // Optional: Speed boost while sliding
    private Vector2 originalColliderSize;

    public float HorizontalInput; // Input for left/right movement

    public event System.Action OnJump;
    public event System.Action<bool, float> OnGroundedChanged;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (stats == null)
        {
            Debug.LogError("PlayerStats ScriptableObject not assigned!");
        }

        if (bodyCollider == null)
        {
            Debug.LogError("Player body collider not assigned!");
        }

        if (groundCheckPoint == null)
        {
            Debug.LogError("GroundCheckPoint not assigned! Create an empty GameObject at the player’s feet and assign it.");
        }

        if (wallCheckPoint == null)
        {
            Debug.LogError("WallCheckPoint not assigned! Create an empty GameObject at the player’s side and assign it.");
        }
    }

    private void Update()
    {
        // Gather input for movement and jumping
        HorizontalInput = Input.GetAxisRaw("Horizontal"); // Returns -1 (left), 1 (right), or 0 (idle)

        // Detect if grounded
        bool wasGrounded = isGrounded;
        isGrounded = CheckGrounded();

        // Trigger event if ground state changes
        if (isGrounded != wasGrounded)
        {
            OnGroundedChanged?.Invoke(isGrounded, Mathf.Abs(rb.velocity.y));
        }

        // Detect wall blocking
        isBlockedByWall = CheckWallBlocked();

        // Jump logic
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || isBlockedByWall))
        {
            Jump();
        }

        // Slide logic
        if (Input.GetKeyDown(KeyCode.DownArrow) && isGrounded && !isSliding)
        {
            StartSlide();
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow) && isSliding)
        {
            StopSlide();
        }

        // Handle animations
        HandleAnimations();
    }

    private void FixedUpdate()
    {
        // Apply movement
        Move();
    }

    private void Move()
    {
        // Move left or right based on input unless blocked by a wall
        float moveAmount = HorizontalInput * stats.moveSpeed;
        Vector2 velocity = rb.velocity;

        // Prevent horizontal movement if blocked by a wall
        if (!isBlockedByWall)
        {
            velocity.x = moveAmount;

            // Boost speed while sliding
            if (isSliding)
            {
                velocity.x *= slideSpeedMultiplier;
            }
        }

        rb.velocity = velocity;
    }

    private void Jump()
    {
        // Apply vertical velocity for jumping
        rb.velocity = new Vector2(rb.velocity.x, stats.jumpForce);
        OnJump?.Invoke();
    }

    private void StartSlide()
    {
        isSliding = true;
        animator.SetBool("IsSliding", true);
    }

    private void StopSlide()
    {
        isSliding = false;
        animator.SetBool("IsSliding", false);
    }

    private bool CheckGrounded()
    {
        // Perform a raycast down from the ground check point
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);

        // Optional debug visualization
        Debug.DrawRay(groundCheckPoint.position, Vector2.down * groundCheckDistance, hit ? Color.green : Color.red);

        return hit.collider != null;
    }

    private bool CheckWallBlocked()
    {
        // Perform a raycast to the right or left from the wall check point
        Vector2 direction = HorizontalInput > 0 ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(wallCheckPoint.position, direction, wallCheckDistance, groundLayer);

        // Optional debug visualization
        Debug.DrawRay(wallCheckPoint.position, direction * wallCheckDistance, hit ? Color.green : Color.red);

        return hit.collider != null;
    }

    private void HandleAnimations()
    {
        if (animator == null) return;

        // Set animator parameters
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsRunning", HorizontalInput != 0 && !isBlockedByWall && !isSliding);
        animator.SetBool("IsIdle", isBlockedByWall || HorizontalInput == 0);

        if (isBlockedByWall && HorizontalInput != 0)
        {
            animator.SetBool("IsAgainstWall", true);
        }
        else
        {
            animator.SetBool("IsAgainstWall", false);
        }

        animator.SetBool("IsSliding", isSliding);
    }
}
