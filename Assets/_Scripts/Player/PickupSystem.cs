using Fusion;
using UnityEngine;

public class PickupSystem : NetworkBehaviour
{
    private Pickup currentPickup;
    private PlayerController player;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    public void PickupItem(Pickup pickup)
    {
        currentPickup = pickup;
        Debug.Log($"Picked up item {pickup.PickupName}");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_UsePickup()
    {
        if (currentPickup != null)
        {
            currentPickup.Use(player);
            Debug.Log("Used pickup");
            currentPickup = null;
        }
    }
}
