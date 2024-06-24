using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] M_Pickup pickupManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Pickup randomPickup = pickupManager.GetRandomPickup();
            if (randomPickup != null)
            {
                collision.gameObject.GetComponent<PickupSystem>().PickupItem(randomPickup);
                PlayPickupAnimation();
            }
        }
    }

    private void PlayPickupAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 popOutScale = originalScale * 1.2f;

        LeanTween.scale(gameObject, popOutScale, 0.2f).setEaseInOutBounce().setOnComplete(() =>
        {
            LeanTween.scale(gameObject, Vector3.zero, 0.2f).setEaseOutSine().setOnComplete(() =>
            {
                Destroy(gameObject);
            });
        });
    }
}
