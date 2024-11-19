using System;
using UnityEngine;
using static NotificationManager;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    // SFX and Music Volume
    public float SFXVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public string PlayerName { get; set; }
    public int Health { get; private set; }
    public int Level { get; private set; }
    public int Experience { get; private set; }
    public int CrownCoins { get; private set; }
    public int Crystals { get; private set; }
    public int PlayersOnline { get; private set; }

    public event Action<NotificationType, string> OnNotification;

    public event Action<string, object> OnDataChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void ResetAllData()
    {
        PlayerName = "";
        Level = 1;
        Experience = 0;
        CrownCoins = 0;
        Crystals = 0;
        PlayersOnline = 0;

        SaveSystem.SavePlayerData(this);
        SaveSystem.ResetFirstLaunch();

        TriggerNotification(NotificationType.Display, "Player data reset.");
        TriggerDataChange("PlayerName", PlayerName);
        TriggerDataChange("Level", Level);
        TriggerDataChange("Experience", Experience);
        TriggerDataChange("CrownCoins", CrownCoins);
        TriggerDataChange("Crystals", Crystals);
        TriggerDataChange("PlayersOnline", PlayersOnline);
    }

    public void ChangePlayerName(string newName)
    {
        if (string.IsNullOrEmpty(newName) || newName.Length > 10 || newName.Contains(" "))
        {
            TriggerNotification(NotificationType.Warning, "Name must be non-empty, shorter than 10 characters, and contain no spaces.");
            return;
        }

        PlayerName = newName;
        SaveSystem.SavePlayerData(this);
        TriggerDataChange("PlayerName", PlayerName);
        TriggerNotification(NotificationType.Success, $"Name changed to: {PlayerName} ");
    }

    private void TriggerNotification(NotificationType type, string message)
    {
        OnNotification?.Invoke(type, message);
    }

    // Helper method to trigger data changes
    private void TriggerDataChange(string dataField, object newValue)
    {
        OnDataChanged?.Invoke(dataField, newValue);
    }

    public void UpdateStats(string playerName, int level, int experience, int crownCoins, int crystals, int playersOnline, float sfxVolume, float musicVolume)
    {
        PlayerName = playerName;
        Level = level;
        Experience = experience;
        CrownCoins = crownCoins;
        Crystals = crystals;
        PlayersOnline = playersOnline;
        SFXVolume = sfxVolume;
        MusicVolume = musicVolume;
    }
}
