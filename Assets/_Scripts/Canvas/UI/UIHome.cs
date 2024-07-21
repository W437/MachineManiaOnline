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
    [SerializeField] private Button[] buttons;
    [SerializeField] private Button playButton;
    [SerializeField] private Button modeSelectButton;


    [Header("Player Stats")]
    [SerializeField] private TextMeshProUGUI playerLevelText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI diamondsText;
    [SerializeField] private TextMeshProUGUI playersOnlineText;
    [SerializeField] private TextMeshProUGUI playNowText;

    [Header("Players Online Settings")]
    [SerializeField] private float joinLeaveRatio = 1.0f;
    [SerializeField] private int currentPlayerCount;
    [SerializeField] private int targetPlayerCount;
    [SerializeField] private int initialPlayerCount = 7817;

    private ButtonHandler buttonHandler;

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
        // Disable until connection is set
        playButton.interactable = false;
    }

    private void Start()
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
            buttonHandler.AddEventTrigger(button, OnButtonReleased, new ButtonConfig(customAnimation: true, realTimeUpdate: true));
        }

        // override
        buttonHandler.AddEventTrigger(playButton, OnPlayButtonReleased, new ButtonConfig(yOffset: -12f, callbackDelay: 0.1f, rotationLock: true));
        buttonHandler.AddEventTrigger(modeSelectButton, OnPlayButtonReleased, new ButtonConfig(yOffset: 0, shrinkScale: 0.95f, rotationLock: true));
    }

    private void OnPlayButtonReleased(Button button)
    {
        GameLauncher.Instance.Launch("MainSession2", false);
        LobbyManager.Instance.ShowLobbyUI();
    }

    private void OnButtonReleased(Button button)
    {
        switch (button.name)
        {
            case "[Button] Menu":

                NotificationManager.Instance.ShowNotification(NotificationManager.NotificationType.Display, "Menu clicked");

            break;

            case "[Button] Chat":

                NotificationManager.Instance.ShowNotification(NotificationManager.NotificationType.Display, "Chat Clicked");
                ChatManager.Instance.ShowChat();

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
