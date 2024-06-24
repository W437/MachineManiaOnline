using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ModeSelection : MonoBehaviour
{
    public Button modeButton;
    public TextMeshProUGUI modeText;
    public TextMeshProUGUI modeInfoText;

    private M_Game.GameMode currentMode;

    private void Start()
    {
        currentMode = M_Game.Instance.CurrentGameMode;
        UpdateModeText();
        modeButton.onClick.AddListener(OnModeButtonClicked);
    }

    private void OnModeButtonClicked()
    {
        CycleMode();
        UpdateModeText();
        M_Game.Instance.SetGameMode(currentMode);
    }

    private void CycleMode()
    {
        M_Game.GameMode[] gameModes = M_Game.Instance.gameModes;
        int nextIndex = (System.Array.IndexOf(gameModes, currentMode) + 1) % gameModes.Length;
        currentMode = gameModes[nextIndex];
    }

    private void UpdateModeText()
    {
        switch (currentMode)
        {
            case M_Game.GameMode.FFA:
                modeText.text = "Free for All";
                modeInfoText.text = "24 players mania match";
                break;
            case M_Game.GameMode.TVT:
                modeText.text = "Team vs Team";
                modeInfoText.text = "2-6 vs 2-6 team battle";
                break;
            case M_Game.GameMode.Custom:
                modeText.text = "Custom";
                modeInfoText.text = "PvP or custom setting";
                break;
        }
    }
}
