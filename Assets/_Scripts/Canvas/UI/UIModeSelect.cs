using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIModeSelect : MonoBehaviour
{
    [SerializeField] private Button modeButton;
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private TextMeshProUGUI modeInfoText;

    private GameManager.GameMode currentMode;

    private void Start()
    {
        currentMode = GameManager.Instance.CurrentGameMode;
        modeButton.onClick.AddListener(OnModeButtonClicked);
        UpdateModeText();
    }

    private void OnModeButtonClicked()
    {
        CycleMode();
        UpdateModeText();
        GameManager.Instance.SetGameMode(currentMode);
    }

    private void CycleMode()
    {
        GameManager.GameMode[] gameModes = GameManager.Instance.gameModes;
        int nextIndex = (System.Array.IndexOf(gameModes, currentMode) + 1) % gameModes.Length;
        currentMode = gameModes[nextIndex];
    }

    private void UpdateModeText()
    {
        switch (currentMode)
        {
            case GameManager.GameMode.FFA:
                modeText.text = "Free for All";
                modeInfoText.text = "24 players mania match";
                break;
            case GameManager.GameMode.TVT:
                modeText.text = "Team vs Team";
                modeInfoText.text = "2-6 vs 2-6 team battle";
                break;
            case GameManager.GameMode.Custom:
                modeText.text = "Custom";
                modeInfoText.text = "PvP or custom setting";
                break;
        }
    }
}
