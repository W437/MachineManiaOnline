using UnityEngine;

[CreateAssetMenu(fileName = "NewSawPickup", menuName = "Pickup/Saw")]
public class SawPickup : Pickup
{
    public override void Use(PlayerController player)
    {
        GameObject sawInstance = Instantiate(prefab, player.transform.position + Vector3.right * spawnOffsetX, Quaternion.identity);
        SawBehavior saw = sawInstance.GetComponent<SawBehavior>();
        if (saw == null)
        {
            saw = sawInstance.AddComponent<SawBehavior>();
        }
        saw.Initialize();
    }
}
