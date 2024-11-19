using Fusion;
using UnityEngine;

public class FinishLine : NetworkBehaviour
{
    private bool triggered = false;
    public override void Spawned()
    {
        triggered = false;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;
            Debug.Log("Hit finish");
            PlayerController player = other.GetComponent<PlayerController>();
            PlayerManager playerManager = player.GetComponent<PlayerManager>();

            if ((player != null && playerManager != null) && HasStateAuthority)
            {
                // get finish time
                string finishTime = GameManager.Instance.GetRaceTime();
                playerManager.FinishTime = finishTime;
                GameManager.Instance.OnPlayerFinished(player.GetComponent<NetworkObject>().InputAuthority);

                Debug.Log($"Time: {finishTime}");
                player.CanMove = false;
            }
        }
    }
}
