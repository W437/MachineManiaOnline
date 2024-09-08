using UnityEngine;

[CreateAssetMenu(fileName = "NewFirestormPickup", menuName = "Pickup/Firestorm")]
public class FirestormPickup : Pickup
{
    public GameObject firecloudsPrefab;
    public GameObject firestormEffectPrefab;
    public float effectDuration = 5f;
    public float distanceThreshold = 10f;

    public override void Use(PlayerController player)
    {
        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        foreach (PlayerController p in allPlayers)
        {
            GameObject fireclouds = Instantiate(firecloudsPrefab, p.transform);
            fireclouds.transform.localPosition = new Vector3(0, 10, 0);
            Destroy(fireclouds, effectDuration);
        }

        FirestormBehavior firestorm = new FirestormBehavior(player, firestormEffectPrefab, effectDuration, distanceThreshold);
        player.StartCoroutine(firestorm.StartFirestorm(allPlayers));
    }
}
