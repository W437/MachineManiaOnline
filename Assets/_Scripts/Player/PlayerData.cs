using System;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    // Events for UI stats updates
    public event Action<int> OnHealthUpdated;
    public event Action<int> OnLevelUpdated;
    public event Action<int> OnExperienceUpdated;
    public event Action<int> OnGoldUpdated;
    public event Action<int> OnDiamondsUpdated;
    public event Action<int> OnPlayersOnlineUpdated;

    // SFX and Music Volume
    public float SFXVolume { get; set; }
    public float MusicVolume { get; set; }
    public string PlayerName { get; set; }

    public LevelSystem levelSystem;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadStats();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public PlayerData(LevelSystem levelSystem)
    {
        this.levelSystem = levelSystem;
        Health = 100;
    }

    int health;
    public int Health
    {
        get => health;
        set
        {
            health = value;
            OnHealthUpdated?.Invoke(health);
        }
    }

    int level;
    public int Level
    {
        get => level;
        set
        {
            level = value;
            OnLevelUpdated?.Invoke(level);
            SaveStats();
        }
    }

    int experience;
    public int Experience
    {
        get => experience;
        set
        {
            experience = value;
            OnExperienceUpdated?.Invoke(experience);
            SaveStats();
        }
    }

    int gold;
    public int Gold
    {
        get => gold;
        set
        {
            gold = value;
            OnGoldUpdated?.Invoke(gold);
            SaveStats();
        }
    }

    int diamonds;
    public int Diamonds
    {
        get => diamonds;
        set
        {
            diamonds = value;
            OnDiamondsUpdated?.Invoke(diamonds);
            SaveStats();
        }
    }

    int playersOnline;
    public int PlayersOnline
    {
        get => playersOnline;
        set
        {
            playersOnline = value;
            OnPlayersOnlineUpdated?.Invoke(playersOnline);
            SaveStats();
        }
    }

    public void AddExperience(int amount)
    {
        Experience += amount;
        while (Experience >= levelSystem.GetRequiredXPForLevel(Level + 1))
        {
            Experience -= levelSystem.GetRequiredXPForLevel(Level + 1);
            Level++;
            if (Level >= levelSystem.GetMaxLevel())
            {
                Experience = 0;
                break;
            }
        }
    }

    public void SaveStats()
    {
        PlayerSaveData data = new PlayerSaveData
        {
            playerName = PlayerName,
            level = Level,
            experience = Experience,
            gold = Gold,
            diamonds = Diamonds,
            playersOnline = PlayersOnline,
            sfxVolume = SFXVolume,
            musicVolume = MusicVolume
        };
        SaveSystem.Save(data);
    }

    public void LoadStats()
    {
        PlayerSaveData data = SaveSystem.Load();
        PlayerName = data.playerName;
        Level = data.level;
        Experience = data.experience;
        Gold = data.gold;
        Diamonds = data.diamonds;
        PlayersOnline = data.playersOnline;
        SFXVolume = data.sfxVolume;
        MusicVolume = data.musicVolume;
    }
}
