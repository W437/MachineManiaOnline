/*using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    //public PlayerController playerController; // Reference to the PlayerController
    public TextMeshProUGUI debugText; // Reference to the UI Text component

    void Update()
    {
        if (playerController != null && debugText != null)
        {
            // Collect the stats from the PlayerController
            float velocity = playerController.GetCurrentVelocity();
            float acceleration = playerController.GetAcceleration();
            float slopeAngle = playerController.GetSlopeAngle();
            int jumpCount = playerController.GetJumpCount();
            bool isGrounded = playerController.IsGrounded();
            bool isBlocked = playerController.IsAgainstWall();
            float currentRotation = playerController.GetPlayerRotation();
            float moveSpeed = playerController.GetPlayerMoveSpeed();

            // Construct the debug string
            string debugString = $"V: {velocity}\n" +
                                 $"Slope Angle: {slopeAngle}\n" +
                                 $"Curr. Rotation: {currentRotation}\n" +
                                 $"Acceleration: {acceleration}\n" +
                                 $"Is Grounded: {isGrounded}\n" +
                                 $"Is Blocked: {isBlocked}\n" +
                                 $"Move Speed: {moveSpeed}\n" +
                                 $"Jump Count: {jumpCount}\n";

            // Update the debug text
            debugText.text = debugString;
        }
    }
}
*/