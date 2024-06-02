using UnityEngine;

public class PickupSystem : MonoBehaviour
{
    public Pickup currentPickup;
    private PlayerController player;

    private void Start()
    {
        player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (currentPickup != null && Input.GetKeyDown(KeyCode.LeftControl))
        {
            UsePickup();
        }
    }

    public void PickupItem(Pickup pickup)
    {
        currentPickup = pickup;
        Debug.Log($"Picked up item {pickup.pickupName}");
    }

    private void UsePickup()
    {
        if (currentPickup != null)
        {
            currentPickup.Use(player);
            Debug.Log("Used pickup");
            currentPickup = null; // Remove pickup after uses
        }
    }

}
