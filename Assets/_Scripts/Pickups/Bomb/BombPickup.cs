using UnityEngine;
using Fusion;

[CreateAssetMenu(fileName = "NewBombPickup", menuName = "Pickup/Bomb")]
public class BombPickup : Pickup
{
    public override void Use(PlayerController player)
    {
        NetworkRunner runner = player.GetComponent<NetworkObject>().Runner;

        Vector3 spawnPosition = player.transform.position;
        NetworkObject bombInstance = runner.Spawn(PickupPrefab, spawnPosition, Quaternion.identity);
        BombBehavior bomb = bombInstance.GetComponent<BombBehavior>();

        bomb.Initialize();
    }
}
