using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] PickupManager pickupManager;
    [SerializeField] int playerRank; // Assume this is fed from GameManager
    [SerializeField] int totalPlayers; // Assume this is fed from GameManager

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Pickup randomPickup = pickupManager.GetRandomPickup(playerRank, totalPlayers);
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
