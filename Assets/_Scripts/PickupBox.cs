using UnityEngine;

public class PickupBox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collision w box!");
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("its player");
            var playerPickupSystem = collision.gameObject.GetComponent<PickupSystem>();
            if (playerPickupSystem != null)
            {
                // Get a random pickup from the PickupManager
                Pickup randomPickup = PickupManager.Instance.GetRandomPickup(playerPickupSystem.PlayerRank, playerPickupSystem.TotalPlayers);
                Debug.Log("Getting rand pickup");
                if (randomPickup != null)
                {
                    Debug.Log("rand pickup");
                    playerPickupSystem.PickupItem(randomPickup);
                    PlayPickupAnimation();
                }
            }
        }
    }

    private void PlayPickupAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 popOutScale = originalScale * 1.1f;

        LeanTween.scale(gameObject, popOutScale, 0.2f).setEaseInOutBounce().setOnComplete(() =>
        {
            LeanTween.scale(gameObject, Vector3.zero, 0.2f).setEaseOutSine().setOnComplete(() =>
            {
                Destroy(gameObject); // Destroy the pickup box after it's been collected
            });
        });
    }
}
