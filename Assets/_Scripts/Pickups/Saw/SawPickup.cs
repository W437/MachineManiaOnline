using UnityEngine;
using Fusion;

[CreateAssetMenu(fileName = "NewSawPickup", menuName = "Pickup/Saw")]
public class SawPickup : Pickup
{
    public override void Use(PlayerController player)
    {
        NetworkRunner runner = player.GetComponent<NetworkObject>().Runner;

        Vector3 spawnPosition = player.transform.position + Vector3.right * 4.5f;
        NetworkObject sawInstance = runner.Spawn(PickupPrefab, spawnPosition, Quaternion.identity);
        SawBehavior saw = sawInstance.GetComponent<SawBehavior>();

        //saw.Initialize();
    }
}
