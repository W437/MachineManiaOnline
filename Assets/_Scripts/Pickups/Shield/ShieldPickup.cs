using UnityEngine;
using Fusion;

[CreateAssetMenu(fileName = "NewShieldPickup", menuName = "Pickup/Shield")]
public class ShieldPickup : Pickup
{
    float spawnOffsetX = 0f;

    public override void Use(PlayerController player)
    {
        NetworkRunner runner = player.GetComponent<NetworkObject>().Runner;

        if (runner != null)
        {
            // Spawn the shield pickup object
           ShieldBehavior sh = player.GetComponent<ShieldBehavior>();
            sh.Initialize();

            
        }
    }
}
