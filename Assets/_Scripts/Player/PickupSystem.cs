using Fusion;
using UnityEngine;

public class PickupSystem : NetworkBehaviour
{
    Pickup currentPickup;
    PlayerController player;

    public int PlayerRank { get; set; }
    public int TotalPlayers { get; set; }

    void Awake()
    {
        player = GetComponent<PlayerController>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && currentPickup != null && player.CanMove)
        {
            UsePickup();
        }
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

    void UpdateUISlotSprite(Pickup pickup)
    {
        if (GameUI.Instance != null)
        {
            Debug.Log("PickupSprite: " + pickup.GetPickupSprite());
            GameUI.Instance.SetSlotSprite(pickup.GetPickupSprite());
        }
    }

    void ClearUISlotSprite()
    {
        if (GameUI.Instance != null)
        {
            GameUI.Instance.ClearSlotSprite();
        }
    }
}