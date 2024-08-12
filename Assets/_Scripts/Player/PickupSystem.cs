using Fusion;
using UnityEngine;

public class PickupSystem : NetworkBehaviour
{
    private Pickup currentPickup;
    private PlayerController player;

    public int PlayerRank { get; set; }
    public int TotalPlayers { get; set; }

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    public override void Spawned()
    {
        Debug.Log("Spawned PickupSys");
    }

    public void PickupItem(Pickup pickup)
    {
        currentPickup = pickup;
        Debug.Log($"Picked up item {pickup.PickupName}");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcUsePickup()
    {
        if (currentPickup != null)
        {
            currentPickup.Use(player);
            Debug.Log("Used pickup");
            currentPickup = null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && currentPickup != null)
        {
            RpcUsePickup();
        }
    }
}
