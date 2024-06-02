using UnityEngine;

public abstract class Pickup : ScriptableObject
{
    public string pickupName;
    public Sprite icon;
    public GameObject prefab;
    public float spawnOffsetX;
    public abstract void Use(PlayerController player);
}
