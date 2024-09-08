using UnityEngine;

public interface IPickup
{
    abstract void Use(PlayerController player);
}

public abstract class Pickup : ScriptableObject, IPickup
{
    public string PickupName;
    public Sprite PickupSprite;
    public GameObject PickupPrefab;

    public abstract void Use(PlayerController player);

    public Sprite GetPickupSprite()
    {
        return PickupSprite;
    }
}
