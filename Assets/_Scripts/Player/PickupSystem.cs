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
    }

    public void PickupItem(Pickup pickup)
    {
        currentPickup = pickup;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcUsePickup()
    {
        if (currentPickup != null)
        {
            currentPickup.Use(player);
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
