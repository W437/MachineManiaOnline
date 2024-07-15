using Fusion;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using static Coffee.UIExtensions.UIParticleAttractor;
using static UnityEngine.UI.Image;

public class PlayerController : NetworkBehaviour
{
    //
    // Player movement inspired by Toyful Games' Capsule Theory - https://www.youtube.com/watch?v=qdskE8PJy6Q
    // Game should feel like Toyful Games' 'Very Very Valet', but in 2D.


    // Create custom physics player movement
    // Declutter player controller
    // Improve multiplayer syncing
    // To keep:
    // 1. Keep upright spring mechanism for custom physics
    // 2. Spring and 'bouncy' feel when hitting hard slope angles

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float maxSlopeAngle = 45f; // recheck if needed anymore
    [SerializeField] private float downhillSpeedMultiplier = 1.5f; // fix downhill movement
    [SerializeField] private CapsuleCollider2D capsuleCollider, capsuleColliderTrigger;
    [SerializeField] private bool raycastRotatesWithPlayer = true;
    [SerializeField] private bool rotateWithGround = true;

    private float speedVelocity;
    private float rotationVelocity;
    private float currentSpeed;

    // Spring 
    [Header("Spring Settings")]
    [SerializeField] private float springStrength = 50f;
    [SerializeField] private float springDamping = 5f;
    [SerializeField] private float rideHeight = 1f;
    [SerializeField] private float rotationDamping = 0.2f;
    [SerializeField] private float compressionLimit = 0.5f;
    [SerializeField] private float decompressionLimit = 0.5f;

    // Player Alignment - this keeps player upright with spring mechanism
    [Header("Player Alignment")]
    [SerializeField] private float uprightCheckDistance = 1f;
    [SerializeField] private float fallAlignmentThreshold = 10f;
    [SerializeField] private float uprightSpringStrength = 50f; 
    [SerializeField] private float uprightSpringDamping = 5f;

    // Jump
    [Header("Jump Settings")]
    [SerializeField] private float secondJumpMultiplier = 0.7f; 
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float jumpForce = 7.5f;
    [SerializeField] private float jumpDelay = 0.2f;

    // Sliding
    [Header("Sliding Settings")]
    [SerializeField] private float slideDuration = 1.5f; 
    [SerializeField] private float slideCheckDistance = 1.0f; 
    [SerializeField] private KeyCode slideKey = KeyCode.LeftShift; 
    [SerializeField] private float slideRotationSpeed = 5f; 

    private bool isSliding = false;
    private bool isReturningToNormal = false; 
    private float slideTimer = 0.0f;
    private float originalRideHeight;
    private Vector2 slideColliderSize = new Vector2(1.0f, 0.5f); 
    private Vector2 originalColliderSize;
    private Quaternion originalRotation;
    private Vector3 originalSpriteScale;

    [Header("Layers & Others")]
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private LayerMask obstaclesLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Transform slopeDetector;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Rigidbody2D playerRb;


    private bool isAgainstWall;
    private int jumpCount;
    private bool isGrounded;
    private bool canJump = true;
    private Vector2 currentGroundNormal;

    private PlayerStats playerStats;
    private NetworkInputData networkInput;
    private PickupSystem pickupSystem;

    private PlayerManager playerManager;


    void Start()
    {
        playerRb.freezeRotation = true;
        jumpCount = maxJumps;
        originalRotation = transform.rotation;
        originalColliderSize = capsuleCollider.size;
        originalRideHeight = rideHeight;
        originalSpriteScale = playerSpriteRenderer.transform.localScale;

        pickupSystem = ServiceLocator.GetPickupSystem();

        //playerStats = GameManager.Instance.PlayerStats;

        if (!Object.HasInputAuthority)
        {
            enabled = false; // Disable the script for non-authoritative clients
            return;
        }

        playerManager = GetComponent<PlayerManager>();

    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        if (playerManager.CanMove && playerManager.IsAlive)
        {
            if (GetInput(out NetworkInputData data))
            {
                networkInput = data;
            }
        }

        // Most checks could be done outside FixedUpdateNetwork for a smoother experience
        CheckGround(); 
        CheckWall();
        Move();
        ApplySpringForce();
        CheckUprightAndAlignRotation();

        if (networkInput.jump)
        {
            Jump();
        }

        if (networkInput.usePickup)
        {
            pickupSystem.UsePickup();
        }

        if (networkInput.slide)
        {
            StartSlide();
        }

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
            playerRb.velocity = new Vector2(0, playerRb.velocity.y); // stop horizontal movement
            return;
        }

        float targetSpeed = maxSpeed;
        currentSpeed = playerRb.velocity.x;

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
        playerRb.velocity = new Vector2(currentSpeed, playerRb.velocity.y);
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
                playerRb.velocity = new Vector2(playerRb.velocity.x, jumpForce);
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

            float distanceFromGround = GetPlayerDistanceFromGround();
            Debug.Log($"Distance from ground: {distanceFromGround}");

            if (!isGrounded && distanceFromGround > 4)
            {
                // Apply downward force when sliding in the air
                Debug.Log("Applying downward force");
                Debug.Log($"Velocity before force: {playerRb.velocity}");
                playerRb.AddForce(Vector2.down * 333, ForceMode2D.Impulse);
                Debug.Log($"Velocity after force: {playerRb.velocity}");

                // Animate the size change faster
                LeanTween.value(gameObject, UpdateColliderSize, originalColliderSize, slideColliderSize, 0.25f)
                    .setEase(LeanTweenType.easeOutBounce);

                // Halve the ride height faster
                LeanTween.value(gameObject, UpdateRideHeight, originalRideHeight, originalRideHeight / 2, 0.25f)
                    .setEase(LeanTweenType.easeOutBounce);

                // Animate the sprite scale change faster
                LeanTween.scaleY(playerSpriteRenderer.gameObject, originalSpriteScale.y * 0.5f, 0.25f)
                    .setEase(LeanTweenType.easeOutBounce);
            }
            else
            {
                // Regular slide animation
                LeanTween.value(gameObject, UpdateColliderSize, originalColliderSize, slideColliderSize, 0.5f)
                    .setEase(LeanTweenType.easeOutBounce);

                LeanTween.value(gameObject, UpdateRideHeight, originalRideHeight, originalRideHeight / 2, 0.5f)
                    .setEase(LeanTweenType.easeOutBounce);

                LeanTween.scaleY(playerSpriteRenderer.gameObject, originalSpriteScale.y * 0.5f, 0.5f)
                    .setEase(LeanTweenType.easeOutBounce);
            }
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

            float animationTime = isGrounded ? 1.3f : 1.3f;

            LeanTween.value(gameObject, UpdateColliderSize, slideColliderSize, originalColliderSize, animationTime)
                .setEase(LeanTweenType.easeOutBounce);

            LeanTween.scaleY(playerSpriteRenderer.gameObject, originalSpriteScale.y, animationTime)
                .setEase(LeanTweenType.easeOutBounce);

            LeanTween.value(gameObject, UpdateRideHeight, rideHeight, originalRideHeight, animationTime)
                .setEase(LeanTweenType.easeOutBounce)
                .setOnComplete(() => isReturningToNormal = false);
        }
    }

    private void HandleReturningToNormal()
    {
        float targetRotationSpeed = isGrounded ? slideRotationSpeed : slideRotationSpeed * 4;
        transform.rotation = Quaternion.Lerp(transform.rotation, originalRotation, Time.fixedDeltaTime * targetRotationSpeed);

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
        playerSpriteRenderer.transform.localScale = originalSpriteScale;
        LeanTween.cancel(gameObject); // Ensure all LeanTween animations are stopped
    }

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
            float springForce = springStrength * clampedDistance - springDamping * playerRb.velocity.y;

            if (Mathf.Abs(springForce) > 0.1f)
            {
                playerRb.AddForce(Vector2.up * springForce);
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

            // slopes and other rideable obstacles will have custom TAGS, to prevent rotation alighment
            if (onSlope && !isAgainstWall && !mainHit.collider.CompareTag("GroundObjects"))
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

        if (hit.collider != null && !hit.collider.CompareTag("GroundObjects"))
        {
            // Maintain upright position with damping and spring mechanism
            float distanceToGround = hit.distance - (capsuleCollider.size.y / 2);
            float distance = rideHeight - distanceToGround;
            float springForce = uprightSpringStrength * distance - uprightSpringDamping * playerRb.velocity.y;
            playerRb.AddForce(Vector2.up * springForce);

            // Align rotation with ground rotation if falling and within threshold distance
            if (!isGrounded && distanceToGround < fallAlignmentThreshold)
            {
                ApplyGroundRotation(hit.normal);
            }
        }
    }

    // Keep upright spring mechanism for custom physics
    public float GetPlayerDistanceFromGround()
    {
        // Position from where the raycast should start
        Vector2 raycastStart = capsuleColliderTrigger.transform.position;
        // Direction for the raycast
        Vector2 raycastDirection = Vector2.down;
        // Maximum distance to check for the ground
        float maxDistance = 1000f;

        // Perform the raycast
        RaycastHit2D mainHit = Physics2D.Raycast(raycastStart, raycastDirection, maxDistance, platformLayer);

        // If we hit something, return the distance
        if (mainHit.collider != null)
        {
            return mainHit.distance;
        }

        // If we didn't hit anything, return a large number or -1 to indicate no ground was found
        return -1f;
    }


    private void CheckGround()
    {
        float groundCheckDistance = 0.777f + rideHeight + (capsuleCollider.size.y / 2);
        Vector2 groundDirection = raycastRotatesWithPlayer ? transform.TransformDirection(Vector2.down) : Vector2.down;
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, groundDirection, groundCheckDistance, platformLayer);

        Debug.DrawRay(transform.position, groundDirection * groundCheckDistance, Color.yellow);

        if (groundHit.collider != null)
        {
            isGrounded = true;
            currentGroundNormal = groundHit.normal;
            // Check ground type for rotation
/*            if (rotateWithGround)
            {
                ApplyGroundRotation(groundHit.normal);
            }*/
            
            // ???????????????????
/*            if(groundHit.distance < rideHeight * 2)
            {
                jumpCount = maxJumps;
            }*/
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

        // checks wall using 3 rays along the capsule colider height
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
