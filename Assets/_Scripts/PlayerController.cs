using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.UI.Image;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float jumpForce = 7.5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float jumpDelay = 0.2f;
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private CapsuleCollider2D capsuleCollider, capsuleColliderTrigger;
    [SerializeField] private float downhillSpeedMultiplier = 1.5f;
    [SerializeField] private float springStrength = 50f;
    [SerializeField] private float springDamping = 5f;
    [SerializeField] private float rideHeight = 1f;
    private float speedVelocity;
    [SerializeField] private bool rotateWithGround = true;
    [SerializeField] private bool raycastRotatesWithPlayer = true;
    [SerializeField] private float rotationDamping = 0.2f;
    private float rotationVelocity;
    [SerializeField] private SpriteRenderer spriteRenderer;


    private float currentSpeed;

    [SerializeField] private float uprightCheckDistance = 1f;
    [SerializeField] private float fallAlignmentThreshold = 10f;
    [SerializeField] private float uprightSpringStrength = 50f; 
    [SerializeField] private float uprightSpringDamping = 5f;

    // Jump
    [SerializeField] private float secondJumpMultiplier = 0.7f; 

    // Sliding
    [SerializeField] private float slideDuration = 1.5f; 
    [SerializeField] private float slideCheckDistance = 1.0f; 
    [SerializeField] private Vector2 slideColliderSize = new Vector2(1.0f, 0.5f); 
    [SerializeField] private KeyCode slideKey = KeyCode.LeftShift; 

    private bool isSliding = false;
    private float slideTimer = 0.0f;
    private Vector2 originalColliderSize;
    private Quaternion originalRotation;
    private float originalRideHeight;
    private Vector3 originalSpriteScale;
    [SerializeField] private float slideRotationSpeed = 5f; 
    private bool isReturningToNormal = false; 


    [Header("Ground and Wall Check")]
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Transform slopeDetector;
    private bool isBlocked;
    private bool isAgainstWall;
    [SerializeField] private Rigidbody2D rb;

    // Private variables
    private int jumpCount;
    private bool isGrounded;
    private bool canJump = true;
    private Vector2 currentGroundNormal;

    void Start()
    {
        rb.freezeRotation = true;
        jumpCount = maxJumps;
        originalRotation = transform.rotation;
        originalColliderSize = capsuleCollider.size;
        originalRideHeight = rideHeight;
        originalSpriteScale = spriteRenderer.transform.localScale;
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
        if (Input.GetKeyDown(slideKey))
        {
            StartSlide();
        }
    }

    void FixedUpdate()
    {
        CheckGround();
        CheckWall();
        Move();
        ApplySpringForce();
        //MaintainUpright();
        CheckUprightAndAlignRotation();

        if (isSliding)
        {
            HandleSlide();
        }

        if (isReturningToNormal)
        {
            HandleReturningToNormal();
        }
    }

    private void Move()
    {
        CheckWall();

        if (isAgainstWall)
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // stop horizontal movement
            return;
        }

        float targetSpeed = maxSpeed;
        currentSpeed = rb.velocity.x;

        if (isGrounded)
        {
            float slopeAngle = Vector2.SignedAngle(Vector2.up, currentGroundNormal);
            if (slopeAngle < 0) // downhill
            {
                targetSpeed *= downhillSpeedMultiplier;
            }
        }

        // acceleration
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, acceleration * Time.fixedDeltaTime);
        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
    }


    private void Jump()
    {
        if (isGrounded)
        {
            // Reset jumps when grounded
            jumpCount = maxJumps;
        }
        if (canJump)
        {
            if (isAgainstWall || jumpCount > 1)
            {
                // unlimited jumps when against a wall
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpCount--;
                StartCoroutine(JumpDelay(jumpDelay));
            }
        }
    }

    private void StartSlide()
    {
        if (!isSliding && !isReturningToNormal)
        {
            isSliding = true;
            slideTimer = 0.0f;

            LeanTween.value(gameObject, UpdateColliderSize, originalColliderSize, slideColliderSize, 0.5f)
                .setEase(LeanTweenType.easeOutBounce);

            LeanTween.value(gameObject, UpdateRideHeight, originalRideHeight, originalRideHeight / 2, 0.5f)
                .setEase(LeanTweenType.easeOutBounce);

            LeanTween.scaleY(spriteRenderer.gameObject, originalSpriteScale.y * 0.5f, 0.5f)
                .setEase(LeanTweenType.easeOutBounce);
        }
    }

    private void UpdateColliderSize(Vector2 newSize)
    {
        capsuleCollider.size = newSize;
    }

    private void UpdateRideHeight(float newHeight)
    {
        rideHeight = newHeight;
    }

    private void HandleSlide()
    {
        slideTimer += Time.fixedDeltaTime;

        Vector2 frontDirection = transform.TransformDirection(Vector2.right);
        Vector2 backDirection = transform.TransformDirection(Vector2.left);

        RaycastHit2D frontHit = Physics2D.Raycast(transform.position, frontDirection, slideCheckDistance, platformLayer);
        RaycastHit2D backHit = Physics2D.Raycast(transform.position, backDirection, slideCheckDistance, platformLayer);

        Debug.DrawRay(transform.position, frontDirection * slideCheckDistance, Color.cyan);
        Debug.DrawRay(transform.position, backDirection * slideCheckDistance, Color.cyan);

        if (slideTimer >= slideDuration && frontHit.collider == null && backHit.collider == null)
        {
            isSliding = false;
            isReturningToNormal = true;

            LeanTween.value(gameObject, UpdateColliderSize, slideColliderSize, originalColliderSize, 0.5f)
                .setEase(LeanTweenType.easeOutBounce);

            LeanTween.value(gameObject, UpdateRideHeight, rideHeight, originalRideHeight, 0.5f)
                .setEase(LeanTweenType.easeOutBounce)
                .setOnComplete(() => isReturningToNormal = false);
            LeanTween.scaleY(spriteRenderer.gameObject, originalSpriteScale.y, 0.5f)
                .setEase(LeanTweenType.easeOutBounce);
        }
    }

    private void HandleReturningToNormal()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, originalRotation, Time.fixedDeltaTime * slideRotationSpeed);

        if (Quaternion.Angle(transform.rotation, originalRotation) < 0.1f)
        {
            transform.rotation = originalRotation;
            EndSlide();
        }
    }

    private void EndSlide()
    {
        isSliding = false;
        isReturningToNormal = false;
        capsuleCollider.size = originalColliderSize;
        transform.rotation = originalRotation;
        rideHeight = originalRideHeight;
        spriteRenderer.transform.localScale = originalSpriteScale;
    }



    [SerializeField] private float compressionLimit = 0.5f; 
    [SerializeField] private float decompressionLimit = 0.5f;


    private void ApplySpringForce()
    {
        Quaternion originalRotation = Quaternion.Euler(0, 0, 0);

        Vector2 bottomTipPosition = transform.TransformPoint(new Vector2(0, -capsuleCollider.size.y / 2));
        Vector2 downDirection = originalRotation * Vector2.down;
        RaycastHit2D mainHit = Physics2D.Raycast(bottomTipPosition, downDirection, rideHeight + originalColliderSize.y / 2, platformLayer);
        Vector2 frontDirection = originalRotation * Quaternion.Euler(0, 0, 45) * Vector2.down;
        Vector2 backDirection = originalRotation * Quaternion.Euler(0, 0, -45) * Vector2.down;
        RaycastHit2D frontHit = Physics2D.Raycast(bottomTipPosition, frontDirection, rideHeight + originalColliderSize.y, platformLayer);
        RaycastHit2D backHit = Physics2D.Raycast(bottomTipPosition, backDirection, rideHeight + originalColliderSize.y, platformLayer);

        Debug.DrawRay(bottomTipPosition, downDirection * (rideHeight + originalColliderSize.y / 2), Color.red);
        Debug.DrawRay(bottomTipPosition, frontDirection * (rideHeight + originalColliderSize.y), Color.blue);
        Debug.DrawRay(bottomTipPosition, backDirection * (rideHeight + originalColliderSize.y), Color.green);

        if (mainHit.collider != null)
        {
            isGrounded = true;
            float distanceToGround = mainHit.distance - (originalColliderSize.y / 2);
            float distance = rideHeight - distanceToGround;

            // hard limits to spring compression and decompression
            float clampedDistance = Mathf.Clamp(distance, -compressionLimit, decompressionLimit);
            float springForce = springStrength * clampedDistance - springDamping * rb.velocity.y;

            if (Mathf.Abs(springForce) > 0.1f)
            {
                rb.AddForce(Vector2.up * springForce);
            }

            Vector2 groundNormal = mainHit.normal;
            bool onSlope = false;

            if (frontHit.collider != null && backHit.collider != null)
            {
                groundNormal = ((frontHit.normal + backHit.normal) / 2).normalized;
                onSlope = true;
            }
            else if (frontHit.collider != null)
            {
                groundNormal = frontHit.normal;
                onSlope = true;
            }
            else if (backHit.collider != null)
            {
                groundNormal = backHit.normal;
                onSlope = true;
            }

            if (onSlope)
            {
                float groundAngle = Mathf.Atan2(groundNormal.y, groundNormal.x) * Mathf.Rad2Deg;
                float targetRotation = groundAngle - 90f;
                float currentRotation = transform.eulerAngles.z;
                float newRotation = Mathf.LerpAngle(currentRotation, targetRotation, Time.fixedDeltaTime * rotationDamping);
                transform.rotation = Quaternion.Euler(0f, 0f, newRotation);
            }

            // player doesn't penetrate the ground
            if (transform.position.y - originalColliderSize.y / 2 < mainHit.point.y)
            {
                transform.position = new Vector2(transform.position.x, mainHit.point.y + originalColliderSize.y / 2);
            }
        }
        else
        {
            isGrounded = false;
        }
    }


    private void CheckUprightAndAlignRotation()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, uprightCheckDistance, platformLayer);

        //Debug.DrawRay(transform.position, Vector2.down * uprightCheckDistance, Color.blue);

        if (hit.collider != null && !isAgainstWall)
        {
            // Maintain upright position with damping and spring mechanism
            float distanceToGround = hit.distance - (capsuleCollider.size.y / 2);
            float distance = rideHeight - distanceToGround;
            float springForce = uprightSpringStrength * distance - uprightSpringDamping * rb.velocity.y;
            rb.AddForce(Vector2.up * springForce);

            // Align rotation with ground rotation if falling and within threshold distance
            if (!isGrounded && distanceToGround < fallAlignmentThreshold)
            {
                ApplyGroundRotation(hit.normal);
            }
        }
    }

    private void CheckGround()
    {
        float groundCheckDistance = rideHeight + capsuleCollider.size.y / 2;
        Vector2 groundDirection = raycastRotatesWithPlayer ? transform.TransformDirection(Vector2.down) : Vector2.down;
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, groundDirection, groundCheckDistance, platformLayer);

        Debug.DrawRay(transform.position, groundDirection * groundCheckDistance, Color.yellow);

        if (groundHit.collider != null)
        {
            isGrounded = true;
            currentGroundNormal = groundHit.normal;
            // Check ground type for rotation
            if (rotateWithGround)
            {
                ApplyGroundRotation(groundHit.normal);
            }
        }
        else
        {
            isGrounded = false;
            currentGroundNormal = Vector2.up;
        }
    }

    private void ApplyGroundRotation(Vector2 groundNormal)
    {
        float groundAngle = Mathf.Atan2(groundNormal.y, groundNormal.x) * Mathf.Rad2Deg;
        float targetRotation = groundAngle - 90f;
        float smoothedRotation = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetRotation, ref rotationVelocity, rotationDamping);
        transform.rotation = Quaternion.Euler(0f, 0f, smoothedRotation);
    }

    private void CheckWall()
    {
        float wallCheckDistance = capsuleCollider.size.x / 2 + 0.333f;
        Vector2 wallDirection = Vector2.right;

        Vector2 wallCheckOriginCenter = transform.position;
        Vector2 wallCheckOriginTop = wallCheckOriginCenter + (Vector2.up * capsuleCollider.size.y / 3);
        Vector2 wallCheckOriginBottom = wallCheckOriginCenter - (Vector2.up * capsuleCollider.size.y / 3);

        RaycastHit2D wallHitCenter = Physics2D.Raycast(wallCheckOriginCenter, wallDirection, wallCheckDistance, wallLayer);
        RaycastHit2D wallHitTop = Physics2D.Raycast(wallCheckOriginTop, wallDirection, wallCheckDistance, wallLayer);
        RaycastHit2D wallHitBottom = Physics2D.Raycast(wallCheckOriginBottom, wallDirection, wallCheckDistance, wallLayer);

        Debug.DrawRay(wallCheckOriginCenter, wallDirection * wallCheckDistance, Color.red);
        Debug.DrawRay(wallCheckOriginTop, wallDirection * wallCheckDistance, Color.red);
        Debug.DrawRay(wallCheckOriginBottom, wallDirection * wallCheckDistance, Color.red);

        isAgainstWall = wallHitCenter.collider != null || wallHitTop.collider != null || wallHitBottom.collider != null;
    }

    private IEnumerator JumpDelay(float delay)
    {
        canJump = false;
        yield return new WaitForSeconds(delay);
        canJump = true;
    }

    public float GetCurrentVelocity()
    {
        return currentSpeed;
    }

    public float GetAcceleration()
    {
        return acceleration;
    }

    public float GetPlayerRotation()
    {
        return transform.rotation.z;
    }

    public float GetSlopeAngle()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 20, platformLayer);

        if (hit.collider != null)
        {
            float angle = Vector2.SignedAngle(Vector2.down, hit.normal);

            return angle;
        }
        return 0; // No slope if no hit
    }

    public int GetJumpCount()
    {
        return jumpCount;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public float GetPlayerMoveSpeed()
    {
        return maxSpeed;
    }

    public bool IsAgainstWall()
    {
        return isAgainstWall;
    }

}
