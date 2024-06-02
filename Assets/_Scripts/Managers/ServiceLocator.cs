using UnityEngine;

// Design pattern to access instances from a single place, neatly
public static class ServiceLocator
{
    private static AudioManager audioManager;
    private static NotificationManager notificationManager;

    public static void RegisterAudioManager(AudioManager manager)
    {
        audioManager = manager;
    }

    public static void RegisterNotificationManager(NotificationManager manager)
    {
        notificationManager = manager;
    }

    public static AudioManager GetAudioManager()
    {
        return audioManager;
    }

    public static NotificationManager GetNotificationManager()
    {
        return notificationManager;
    }
}
