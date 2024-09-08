using System.Collections;
using UnityEngine;

public class FirestormBehavior : MonoBehaviour
{
    PlayerController player;
    GameObject firestormEffectPrefab;
    float effectDuration;
    float distanceThreshold;

    public FirestormBehavior(PlayerController player, GameObject firestormEffectPrefab, float effectDuration, float distanceThreshold)
    {
        this.player = player;
        this.firestormEffectPrefab = firestormEffectPrefab;
        this.effectDuration = effectDuration;
        this.distanceThreshold = distanceThreshold;
    }

    public IEnumerator StartFirestorm(PlayerController[] allPlayers)
    {
        yield return new WaitForSeconds(1f);

        foreach (PlayerController otherPlayer in allPlayers)
        {
            if (otherPlayer != player && Vector3.Distance(player.transform.position, otherPlayer.transform.position) < distanceThreshold)
            {
                GameObject firestormInstance = Instantiate(firestormEffectPrefab, otherPlayer.transform.position, Quaternion.identity);
                Destroy(firestormInstance, 1.333f);

                //otherPlayer.TakeDamage();
            }
        }

        yield return new WaitForSeconds(effectDuration - 1f);
    }
}
