using UnityEngine;

public class PickupBox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var playerPickupSystem = collision.gameObject.GetComponent<PickupSystem>();
            if (playerPickupSystem != null)
            {
                if (playerPickupSystem.GetCurrentPickup() != null) return;

                Pickup randomPickup = PickupManager.Instance.GetRandomPickup(playerPickupSystem.PlayerRank, playerPickupSystem.TotalPlayers);
                if (randomPickup != null)
                {
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
                Destroy(gameObject);
            });
        });
    }
}
