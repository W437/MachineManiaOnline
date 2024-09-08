using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeUI : MonoBehaviour
{
    public static HomeUI Instance;

    [Header("Buttons")]
    [SerializeField] Button[] buttons;
    [SerializeField] Button playButton;
    [SerializeField] Button modeSelectButton;


    [Header("Player Stats")]
    [SerializeField] TextMeshProUGUI playerLevelText;
    [SerializeField] TextMeshProUGUI goldText;
    [SerializeField] TextMeshProUGUI diamondsText;
    [SerializeField] TextMeshProUGUI playersOnlineText;
    [SerializeField] TextMeshProUGUI playNowText;
    [SerializeField] TextMeshProUGUI playerName;

    [Header("Custom Session")]
    [SerializeField] TMP_InputField inputSessionName;
    [SerializeField] TMP_InputField inputSessionPassword;
    [SerializeField] TMP_Dropdown inputMaxPlayers;
    [SerializeField] Button btnCreateCustomSession;
    [SerializeField] Button btnExitCustomSession;
    [SerializeField] GameObject customSessionPanel;
    [SerializeField] GameObject customSessionContainer;
    [SerializeField] Image customSessionPanelBG;
    float originalAlpha;

    [Header("Players Online Settings")]
    [SerializeField] float joinLeaveRatio = 1.0f;
    [SerializeField] int currentPlayerCount;
    [SerializeField] int targetPlayerCount;
    [SerializeField] int initialPlayerCount = 7817;
    
    [Header("Private Lobby")]
    public Transform PrivateLobbyPositionsParent;
    [SerializeField] TextMeshProUGUI sessionNameText;
    public PrivateLobbyPosition[] PrivateLobbyPositions { get; private set; }

    ButtonHandler buttonHandler;

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
        // Disable until connection is set
        playButton.interactable = false;
    }

    void Start()
    {
        buttonHandler = gameObject.AddComponent<ButtonHandler>();

        currentPlayerCount = 0;
        targetPlayerCount = initialPlayerCount;
        UpdatePlayersOnline(currentPlayerCount);

        // Players online sim
        LeanTween.value(gameObject, 0, initialPlayerCount, 3f)
            .setOnUpdate((float value) => { UpdatePlayersOnline((int)value); })
            .setEase(LeanTweenType.easeInOutSine).setOnComplete(() =>
            {
                currentPlayerCount = initialPlayerCount;
                targetPlayerCount = currentPlayerCount;

                AnimatePlayersOnline();
            });

        InvokeRepeating("AnimatePlayNowText", 0f, 7f);

        // Buttono listeners
        foreach (var button in buttons)
        {
            buttonHandler.AddButtonEventTrigger(button, OnButtonReleased, new ButtonConfig(customAnimation: true, realTimeUpdate: true));
        }

        // override
        buttonHandler.AddButtonEventTrigger(playButton, OnPlayButtonClick, new ButtonConfig(yOffset: -12f, callbackDelay: 0.1f, rotationLock: true));
        buttonHandler.AddButtonEventTrigger(modeSelectButton, ModeSelectUI.Instance.OnModeButtonClicked, new ButtonConfig(yOffset: 0, shrinkScale: 0.95f, rotationLock: true, returnTime: 0.1f));
        buttonHandler.AddButtonEventTrigger(btnCreateCustomSession, OnCreateCustomSession, new ButtonConfig(callbackDelay: 0.1f, rotationLock: true));
        buttonHandler.AddButtonEventTrigger(btnExitCustomSession, OnExitCustomSessionPanel, new ButtonConfig(yOffset: -1));

        // init values
        originalAlpha = customSessionPanelBG.color.a;

        // private lobby
        InitializeLobbyPositions();

        // Load player data
        playerName.text = PlayerData.Instance.PlayerName;
    }

    void InitializeLobbyPositions()
    {
        if (PrivateLobbyPositionsParent != null)
        {
            int childCount = PrivateLobbyPositionsParent.childCount;
            PrivateLobbyPositions = new PrivateLobbyPosition[childCount];
            for (int i = 0; i < childCount; i++)
            {
                Transform child = PrivateLobbyPositionsParent.GetChild(i);
                PrivateLobbyPositions[i] = new PrivateLobbyPosition(child);
            }
        }
    }

    void OnPlayButtonClick(Button button)
    {
        var selectedMode = ModeSelectUI.Instance.CurrentGameMode;
        switch (selectedMode)
        {
            case ModeSelectUI.GameMode.Custom:
                ToggleCustomSessionPanel();
            break;

            case ModeSelectUI.GameMode.FFA:
                LobbyUI.Instance.ConnectToLobby();
                GameLauncher.Instance.Launch("FFASession", false, SessionType.Public, 6);
            break;

            case ModeSelectUI.GameMode.TVT:
                LobbyUI.Instance.ConnectToLobby();
                GameLauncher.Instance.Launch("TVTSession", false);
            break;

            default:
                Debug.Log("Unknown game mode selected.");
            break;
        }
    }

    void OnButtonReleased(Button button)
    {
        switch (button.name)
        {
            case "[Button] Menu":

                NotificationManager.Instance.ShowNotification(NotificationManager.NotificationType.Display, "Menu clicked");

            break;

            case "[Button] Chat":

                NotificationManager.Instance.ShowNotification(NotificationManager.NotificationType.Display, "Chat Clicked");
                HomeChatManager.Instance.ShowChat();

            break;

            case "[Button] Mania Pass":

                NotificationManager.Instance.ShowNotification(NotificationManager.NotificationType.Display, "SP Clicked");

            break;

            case "[Button] Shop":

                NotificationManager.Instance.ShowNotification(NotificationManager.NotificationType.Display, "Shop Clicked");

            break;

            case "[Button] Mode":

                NotificationManager.Instance.ShowNotification(NotificationManager.NotificationType.Display, "Mode Clicked");

            break;

            case "[Button] Play":

                NotificationManager.Instance.ShowNotification(NotificationManager.NotificationType.Display, "Searching session..");

            break;
        }
    }

    void OnCreateCustomSession(Button button)
    {
        string sessionName = inputSessionName.text;
        string sessionPassword = inputSessionPassword.text;
        int maxPlayers = int.Parse(inputMaxPlayers.options[inputMaxPlayers.value].text);

        if (string.IsNullOrEmpty(sessionName) || sessionName.Length >= 12 || sessionName.Contains(" "))
        {
            NotificationManager.Instance.ShowNotification(NotificationManager.NotificationType.Warning, "Session name must be non-empty, shorter than 10 characters, and contain no spaces.");
            return;
        }

        bool withPassword = !string.IsNullOrEmpty(sessionPassword);

        GameLauncher.Instance.Launch(sessionName, false, SessionType.Private, maxPlayers);
        ToggleCustomSessionPanel();
    }

    void OnExitCustomSessionPanel(Button button)
    {
        ToggleCustomSessionPanel();
    }

    public void ToggleCustomSessionPanel()
    {
        bool isOpening = !customSessionPanel.activeSelf;
        customSessionPanel.SetActive(true);

        if (isOpening)
        {
            customSessionContainer.transform.localScale = Vector3.zero;

            LeanTween.scale(customSessionContainer, Vector3.one, 0.15f).setEase(LeanTweenType.easeOutQuad);

            LeanTween.value(customSessionPanelBG.gameObject, UpdateBGAlpha, 0, originalAlpha, 0.25f);
        }
        else
        {
            LeanTween.scale(customSessionContainer, Vector3.zero, 0.15f).setEase(LeanTweenType.easeInQuad).setOnComplete(() =>
            {
                customSessionPanel.SetActive(false);
            });

            LeanTween.value(customSessionPanelBG.gameObject, UpdateBGAlpha, originalAlpha, 0, 0.25f);
        }
    }

    void UpdateBGAlpha(float alpha)
    {
        Color color = customSessionPanelBG.color;
        color.a = alpha;
        customSessionPanelBG.color = color;
    }

    public void DisablePlayButton()
    {
        playButton.interactable = false;
    }

    public void EnablePlayButton()
    {
        playButton.interactable = true;
    }

    void UpdatePlayerLevel(int newLevel)
    {
        playerLevelText.text = $"Level: {newLevel}";
    }

    void UpdateGold(int newGold)
    {
        goldText.text = $"Gold: {newGold}";
    }

    void UpdateDiamonds(int newDiamonds)
    {
        diamondsText.text = $"Diamonds: {newDiamonds}";
    }

    void UpdatePlayersOnline(int newPlayersOnline)
    {
        string playersOnlineColored = $"<color=#6E6404>{newPlayersOnline}</color> PLAYERS ONLINE!";
        playersOnlineText.text = playersOnlineColored;
    }

    void AnimatePlayersOnline()
    {
        float delay = Random.Range(1, 4) * 2;

        int change = Random.Range(1, 8);
        if (Random.value < joinLeaveRatio / (joinLeaveRatio + 1))
        {
            targetPlayerCount += change; 
        }
        else
        {
            targetPlayerCount -= change;
            targetPlayerCount = Mathf.Max(targetPlayerCount, 0);
        }

        LeanTween.value(gameObject, currentPlayerCount, targetPlayerCount, delay).setOnUpdate((float value) =>
        {
            UpdatePlayersOnline((int)value);
        }).setEase(LeanTweenType.easeInOutSine).setOnComplete(() =>
        {
            currentPlayerCount = targetPlayerCount;

            AnimatePlayersOnline();
        });
    }

    void AnimatePlayNowText()
    {
        float originalSize = playNowText.fontSize;
        float targetSize = originalSize * 1.1f;

        LeanTween.value(playNowText.gameObject, originalSize, targetSize, 0.5f).setEase(LeanTweenType.easeInOutSine).setOnUpdate((float value) =>
        {
            playNowText.fontSize = value;
        }).setOnComplete(() =>
        {
            LeanTween.value(playNowText.gameObject, targetSize, originalSize, 0.25f).setEase(LeanTweenType.easeInOutSine).setOnUpdate((float value) =>
            {
                playNowText.fontSize = value;
            });
        });
    }

    public void SetSessionNameUI(string sessionName)
    {
        sessionNameText.text = $"<color=#59B4F7>session:</color> {sessionName}";
    }

    void OnDestroy()
    {
        // Unsubscribe from events
/*        PlayerStats playerStats = ServiceLocator.GetPlayerStats();

        playerStats.OnLevelUpdated -= UpdatePlayerLevel;
        playerStats.OnGoldUpdated -= UpdateGold;
        playerStats.OnDiamondsUpdated -= UpdateDiamonds;
        playerStats.OnPlayersOnlineUpdated -= UpdatePlayersOnline;*/
    }
}
