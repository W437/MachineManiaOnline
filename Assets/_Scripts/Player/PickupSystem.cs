using UnityEngine;

public class PickupSystem : MonoBehaviour
{
    private Pickup currentPickup;
    private PlayerController player;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        ServiceLocator.RegisterPickupSystem(this);
    }


    public void PickupItem(Pickup pickup)
    {
        currentPickup = pickup;
        Debug.Log($"Picked up item {pickup.pickupName}");
    }

    public void UsePickup()
    {
        if (currentPickup != null)
        {
            currentPickup.Use(player);
            Debug.Log("Used pickup");
            currentPickup = null; // Remove pickup after uses
        }
    }

}
