using UnityEngine;
using Fusion;

[CreateAssetMenu(fileName = "NewSpeedBoostPickup", menuName = "Pickup/SpeedBoost")]
public class SpeedBoostPickup : Pickup
{
    public override void Use(PlayerController player)
    {
        NetworkRunner runner = player.GetComponent<NetworkObject>().Runner;

        if (runner != null)
        {
            // Spawn the speed boost pickup object
            NetworkObject boostObject = runner.Spawn(PickupPrefab, player.transform.position, Quaternion.identity, player.GetComponent<NetworkObject>().InputAuthority);

            // Manually initialize the speed boost behavior after spawning
            InitializeSpeedBoost(boostObject);
        }
    }

    private void InitializeSpeedBoost(NetworkObject boostObject)
    {
        SpeedBoostBehavior boost = boostObject.GetComponent<SpeedBoostBehavior>();
        if (boost != null)
        {
            boost.Initialize();
        }
        else
        {
            Debug.LogError("SpeedBoostBehavior component not found on the spawned speed boost object.");
        }
    }
}
