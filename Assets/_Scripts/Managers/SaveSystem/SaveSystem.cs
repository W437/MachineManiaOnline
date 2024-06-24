using UnityEngine;

public static class SaveSystem
{
    private static readonly string SaveKey = "PlayerSaveData";

    public static void Save(S_PlayerSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static S_PlayerSaveData Load()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            return JsonUtility.FromJson<S_PlayerSaveData>(json);
        }
        else
        {
            // default values
            return new S_PlayerSaveData
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
