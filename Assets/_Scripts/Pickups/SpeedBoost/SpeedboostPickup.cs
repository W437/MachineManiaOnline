using UnityEngine;
using Fusion;

[CreateAssetMenu(fileName = "NewSpeedBoostPickup", menuName = "Pickup/SpeedBoost")]
public class SpeedBoostPickup : Pickup
{
    public override void Use(PlayerController player)
    {
        NetworkObject runner = player.GetComponent<NetworkObject>();

        if (runner != null)
        {
            // Spawn the speed boost pickup object
           SpeedBoostBehavior behavior = player.GetComponent<SpeedBoostBehavior>();

            // Manually initialize the speed boost behavior after spawning
            behavior.Initialize();
        }
    }
}
