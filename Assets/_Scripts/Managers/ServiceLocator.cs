using UnityEngine;

// Design pattern to access instances from a single place, neatly
public static class ServiceLocator
{
    private static M_Audio audioManager;
    private static M_Notification notificationManager;
    private static M_Game gameManager;
    private static M_Pickup pickupManager;
    private static PlayerStats playerStats;
    private static PickupSystem pickupSystem;
    private static M_Lobby lobbyManager;
    private static M_Network networkManager;
    private static M_Player playerManager;
    private static UI_Home UI_Home;

    public static void RegisterAudioManager(M_Audio manager)
    {
        audioManager = manager;
    }

    public static void RegisterNotificationManager(M_Notification manager)
    {
        notificationManager = manager;
    }

    public static void RegisterGameManager(M_Game manager)
    {
        gameManager = manager;
    }

    public static void RegisterPickupManager(M_Pickup manager)
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

    public static void RegisterLobbyManager(M_Lobby manager)
    {
        lobbyManager = manager;
    }

    public static void RegisterNetworkManager(M_Network manager)
    {
        networkManager = manager;
    }

    public static void RegisterPlayerManager(M_Player manager)
    {
        playerManager = manager;
    }

    public static void RegisterUI_Home(UI_Home manager)
    {
        UI_Home = manager;
    }

    public static M_Audio GetAudioManager()
    {
        return audioManager;
    }

    public static M_Notification GetNotificationManager()
    {
        return notificationManager;
    }

    public static M_Game GetGameManager()
    {
        return gameManager;
    }

    public static M_Pickup GetPickupManager()
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

    public static M_Lobby GetLobbyManager()
    {
        return lobbyManager;
    }

    public static M_Network GetNetworkManager()
    {
        return networkManager;
    }

    public static UI_Home GetUIHome()
    {
        return UI_Home;
    }

    public static M_Player GetPlayerManager()
    {
        return playerManager;
    }
}
