using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("Notification Settings")]
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private Transform topCenterNotificationParent;
    [SerializeField] private Transform inGameNotificationParent;

    public const float DISPLAY_NOTIFICATION_DURATION = 3f;
    public const float WARNING_NOTIFICATION_DURATION = 4f;
    public const float GAME_NOTIFICATION_DURATION = 2f;

    [Header("Notification Icons")]
    [SerializeField] private Sprite displayIcon;
    [SerializeField] private Sprite warningIcon;
    [SerializeField] private Sprite inGameIcon;

    private List<GameObject> topCenterNotifications = new();
    private List<GameObject> inGameNotifications = new();

    private void Awake()
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
                CreateNotification(topCenterNotificationParent, message, DISPLAY_NOTIFICATION_DURATION, topCenterNotifications, displayIcon);
                break;
            case NotificationType.Warning:
                CreateNotification(topCenterNotificationParent, message, WARNING_NOTIFICATION_DURATION, topCenterNotifications, warningIcon);
                break;
            case NotificationType.InGame:
                CreateInGameNotification(inGameNotificationParent, message, GAME_NOTIFICATION_DURATION, inGameNotifications, inGameIcon);
                break;
        }

        Debug.Log("Notification shown");
    }

    private void CreateNotification(Transform parent, string message, float duration, List<GameObject> notificationsList, Sprite icon)
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

    private IEnumerator DisplayNotification(MainNotification notification, float duration, List<GameObject> notificationsList)
    {
        LeanTween.alphaCanvas(notification.CanvasGroup, 1, 0.5f); // Fade in

        yield return new WaitForSeconds(duration);

        if (notification != null)
        {
            LeanTween.alphaCanvas(notification.CanvasGroup, 0, 0.5f).setOnComplete(() =>
            {
                notificationsList.Remove(notification.gameObject);
                Destroy(notification.gameObject);
                AdjustNotificationPositions(notificationsList);
            }); // Fade out
        }
    }

    private void CreateInGameNotification(Transform parent, string message, float duration, List<GameObject> notificationsList, Sprite icon)
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

    private IEnumerator DisplayInGameNotification(MainNotification notification, float duration, List<GameObject> notificationsList)
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

    private void AdjustNotificationPositions(List<GameObject> notificationsList)
    {
        float _offset = 150f;

        for (int i = 0; i < notificationsList.Count; i++)
        {
            GameObject notification = notificationsList[i];
            LeanTween.moveLocalY(notification, -((notificationsList.Count - 1 - i) * _offset), 0.3f).setEase(LeanTweenType.easeOutExpo); // Adjust Y position
        }
    }

    private void FadeOutAndDestroy(GameObject notification)
    {
        if (notification != null)
        {
            var canvasGroup = notification.GetComponent<CanvasGroup>();
            LeanTween.alphaCanvas(canvasGroup, 0, 0.5f).setOnComplete(() => Destroy(notification));
        }
    }
}
