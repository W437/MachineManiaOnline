using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.Collections.Unicode;

public class HomeUI : MonoBehaviour
{
    public static HomeUI Instance;

    [Header("Buttons")]
    [SerializeField] Button[] buttons;
    [SerializeField] Button playButton;
    [SerializeField] Button modeSelectButton;
    public Button menuButton;

    [Header("Menu")]
    public GameObject menuBar;
    public Button[] menuButtons;
    public GameObject settingsPanel;
    private bool menuBarIsOpen = false;

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
    [SerializeField] TextMeshProUGUI sessionNameText;
    public Transform PrivateLobbyPositionsParent;

    [Header("Private Lobby Chat")]
    public GameObject ChatPanel;
    public ScrollRect MessageScrollView;
    public TMP_InputField MessageInputField;
    public Transform MessageContent;
    public Button SendMessageButton;
    public bool ChatVisible = false;
    public GameObject ChatBG;
    public RectTransform ChatContainer;
    public float FadeDuration = 0.25f;
    public float SlideDuration = 0.5f;
    public Button BGExitButton;
    public Button ChatExitButton;


    ButtonHandler buttonHandler;
    public ButtonHandler ButtonHandler { get { return buttonHandler; } }

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

        // init values
        originalAlpha = customSessionPanelBG.color.a;
        menuBar.transform.localScale = new Vector3(1, 0, 1);

        foreach (Button button in menuButtons)
        {
            button.gameObject.SetActive(false);
        }

        AddButtonEventTriggers();

        // Load player data
        playerName.text = PlayerData.Instance.PlayerName;
    }

    void Update()
    {
        if (menuBarIsOpen)
        {
            if (Input.touchCount > 0)
            {
                // Handle touch input
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    if (!IsPointerOverUIElement(touch.position))
                    {
                        CloseMenuBar();
                    }
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                // Handle mouse input
                if (!IsPointerOverUIElement(Input.mousePosition))
                {
                    CloseMenuBar();
                }
            }
        }

        if(FusionLauncher.Instance.Runner() != null)
        {
            if(FusionLauncher.Instance.Runner() !.IsRunning)
            {
                playButton.interactable = false;
            }
            else
            {
                playButton.interactable = true;
            }
        }
    }

    void AddButtonEventTriggers()
    {
        // Main Buttono listeners
        foreach (var button in buttons)
        {
            buttonHandler.AddButtonEventTrigger(button, OnButtonReleased, new ButtonConfig(customAnimation: true, realTimeUpdate: true));
        }

        // override
        buttonHandler.AddButtonEventTrigger(playButton, OnPlayButtonClick, new ButtonConfig(yOffset: -12f, callbackDelay: 0.1f, rotationLock: true));
        buttonHandler.AddButtonEventTrigger(modeSelectButton, ModeSelectUI.Instance.OnModeButtonClicked, new ButtonConfig(yOffset: 0, shrinkScale: 0.95f, rotationLock: true, returnTime: 0.1f));
        buttonHandler.AddButtonEventTrigger(btnCreateCustomSession, OnCreateCustomSession, new ButtonConfig(callbackDelay: 0.1f, rotationLock: true));
        buttonHandler.AddButtonEventTrigger(btnExitCustomSession, OnExitCustomSessionPanel, new ButtonConfig(yOffset: -1));

        // Menu
        buttonHandler.AddButtonEventTrigger(menuButton, OnButtonReleased, new ButtonConfig(customAnimation: true, realTimeUpdate: true, returnTime: 0));

        // menubar buttonos
        foreach (Button button in menuButtons)
        {
            buttonHandler.AddButtonEventTrigger(button, OnButtonReleased, new ButtonConfig(customAnimation: true, realTimeUpdate: true));
        }
    }

    void ToggleMenuBar()
    {
        if (menuBarIsOpen)
        {
            CloseMenuBar();
        }
        else
        {
            OpenMenuBar();
        }
    }

    void OpenMenuBar()
    {
        menuBarIsOpen = true;

        foreach (Button button in menuButtons)
        {
            button.gameObject.SetActive(true);
            button.transform.localScale = Vector3.zero;
            button.interactable = false;
        }

        menuBar.transform.localScale = Vector3.zero;

        LeanTween.scale(menuBar, Vector3.one, 0.2f).setEase(LeanTweenType.easeOutCubic);
        foreach (Button button in menuButtons)
        {
            LeanTween.scale(button.gameObject, Vector3.one, 0.2f).setEase(LeanTweenType.easeOutCubic);
        }

        LeanTween.delayedCall(0.2f, () =>
        {
            foreach (Button button in menuButtons)
            {
                button.interactable = true;
            }
        });
    }

    void CloseMenuBar()
    {
        if (!menuBarIsOpen)
            return;

        foreach (Button button in menuButtons)
        {
            button.interactable = false;
        }

        LeanTween.scale(menuBar, Vector3.zero, 0.2f).setEase(LeanTweenType.easeInCubic);
        foreach (Button button in menuButtons)
        {
            LeanTween.scale(button.gameObject, Vector3.zero, 0.2f).setEase(LeanTweenType.easeInCubic);
        }

        LeanTween.delayedCall(0.2f, () =>
        {
            foreach (Button button in menuButtons)
            {
                button.gameObject.SetActive(false);
            }
            menuBarIsOpen = false;
        });
    }

    bool IsPointerOverUIElement(Vector2 screenPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            // Check if the pointer is over the menu bar, its children, or the menu button
            if (result.gameObject == menuBar || result.gameObject.transform.IsChildOf(menuBar.transform) ||
                result.gameObject == menuButton.gameObject || result.gameObject.transform.IsChildOf(menuButton.transform))
            {
                return true;
            }
        }
        return false;
    }

    void ShowSettingsPanel()
    {
        settingsPanel.SetActive(true);
        CloseMenuBar();
    }

    public void CloseSettingsPanel()
    {
        settingsPanel.SetActive(false);
    }

    void ShowNewsfeedPanel()
    {
        CloseMenuBar();
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
                FusionLauncher.Instance.Runner().LoadScene(SceneRef.FromIndex(2), LoadSceneMode.Single);
                break;

            case ModeSelectUI.GameMode.TVT:

            break;

            default:
                Debug.Log("Unknown game mode selected.");
            break;
        }
    }

    void OnButtonReleased(Button button)
    {
        if (menuBarIsOpen && button.name != "[Button] Menu")
        {
            CloseMenuBar();
        }

        switch (button.name)
        {
            case "[Button] Menu":

                ToggleMenuBar();

            break;

            case "[Button] Chat":

                HomeChatManager.Instance.ShowChat();

            break;

            case "[Button] Mania Pass":


            break;

            case "[Button] Shop":


            break;

            case "[Button] Mode":


            break;

            case "[Button] Play":


            break;

            case "[Button] Settings":
                ShowSettingsPanel();
                break;

            case "[Button] Newsfeed":
                ShowNewsfeedPanel();
                break;

            default:
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
            LeanTween.scale(customSessionContainer, Vector3.zero, 0.15f).setEase(LeanTweenType.easeInQuad);

            LeanTween.value(customSessionPanelBG.gameObject, UpdateBGAlpha, originalAlpha, 0, 0.25f).setOnComplete(() =>
            {
                customSessionPanel.SetActive(false);
            });
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
