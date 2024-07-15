using UnityEngine;

public static class SaveSystem
{
    private static readonly string SaveKey = "PlayerSaveData";

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
            // default values
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
}
