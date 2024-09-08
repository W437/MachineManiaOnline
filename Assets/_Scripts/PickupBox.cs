using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class PickupBox : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collision object has a "Player" tag
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get the player's NetworkObject
            var playerNetworkObject = collision.gameObject.GetComponent<NetworkObject>();

            // Ensures the local player has InputAuthority before picking up the item
            if (!playerNetworkObject.HasInputAuthority)
            {
                Debug.Log("Not input authority (Shared Mode)");
                return;
            }

            var playerPickupSystem = collision.gameObject.GetComponent<PickupSystem>();

            if (playerPickupSystem == null)
            {
                Debug.LogError("PickupSystem not found on the player!");
                return; 
            }

            if (playerPickupSystem.GetCurrentPickup() != null)
            {
                return;
            }

            Pickup randomPickup = PickupManager.Instance.GetRandomPickup(playerPickupSystem.PlayerRank, playerPickupSystem.TotalPlayers);

            if (randomPickup != null)
            {
                playerPickupSystem.PickupItem(randomPickup);
                PlayPickupAnimation();
            }
        }
    }

    void PlayPickupAnimation()
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
