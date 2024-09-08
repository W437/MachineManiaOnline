using UnityEngine;
using Fusion;

[CreateAssetMenu(fileName = "NewCloakPickup", menuName = "Pickup/Cloak")]
public class CloakPickup : Pickup
{
    float spawnOffsetX = 0f;

    public override void Use(PlayerController player)
    {
        if (player != null)
        {
            var cloakBehavior = player.GetComponent<CloakBehavior>();
            if (cloakBehavior != null)
            {
                cloakBehavior.Initialize();
            }
        }
    }
}
