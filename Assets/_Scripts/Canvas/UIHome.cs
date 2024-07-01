using Coffee.UIExtensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Fusion.Sockets.NetBitBuffer;

public class UIHome : MonoBehaviour
{
    public static UIHome Instance;

    [Header("Buttons")]
    public Button[] buttons;
    public Button playButton;
    public Button modeSelectButton;
    private bool isButtonOnCooldown = false;

    [Header("Player Stats")]
    public TextMeshProUGUI playerLevelText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI diamondsText;
    public TextMeshProUGUI playersOnlineText;
    public TextMeshProUGUI playNowText;

    [Header("Players Online Settings")]
    public float joinLeaveRatio = 1.0f; // Higher values make it more likely to increase
    private int currentPlayerCount;
    private int targetPlayerCount;
    private int initialPlayerCount = 7817;
    private UIParticleAttractor particleAttractor;

    private LobbyManager networkManager; // Add reference to NetworkManager
    private AudioManager audioManager;
    private ButtonHandler buttonHandler;
    private NotificationManager notificationManager;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        playButton.interactable = false;
    }

    private void Start()
    {
        audioManager = ServiceLocator.GetAudioManager();
        notificationManager = ServiceLocator.GetNotificationManager();
        networkManager = ServiceLocator.GetLobbyManager();

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
            buttonHandler.AddEventTrigger(button, OnButtonReleased, new ButtonConfig(customAnimation: true, realTimeUpdate: true));
        }

        // override
        buttonHandler.AddEventTrigger(playButton, OnPlayButtonReleased, new ButtonConfig(yOffset: -12f, callbackDelay: 0.1f, rotationLock: true));
        buttonHandler.AddEventTrigger(modeSelectButton, OnPlayButtonReleased, new ButtonConfig(yOffset: 0, shrinkScale: 0.95f, rotationLock: true));
    }

    private void OnPlayButtonReleased(Button button)
    {
        GameLauncher.Instance.Launch("MainSession", false);
        ServiceLocator.GetLobbyManager().ShowLobbyUI();
    }

    private void OnButtonReleased(Button button)
    {
        switch (button.name)
        {
            case "[Button] Menu":

                notificationManager.ShowNotification(NotificationManager.NotificationType.Display, "Menu clicked");

            break;

            case "[Button] Chat":

                notificationManager.ShowNotification(NotificationManager.NotificationType.Display, "Chat Clicked");

            break;

            case "[Button] Season Pass":

                notificationManager.ShowNotification(NotificationManager.NotificationType.Display, "SP Clicked");

            break;

            case "[Button] Shop":

                notificationManager.ShowNotification(NotificationManager.NotificationType.Display, "Shop Clicked");

            break;

            case "[Button] Mode":

                notificationManager.ShowNotification(NotificationManager.NotificationType.Display, "Mode Clicked");

            break;

            case "[Button] Play":

                notificationManager.ShowNotification(NotificationManager.NotificationType.Display, "Searching session..");

            break;
        }
    }

    public void DisablePlayButton()
    {
        playButton.interactable = false;
    }

    public void EnablePlayButton()
    {
        playButton.interactable = true;
    }

    private void UpdatePlayerLevel(int newLevel)
    {
        playerLevelText.text = $"Level: {newLevel}";
    }

    private void UpdateGold(int newGold)
    {
        goldText.text = $"Gold: {newGold}";
    }

    private void UpdateDiamonds(int newDiamonds)
    {
        diamondsText.text = $"Diamonds: {newDiamonds}";
    }

    private void UpdatePlayersOnline(int newPlayersOnline)
    {
        string playersOnlineColored = $"<color=#6E6404>{newPlayersOnline}</color> PLAYERS ONLINE!";
        playersOnlineText.text = playersOnlineColored;
    }

    private void AnimatePlayersOnline()
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

    private void AnimatePlayNowText()
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

    private void OnDestroy()
    {
        // Unsubscribe from events
/*        PlayerStats playerStats = ServiceLocator.GetPlayerStats();

        playerStats.OnLevelUpdated -= UpdatePlayerLevel;
        playerStats.OnGoldUpdated -= UpdateGold;
        playerStats.OnDiamondsUpdated -= UpdateDiamonds;
        playerStats.OnPlayersOnlineUpdated -= UpdatePlayersOnline;*/
    }
}
