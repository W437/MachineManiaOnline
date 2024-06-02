using UnityEngine;

public class NotificationTestKey : MonoBehaviour
{
    public KeyCode testKey = KeyCode.N;  // Set the key to 'N' for testing

    private void Update()
    {
        // Check if the test key is pressed
        if (Input.GetKeyDown(testKey))
        {
            ShowTestNotification();
        }
    }

    private void ShowTestNotification()
    {
        // Show a test notification
        NotificationManager.Instance.ShowNotification(NotificationManager.NotificationType.Display, "This is a test notification!");
    }
}
