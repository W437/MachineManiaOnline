using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Home : MonoBehaviour
{
    [Header("Buttons")]
    public Button[] buttons;
    public Button findGameButton;
    private bool isButtonOnCooldown = false;
    private Dictionary<Button, float> originalYPositions = new Dictionary<Button, float>();

    [Header("Player Stats")]
    public TextMeshProUGUI playerLevelText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI diamondsText;
    public TextMeshProUGUI playersOnlineText;

    [Header("Players Online Settings")]
    public float joinLeaveRatio = 1.0f; // Higher values make it more likely to increase
    private int currentPlayerCount;
    private int targetPlayerCount;
    private int initialPlayerCount = 7817;

    private void Start()
    {
        // Assign listeners to each button using EventTrigger
        foreach (var button in buttons)
        {
            AddEventTrigger(button);
            originalYPositions[button] = button.transform.localPosition.y;
        }

        AddEventTrigger(findGameButton);
        originalYPositions[findGameButton] = findGameButton.transform.localPosition.y;


        // Initialize events for updating stats
        PlayerStats.OnPlayerLevelUpdated += UpdatePlayerLevel;
        PlayerStats.OnGoldUpdated += UpdateGold;
        PlayerStats.OnDiamondsUpdated += UpdateDiamonds;
        PlayerStats.OnPlayersOnlineUpdated += UpdatePlayersOnline;

        // Initialize player count to 0
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


    private void OnButtonPressed(Button button, Transform buttonTransform)
    {
        if (isButtonOnCooldown) return;

        AudioManager audioManager = ServiceLocator.GetAudioManager();
        audioManager.PlayMenuSFX(AudioManager.MenuSFX.Click);

        NotificationManager notificationManager = ServiceLocator.GetNotificationManager();
        notificationManager.ShowNotification(NotificationManager.NotificationType.Display, "Clicked button... 123!");

        LeanTween.cancel(button.gameObject);

        Transform buttonParent = button.gameObject.transform.parent;

        Vector3 originalScale = buttonParent.transform.localScale;
        float yOffset = button.name == "[Button] Find Game" ? -12f : -4f;
        float animationTime = 0.09997f;



        // Parent Container
        //LeanTween.scaleX(buttonParent.gameObject, originalScale.x, animationTime).setEase(LeanTweenType.easeInElastic);
        // Move Button
        LeanTween.moveLocalY(button.gameObject, originalYPositions[button] + yOffset, animationTime).setEase(LeanTweenType.easeInExpo);
    }


    private void OnButtonReleased(Button button, Transform buttonTransform)
    {
        float returnTime = 0.333f;

        LeanTween.moveLocalY(button.gameObject, originalYPositions[button], returnTime).setEase(LeanTweenType.easeOutExpo);
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

    private void OnDestroy()
    {
        // Unsubscribe from events
        PlayerStats.OnPlayerLevelUpdated -= UpdatePlayerLevel;
        PlayerStats.OnGoldUpdated -= UpdateGold;
        PlayerStats.OnDiamondsUpdated -= UpdateDiamonds;
        PlayerStats.OnPlayersOnlineUpdated -= UpdatePlayersOnline;
    }
}
