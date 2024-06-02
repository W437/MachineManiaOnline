using UnityEngine;

[CreateAssetMenu(fileName = "NewFirestormPickup", menuName = "Pickup/Firestorm")]
public class FirestormPickup : Pickup
{
    public GameObject firecloudsPrefab;
    public GameObject firestormEffectPrefab;
    public float effectDuration = 5f;
    public float distanceThreshold = 10f; // threshold for affecting other players

    public override void Use(PlayerController player)
    {
        // Instantiate the fireclouds effect at the top part of the screen for all players
        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        foreach (PlayerController p in allPlayers)
        {
            GameObject fireclouds = Instantiate(firecloudsPrefab, p.transform);
            fireclouds.transform.localPosition = new Vector3(0, 10, 0);
            Destroy(fireclouds, effectDuration);
        }

        // Start the firestorm effect
        FirestormBehavior firestorm = new FirestormBehavior(player, firestormEffectPrefab, effectDuration, distanceThreshold);
        player.StartCoroutine(firestorm.StartFirestorm(allPlayers));
    }
}
