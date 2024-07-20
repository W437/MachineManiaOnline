using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    // Events for stat updates
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

    private void Awake()
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

    public PlayerStats(LevelSystem levelSystem)
    {
        this.levelSystem = levelSystem;
        Health = 100;
    }

    // Health
    private int health;
    public int Health
    {
        get => health;
        set
        {
            health = value;
            OnHealthUpdated?.Invoke(health);
        }
    }

    // Level
    private int level;
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

    // Experience
    private int experience;
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

    // Gold
    private int gold;
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

    // Diamonds
    private int diamonds;
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

    private int playersOnline;
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

    // Save stats using SaveSystem
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

    // Load stats using SaveSystem
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
