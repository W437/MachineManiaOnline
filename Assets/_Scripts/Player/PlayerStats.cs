using System;

public static class PlayerStats
{
    public static Action<int> OnPlayerLevelUpdated;
    public static Action<int> OnGoldUpdated;
    public static Action<int> OnDiamondsUpdated;
    public static Action<int> OnPlayersOnlineUpdated;
}