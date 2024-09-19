using Fusion;
using UnityEngine;

public class FinishLine : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && HasStateAuthority)
            {
                GameManager.Instance.PlayerFinished(player.GetComponent<NetworkObject>().InputAuthority);
            }
        }
    }
}
