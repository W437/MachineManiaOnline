using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PickupProbability
{
    public Pickup pickup;
    public float baseProbability; // Base probability without any rank adjustments
}

public class PickupManager : MonoBehaviour
{
    public PickupProbability[] allPickupProbabilities;
    public static PickupManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Pickup GetRandomPickup(int playerRank, int totalPlayers)
    {
        if (allPickupProbabilities.Length > 0)
        {
            List<float> adjustedProbabilities = new List<float>();
            float totalAdjustedProbability = 0f;

            foreach (var pickupProb in allPickupProbabilities)
            {
                float adjustedProbability = AdjustProbabilityBasedOnRank(pickupProb.baseProbability, pickupProb.pickup.PickupName, playerRank, totalPlayers);
                adjustedProbabilities.Add(adjustedProbability);
                totalAdjustedProbability += adjustedProbability;
            }

            float randomValue = Random.Range(0, totalAdjustedProbability);
            float cumulativeProbability = 0f;

            for (int i = 0; i < allPickupProbabilities.Length; i++)
            {
                cumulativeProbability += adjustedProbabilities[i];
                if (randomValue < cumulativeProbability)
                {
                    return allPickupProbabilities[i].pickup;
                }
            }
        }
        return null;
    }

    float AdjustProbabilityBasedOnRank(float baseProbability, string pickupName, int playerRank, int totalPlayers)
    {
        float rankFactor = 1f;

        switch (pickupName)
        {
            case "SpeedBoost":
                rankFactor = playerRank == 1 ? 0.1f : playerRank == totalPlayers ? 0.6f : 1f;
                break;
            case "Saw":
                rankFactor = playerRank == 1 ? 0.5f : playerRank == totalPlayers ? 1.5f : 1f;
                break;

            default:
                rankFactor = 1f;
                break;
        }
        return baseProbability * rankFactor;
    }
}
