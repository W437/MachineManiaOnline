using ExitGames.Client.Photon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModeSelectUI : MonoBehaviour
{
    public static ModeSelectUI Instance;
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private TextMeshProUGUI modeInfoText;

    private GameMode currentMode;

    private void Awake()
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

    private void Start()
    {
        UpdateModeText();
    }

    public void OnModeButtonClicked(Button button)
    {
        CycleMode();
        UpdateModeText();
        SetGameMode(currentMode);
    }

    private void CycleMode()
    {
        int nextIndex = (System.Array.IndexOf(gameModes, currentMode) + 1) % gameModes.Length;
        currentMode = gameModes[nextIndex];
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
    }

    public GameMode GetGameMode()
    {
        return currentMode;
    }

    private void UpdateModeText()
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
