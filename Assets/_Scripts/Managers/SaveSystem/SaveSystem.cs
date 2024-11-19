using UnityEngine;

public static class SaveSystem
{
    static readonly string SaveKey = "PlayerSaveData";
    static readonly string FirstLaunchKey = "IsFirstLaunch";

    public static void SavePlayerData(PlayerData playerData)
    {
        PlayerSaveData data = new PlayerSaveData
        {
            playerName = playerData.PlayerName,
            level = playerData.Level,
            experience = playerData.Experience,
            gold = playerData.CrownCoins,
            diamonds = playerData.Crystals,
            playersOnline = playerData.PlayersOnline,
            sfxVolume = playerData.SFXVolume,
            musicVolume = playerData.MusicVolume
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static void LoadPlayerData(PlayerData playerData)
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
            playerData.UpdateStats(data.playerName, data.level, data.experience, data.gold, data.diamonds, data.playersOnline, data.sfxVolume, data.musicVolume);
        }
        else
        {
            playerData.UpdateStats("defaultnoob", 1, 0, 0, 0, 0, 1.0f, 1.0f);
        }
    }

    public static bool IsFirstLaunch()
    {
        return !PlayerPrefs.HasKey(FirstLaunchKey);
    }

    public static void ResetFirstLaunch()
    {
        PlayerPrefs.DeleteKey(FirstLaunchKey);
    }

    public static void SetFirstLaunchComplete()
    {
        PlayerPrefs.SetInt(FirstLaunchKey, 1);
        PlayerPrefs.Save();
    }
}
