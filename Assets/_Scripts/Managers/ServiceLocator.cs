using UnityEngine;

// Design pattern to access instances from a single place, neatly
public static class ServiceLocator
{
    private static AudioManager audioManager;
    private static NotificationManager notificationManager;
    private static GameManager gameManager;
    private static PickupManager pickupManager;
    private static PlayerStats playerStats;
    private static PickupSystem pickupSystem;
    private static LobbyManager lobbyManager;
    private static UIHome UIHome;

    public static void RegisterAudioManager(AudioManager manager)
    {
        audioManager = manager;
    }

    public static void RegisterNotificationManager(NotificationManager manager)
    {
        notificationManager = manager;
    }

    public static void RegisterGameManager(GameManager manager)
    {
        gameManager = manager;
    }

    public static void RegisterPickupManager(PickupManager manager)
    {
        pickupManager = manager;
    }

    public static void RegisterPlayerStats(PlayerStats manager)
    {
        playerStats = manager;
    }

    public static void RegisterPickupSystem(PickupSystem manager)
    {
        pickupSystem = manager;
    }

    public static void RegisterLobbyManager(LobbyManager manager)
    {
        lobbyManager = manager;
    }


    public static void RegisterUI_Home(UIHome manager)
    {
        UIHome = manager;
    }

    public static AudioManager GetAudioManager()
    {
        return audioManager;
    }

    public static NotificationManager GetNotificationManager()
    {
        return notificationManager;
    }

    public static GameManager GetGameManager()
    {
        return gameManager;
    }

    public static PickupManager GetPickupManager()
    {
        return pickupManager;
    }

    public static PlayerStats GetPlayerStats()
    {
        return playerStats;
    }

    public static PickupSystem GetPickupSystem()
    {
        return pickupSystem;
    }

    public static LobbyManager GetLobbyManager()
    {
        return lobbyManager;
    }


    public static UIHome GetUIHome()
    {
        return UIHome;
    }
}
