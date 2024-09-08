using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    public const float DISPLAY_DURATION = 3f;
    public const float WARNING_DURATION = 4f;
    public const float GAME_DURATION = 2f;
    public const float SPACING = 150;

    [Header("Notification Settings")]
    [SerializeField] GameObject notificationPrefab;
    [SerializeField] Transform topCenterNotificationParent;
    [SerializeField] Transform inGameNotificationParent;

    [Header("Notification Icons")]
    [SerializeField] Sprite displayIcon;
    [SerializeField] Sprite warningIcon;
    [SerializeField] Sprite inGameIcon;

    List<GameObject> topCenterNotifications = new();
    List<GameObject> inGameNotifications = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public enum NotificationType
    {
        Display,
        Warning,
        InGame
    }

    public void ShowNotification(NotificationType type, string message)
    {
        switch (type)
        {
            case NotificationType.Display:
                CreateNotification(topCenterNotificationParent, message, DISPLAY_DURATION, topCenterNotifications, displayIcon);
                break;
            case NotificationType.Warning:
                CreateNotification(topCenterNotificationParent, message, WARNING_DURATION, topCenterNotifications, warningIcon);
                break;
            case NotificationType.InGame:
                CreateInGameNotification(inGameNotificationParent, message, GAME_DURATION, inGameNotifications, inGameIcon);
                break;
        }

        Debug.Log("Notification shown");
    }

    void CreateNotification(Transform parent, string message, float duration, List<GameObject> notificationsList, Sprite icon)
    {
        if (notificationsList.Count >= 3)
        {
            var oldNotification = notificationsList[0];
            notificationsList.RemoveAt(0);
            FadeOutAndDestroy(oldNotification);
        }

        var notification = Instantiate(notificationPrefab, parent);
        var notificationScript = notification.GetComponent<MainNotification>();

        notificationScript.MessageText.text = message;
        notificationScript.IconImage.sprite = icon;

        notificationsList.Add(notification);
        StartCoroutine(DisplayNotification(notificationScript, duration, notificationsList));
        AdjustNotificationPositions(notificationsList);
    }

    void CreateInGameNotification(Transform parent, string message, float duration, List<GameObject> notificationsList, Sprite icon)
    {
        if (notificationsList.Count >= 3)
        {
            var oldNotification = notificationsList[0];
            notificationsList.RemoveAt(0);
            FadeOutAndDestroy(oldNotification);
        }

        var notification = Instantiate(notificationPrefab, parent);
        var notificationScript = notification.GetComponent<MainNotification>();

        notificationScript.MessageText.text = message;
        notificationScript.IconImage.sprite = icon;

        notificationsList.Add(notification);
        StartCoroutine(DisplayInGameNotification(notificationScript, duration, notificationsList));
    }

    void AdjustNotificationPositions(List<GameObject> notificationsList)
    {
        for (int i = 0; i < notificationsList.Count; i++)
        {
            GameObject notification = notificationsList[i];
            LeanTween.moveLocalY(notification, -((notificationsList.Count - 1 - i) * SPACING), 0.3f).setEase(LeanTweenType.easeOutExpo); // Adjust Y pos
        }
    }

    void FadeOutAndDestroy(GameObject notification)
    {
        if (notification != null)
        {
            var canvasGroup = notification.GetComponent<CanvasGroup>();
            LeanTween.alphaCanvas(canvasGroup, 0, 0.5f).setOnComplete(() => Destroy(notification));
        }
    }
    
    IEnumerator DisplayNotification(MainNotification notification, float duration, List<GameObject> notificationsList)
    {
        LeanTween.alphaCanvas(notification.CanvasGroup, 1, 0.5f);

        yield return new WaitForSeconds(duration);

        if (notification != null)
        {
            LeanTween.alphaCanvas(notification.CanvasGroup, 0, 0.5f).setOnComplete(() =>
            {
                notificationsList.Remove(notification.gameObject);
                Destroy(notification.gameObject);
                AdjustNotificationPositions(notificationsList);
            }); 
        }
    }
    
    IEnumerator DisplayInGameNotification(MainNotification notification, float duration, List<GameObject> notificationsList)
    {
        RectTransform rectTransform = notification.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(0, -10, 0); // Start 10 units below the center
        LeanTween.alphaCanvas(notification.CanvasGroup, 1, 0.5f);
        LeanTween.moveLocalY(notification.gameObject, 0, 0.5f).setEase(LeanTweenType.easeOutBack); // Animate to center

        yield return new WaitForSeconds(duration);

        if (notification != null)
        {
            LeanTween.moveLocalY(notification.gameObject, 10, 0.5f).setEase(LeanTweenType.easeInBack); // Animate up
            LeanTween.alphaCanvas(notification.CanvasGroup, 0, 0.5f).setOnComplete(() =>
            {
                notificationsList.Remove(notification.gameObject);
                Destroy(notification.gameObject);
            });
        }
    }
}
