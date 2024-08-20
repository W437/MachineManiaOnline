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
        UpdateUISlotSprite(pickup);
    }

    public Pickup GetCurrentPickup()
    {
        return currentPickup; 
    }

    public void UsePickup()
    {
        if (currentPickup != null && Runner.IsPlayer)
        {
            currentPickup.Use(player);
            ClearUISlotSprite();
            currentPickup = null;
        }
    }

    private void UpdateUISlotSprite(Pickup pickup)
    {
        if (GameUI.Instance != null)
        {
            GameUI.Instance.SetSlotSprite(pickup.GetSprite());
        }
    }

    private void ClearUISlotSprite()
    {
        if (GameUI.Instance != null)
        {
            GameUI.Instance.ClearSlotSprite();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && currentPickup != null && player.CanMove)
        {
            UsePickup();
        }
    }
}