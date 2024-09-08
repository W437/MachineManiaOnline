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
           SpeedBoostBehavior behavior = player.GetComponent<SpeedBoostBehavior>();
           behavior.Initialize();
        }
    }
}
