using Coffee.UIExtensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Home : MonoBehaviour
{
    public static UI_Home Instance;
    [Header("Buttons")]
    public Button[] buttons;
    public Button playButton;
    private bool isButtonOnCooldown = false;
    private Dictionary<Button, float> originalYPositions = new Dictionary<Button, float>();

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

    private M_Lobby networkManager; // Add reference to NetworkManager
    private M_Audio audioManager;
    private M_Notification notificationManager;

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
        // Assign listeners to each button using EventTrigger
        foreach (var button in buttons)
        {
            AddEventTrigger(button);
            originalYPositions[button] = button.transform.localPosition.y;
        }

        AddEventTrigger(playButton);
        originalYPositions[playButton] = playButton.transform.localPosition.y;

        // Initialize events for updating stats
/*        PlayerStats playerStats = ServiceLocator.GetPlayerStats();
        playerStats.OnLevelUpdated += UpdatePlayerLevel;
        playerStats.OnGoldUpdated += UpdateGold;
        playerStats.OnDiamondsUpdated += UpdateDiamonds;
        playerStats.OnPlayersOnlineUpdated += UpdatePlayersOnline;*/

        currentPlayerCount = 0;
        targetPlayerCount = initialPlayerCount;
        UpdatePlayersOnline(currentPlayerCount);

        // Start the initial tween to the assigned count of players
        LeanTween.value(gameObject, 0, initialPlayerCount, 3f).setOnUpdate((float value) =>
        {
            UpdatePlayersOnline((int)value);
        }).setEase(LeanTweenType.easeInOutSine).setOnComplete(() =>
        {
            // Update the current player count to the initial assigned value
            currentPlayerCount = initialPlayerCount;
            targetPlayerCount = currentPlayerCount;

            // Start the players online animation
            AnimatePlayersOnline();
        });


        audioManager = ServiceLocator.GetAudioManager();
        notificationManager = ServiceLocator.GetNotificationManager();
        networkManager = ServiceLocator.GetLobbyManager();

        InvokeRepeating("AnimatePlayNowText", 0f, 7f);
    }

    public void DisablePlayButton()
    {
        playButton.interactable = false;
    }

    public void EnablePlayButton()
    {
        playButton.interactable = true;
    }

    private void AddEventTrigger(Button button)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDownEntry.callback.AddListener((eventData) => { OnButtonPressed(button, button.transform); });
        trigger.triggers.Add(pointerDownEntry);

        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUpEntry.callback.AddListener((eventData) => { OnButtonReleased(button, button.transform); });
        trigger.triggers.Add(pointerUpEntry);
    }

    Vector3 originalScale;
    private void OnButtonPressed(Button button, Transform buttonTransform)
    {
        if (isButtonOnCooldown) return;

        audioManager.PlayMenuSFX(M_Audio.MenuSFX.Click);

        LeanTween.cancel(button.gameObject);

        float yOffset = -6f;
        float animationTime = 0.09997f;
        originalScale = buttonTransform.localScale;

        switch (button.name)
        {
            case "[Button] Menu":

                notificationManager.ShowNotification(M_Notification.NotificationType.Display, "Menu clicked");

                // Parent Container
                //LeanTween.scaleX(buttonParent.gameObject, originalScale.x, animationTime).setEase(LeanTweenType.easeInElastic);
                // Move Button
                LeanTween.moveLocalY(button.gameObject, originalYPositions[button] + yOffset, animationTime).setEase(LeanTweenType.easeInExpo);
            break;

            case "[Button] Chat":

                notificationManager.ShowNotification(M_Notification.NotificationType.Display, "Chat Clicked");

                // Parent Container
                //LeanTween.scaleX(buttonParent.gameObject, originalScale.x, animationTime).setEase(LeanTweenType.easeInElastic);
                // Move Button
                LeanTween.moveLocalY(button.gameObject, originalYPositions[button] + yOffset, animationTime).setEase(LeanTweenType.easeInExpo);

            break;

            case "[Button] Season Pass":

                notificationManager.ShowNotification(M_Notification.NotificationType.Display, "SP Clicked");

                // Parent Container
                //LeanTween.scaleX(buttonParent.gameObject, originalScale.x, animationTime).setEase(LeanTweenType.easeInElastic);
                // Move Button
                LeanTween.moveLocalY(button.gameObject, originalYPositions[button] + yOffset, animationTime).setEase(LeanTweenType.easeInExpo);

            break;

            case "[Button] Shop":

                notificationManager.ShowNotification(M_Notification.NotificationType.Display, "Shop Clicked");

                // Parent Container
                //LeanTween.scaleX(buttonParent.gameObject, originalScale.x, animationTime).setEase(LeanTweenType.easeInElastic);
                // Move Button
                LeanTween.moveLocalY(button.gameObject, originalYPositions[button] + yOffset, animationTime).setEase(LeanTweenType.easeInExpo);

            break;

            case "[Button] Mode":

                notificationManager.ShowNotification(M_Notification.NotificationType.Display, "Mode Clicked");

                // Parent Container
                //LeanTween.scaleX(buttonParent.gameObject, originalScale.x, animationTime).setEase(LeanTweenType.easeInElastic);
                // Move Button
                LeanTween.scale(button.gameObject, originalScale * 0.975f, animationTime/2).setEase(LeanTweenType.easeInExpo);
                break;

            case "[Button] Play":

                yOffset = -12f;
                
                //particleAttractor.

                // Parent Container
                //LeanTween.scaleX(buttonParent.gameObject, originalScale.x, animationTime).setEase(LeanTweenType.easeInElastic);
                // Move Button
                LeanTween.moveLocalY(button.gameObject, originalYPositions[button] + yOffset, animationTime).setEase(LeanTweenType.easeInExpo);

                notificationManager.ShowNotification(M_Notification.NotificationType.Display, "Joining Game...");

                // Start the game using NetworkManager
                //networkManager.StartGame(true);
                ServiceLocator.GetLobbyManager().ShowLoadingScreen();
                break;
        }
    }

    private void OnButtonReleased(Button button, Transform buttonTransform)
    {
        float returnTime = 0.333f;

        switch (button.name)
        {
            case "[Button] Play":
            case "[Button] Menu":
            case "[Button] Chat":
            case "[Button] Season Pass":
            case "[Button] Shop":

                LeanTween.moveLocalY(button.gameObject, originalYPositions[button], returnTime).setEase(LeanTweenType.easeOutExpo);

            break;

            case "[Button] Mode":

                LeanTween.scale(button.gameObject, originalScale, returnTime/2).setEase(LeanTweenType.easeOutExpo);

            break;
        }

        StartCoroutine(ButtonCooldown());
    }

    private IEnumerator ButtonCooldown()
    {
        isButtonOnCooldown = true;
        yield return new WaitForSeconds(.133f);
        isButtonOnCooldown = false;
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
