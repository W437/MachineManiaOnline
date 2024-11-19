using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public float moveSpeed = 5f; // Speed for horizontal movement
    public float jumpForce = 10f; // Force for jumping
}
