using UnityEngine;

public static class SaveSystem
{
    static readonly string SaveKey = "PlayerSaveData";
    static readonly string FirstLaunchKey = "IsFirstLaunch";

    public static void Save(PlayerSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static PlayerSaveData Load()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            return JsonUtility.FromJson<PlayerSaveData>(json);
        }
        else
        {
            // Default values for a new player
            return new PlayerSaveData
            {
                playerName = "PlayaHater",
                level = 1,
                experience = 0,
                gold = 0,
                diamonds = 0,
                playersOnline = 0,
                sfxVolume = 1.0f,
                musicVolume = 1.0f
            };
        }
    }

    public static bool IsFirstLaunch()
    {
        return !PlayerPrefs.HasKey(FirstLaunchKey);
    }

    public static void SetFirstLaunchComplete()
    {
        PlayerPrefs.SetInt(FirstLaunchKey, 1);
        PlayerPrefs.Save();
    }
}
