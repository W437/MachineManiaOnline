using UnityEngine;
using Fusion;

[CreateAssetMenu(fileName = "NewShieldPickup", menuName = "Pickup/Shield")]
public class ShieldPickup : Pickup
{
    public override void Use(PlayerController player)
    {
        NetworkRunner runner = player.GetComponent<NetworkObject>().Runner;

        if (runner != null)
        {
            // Spawn the shield pickup object
            NetworkObject shieldObject = runner.Spawn(PickupPrefab, player.transform.position, Quaternion.identity, player.GetComponent<NetworkObject>().InputAuthority);

            // Manually initialize the shield behavior after spawning
            InitializeShield(shieldObject);
        }
    }

    private void InitializeShield(NetworkObject shieldObject)
    {
        ShieldBehavior shield = shieldObject.GetComponent<ShieldBehavior>();
        if (shield != null)
        {
            shield.Initialize();
        }
        else
        {
            Debug.LogError("ShieldBehavior component not found on the spawned shield object.");
        }
    }
}
