using UnityEngine;

public interface IPickup
{
    void Use(PlayerController player);
}

public abstract class Pickup : ScriptableObject, IPickup
{
    public string PickupName;
    public Sprite PickupIcon;
    public GameObject PickupPrefab;
    public float SpawnOffsetX;
    public abstract void Use(PlayerController player);
}
