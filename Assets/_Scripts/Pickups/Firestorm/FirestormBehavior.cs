using System.Collections;
using UnityEngine;

public class FirestormBehavior : MonoBehaviour
{
    private PlayerController player;
    private GameObject firestormEffectPrefab;
    private float effectDuration;
    private float distanceThreshold;

    public FirestormBehavior(PlayerController player, GameObject firestormEffectPrefab, float effectDuration, float distanceThreshold)
    {
        this.player = player;
        this.firestormEffectPrefab = firestormEffectPrefab;
        this.effectDuration = effectDuration;
        this.distanceThreshold = distanceThreshold;
    }

    public IEnumerator StartFirestorm(PlayerController[] allPlayers)
    {
        yield return new WaitForSeconds(1f); // Delay before firestorm hits

        foreach (PlayerController otherPlayer in allPlayers)
        {
            if (otherPlayer != player && Vector3.Distance(player.transform.position, otherPlayer.transform.position) < distanceThreshold)
            {
                // Instantiate firestorm effect on the other player's position
                GameObject firestormInstance = Instantiate(firestormEffectPrefab, otherPlayer.transform.position, Quaternion.identity);
                Destroy(firestormInstance, 1.333f);

                // Apply damage or kill the other player
                //otherPlayer.TakeDamage(); // Assuming there's a TakeDamage method
            }
        }

        yield return new WaitForSeconds(effectDuration - 1f); // Wait for the rest of the duration
    }
}
