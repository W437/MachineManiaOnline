using Fusion;
using UnityEngine;

public class SawBehavior : NetworkBehaviour
{
    private Rigidbody2D rb;
    public float initialForce = 500;
    public float acceleration = 700f;
    public float rotationSpeed = -170;
    public float lifetime = 11f;
    private Vector2 movementDirection;
    private float currentForce;
    private bool isInitialized = false;
    private Vector3 originalScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        movementDirection = transform.right;
        currentForce = initialForce;
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero; // Ensure we start from a scale of zero for proper tweening
    }

    public void Initialize()
    {
        currentForce = initialForce;
        isInitialized = true;

        LeanTween.scale(gameObject, originalScale * 1.5f, 0.3f).setEaseOutBounce().setOnComplete(() =>
        {
            LeanTween.scale(gameObject, originalScale, 0.1f).setEaseOutBounce();
        });

        LeanTween.delayedCall(lifetime, DestroySaw);
    }

    public override void Spawned()
    {
        if (!isInitialized)
        {
            Initialize();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        rb.AddForce(movementDirection * currentForce);
        rb.angularVelocity = rotationSpeed * (currentForce / initialForce);
        currentForce += acceleration * Time.fixedDeltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!HasStateAuthority)
        {
            return;
        }

        ContactPoint2D contact = collision.contacts[0];
        if (Mathf.Abs(contact.normal.x) > Mathf.Abs(contact.normal.y))
        {
            movementDirection = -movementDirection;
            rotationSpeed = -rotationSpeed;
            currentForce = initialForce;
        }
    }

    private void DestroySaw()
    {
        LeanTween.scale(gameObject, originalScale * 1.1f, 0.2f).setEaseOutBounce().setOnComplete(() =>
        {
            LeanTween.scale(gameObject, Vector3.zero, 0.1f).setEaseInBounce().setOnComplete(() =>
            {
                if (Runner.IsRunning && Object != null)
                {
                    Runner.Despawn(Object);
                }
            });
        });
    }
}
