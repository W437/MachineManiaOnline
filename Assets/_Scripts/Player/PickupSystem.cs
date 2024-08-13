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

    public void UsePickup()
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
            UsePickup();
        }
    }
}

// problem with multiple players/objects spawning is because im using RPCs to spawn them Authoprity > all
// which is wrong, I should only use RPC to position them, and spawn them onPlayerJoined
// Spawn all joined players simply, then use RPC to position them.
// RPCs only to change networked values
// most networked objects are synced for scale and position, transform
