using UnityEngine;

public class M_Game : MonoBehaviour
{
    public static M_Game Instance { get; private set; }
    public PlayerStats PlayerStats { get; private set; }

    public LevelSystem LevelSystem;

    private void Awake()
    {
        // Cap fps
        Application.targetFrameRate = 60;

        if (Instance == null)
        {
            Instance = this;
            InitializeGame();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public enum GameMode
    {
        FFA,    // Free For All
        TVT,    // Team vs Team
        Custom  // PvP or custom setting
    }

    public GameMode CurrentGameMode { get; private set; }

    public readonly GameMode[] gameModes =
    {
        GameMode.FFA,
        GameMode.TVT,
        GameMode.Custom
    };

    public void SetGameMode(GameMode mode)
    {
        CurrentGameMode = mode;
        // Additional logic for setting game mode can be added here
    }

    private void InitializeGame()
    {
        PlayerStats = new PlayerStats(LevelSystem);
        PlayerStats.LoadStats();
        SetGameMode(GameMode.FFA);
    }

    public void SaveGame()
    {
        PlayerStats.SaveStats();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}
