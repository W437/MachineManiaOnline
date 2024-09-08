using ExitGames.Client.Photon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModeSelectUI : MonoBehaviour
{
    public static ModeSelectUI Instance;
    [SerializeField] TextMeshProUGUI modeText;
    [SerializeField] TextMeshProUGUI modeInfoText;

    GameMode currentMode;

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

    void Start()
    {
        UpdateModeText();
    }

    public void OnModeButtonClicked(Button button)
    {
        CycleMode();
        UpdateModeText();
        SetGameMode(currentMode);
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
    }
    
    public enum GameMode
    {
        FFA,    // Free For All
        TVT,    // Team vs Team
        Custom  // PvP or custom setting
    }
    public GameMode GetGameMode()
    {
        return currentMode;
    }

    void CycleMode()
    {
        int nextIndex = (System.Array.IndexOf(gameModes, currentMode) + 1) % gameModes.Length;
        currentMode = gameModes[nextIndex];
    }
    void UpdateModeText()
    {
        switch (currentMode)
        {
            case GameMode.FFA:
                modeText.text = "Free for All";
                modeInfoText.text = "6 players mania match";
                break;
            case GameMode.TVT:
                modeText.text = "Team vs Team";
                modeInfoText.text = "3 vs 3 team battle";
                break;
            case GameMode.Custom:
                modeText.text = "Custom";
                modeInfoText.text = "PvP or custom setting";
                break;
        }
    }
}
