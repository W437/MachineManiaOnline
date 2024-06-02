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

    public Pickup GetRandomPickup()
    {
        if (allPickupProbabilities.Length > 0)
        {
            int randomIndex = Random.Range(0, allPickupProbabilities.Length);
            return allPickupProbabilities[randomIndex].pickup;
        }
        return null;
    }

    public Pickup GetRandomProbabilityPickup(int playerRank, int totalPlayers)
    {
        if (allPickupProbabilities.Length > 0)
        {
            List<float> adjustedProbabilities = new List<float>();
            float totalAdjustedProbability = 0f;

            // Adjust probabilities based on player rank
            foreach (var pickupProb in allPickupProbabilities)
            {
                float adjustedProbability = AdjustProbabilityBasedOnRank(pickupProb.baseProbability, pickupProb.pickup.name, playerRank, totalPlayers);
                adjustedProbabilities.Add(adjustedProbability);
                totalAdjustedProbability += adjustedProbability;
            }

            // Get a random value within the total adjusted probability range
            float randomValue = Random.Range(0, totalAdjustedProbability);
            float cumulativeProbability = 0f;

            // Select a pickup based on the random value and adjusted probabilities
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

    private float AdjustProbabilityBasedOnRank(float baseProbability, string pickupName, int playerRank, int totalPlayers)
    {
        float rankFactor = 1f;

        switch (pickupName)
        {
            case "SpeedBoost":
                if (playerRank == 1)
                {
                    rankFactor = 0.1f; // 10% of the base probability if the player is first
                }
                else if (playerRank == totalPlayers)
                {
                    rankFactor = 0.6f; // 60% of the base probability if the player is last
                }
                else
                {
                    rankFactor = 1f; // No adjustment for other ranks
                }
                break;

            case "Saw":
                if (playerRank == 1)
                {
                    rankFactor = 0.5f; // 50% of the base probability if the player is first
                }
                else if (playerRank == totalPlayers)
                {
                    rankFactor = 1.5f; // 150% of the base probability if the player is last
                }
                else
                {
                    rankFactor = 1f; // No adjustment for other ranks
                }
                break;

            // Add cases for other pickups here

            default:
                rankFactor = 1f; // No adjustment for unknown pickups
                break;
        }

        return baseProbability * rankFactor;
    }
}
