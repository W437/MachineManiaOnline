using UnityEngine;
using Fusion;

[CreateAssetMenu(fileName = "NewCloakPickup", menuName = "Pickup/Cloak")]
public class CloakPickup : Pickup
{
    public override void Use(PlayerController player)
    {
        NetworkRunner runner = player.GetComponent<NetworkObject>().Runner;

        if (runner != null)
        {
            // Spawn the cloak pickup object
            NetworkObject cloakObject = runner.Spawn(PickupPrefab, player.transform.position, Quaternion.identity, player.GetComponent<NetworkObject>().InputAuthority);

            // Manually initialize the cloak behavior after spawning
            CloakBehavior cloakBehavior = cloakObject.GetComponent<CloakBehavior>();
            if (cloakBehavior != null)
            {
                cloakBehavior.Initialize();
            }
        }
    }
}
