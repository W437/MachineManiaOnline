using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("Notification Settings")]
    public GameObject notificationPrefab;
    public Transform topCenterNotificationParent;
    public Transform inGameNotificationParent;

    public float displayNotificationDuration = 3f;
    public float warningNotificationDuration = 4f;
    public float inGameNotificationDuration = 2f;

    [Header("Notification Icons")]
    public Sprite displayIcon;
    public Sprite warningIcon;
    public Sprite inGameIcon;

    private List<GameObject> topCenterNotifications = new List<GameObject>();
    private List<GameObject> inGameNotifications = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            ServiceLocator.RegisterNotificationManager(this);
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
                CreateNotification(topCenterNotificationParent, message, displayNotificationDuration, topCenterNotifications, displayIcon);
                break;
            case NotificationType.Warning:
                CreateNotification(topCenterNotificationParent, message, warningNotificationDuration, topCenterNotifications, warningIcon);
                break;
            case NotificationType.InGame:
                CreateInGameNotification(inGameNotificationParent, message, inGameNotificationDuration, inGameNotifications, inGameIcon);
                break;
        }
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
        var notificationScript = notification.GetComponent<Notification>();

        notificationScript.messageText.text = message;
        notificationScript.iconImage.sprite = icon;

        notificationsList.Add(notification);
        StartCoroutine(DisplayNotification(notificationScript, duration, notificationsList));
        AdjustNotificationPositions(notificationsList);
    }

    private IEnumerator DisplayNotification(Notification notification, float duration, List<GameObject> notificationsList)
    {
        LeanTween.alphaCanvas(notification.canvasGroup, 1, 0.5f); // Fade in

        yield return new WaitForSeconds(duration);

        if (notification != null)
        {
            LeanTween.alphaCanvas(notification.canvasGroup, 0, 0.5f).setOnComplete(() =>
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
        var notificationScript = notification.GetComponent<Notification>();

        notificationScript.messageText.text = message;
        notificationScript.iconImage.sprite = icon;

        notificationsList.Add(notification);
        StartCoroutine(DisplayInGameNotification(notificationScript, duration, notificationsList));
    }

    private IEnumerator DisplayInGameNotification(Notification notification, float duration, List<GameObject> notificationsList)
    {
        RectTransform rectTransform = notification.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(0, -10, 0); // Start 10 units below the center
        LeanTween.alphaCanvas(notification.canvasGroup, 1, 0.5f); // Fade in
        LeanTween.moveLocalY(notification.gameObject, 0, 0.5f).setEase(LeanTweenType.easeOutBack); // Animate to center

        yield return new WaitForSeconds(duration);

        if (notification != null)
        {
            LeanTween.moveLocalY(notification.gameObject, 10, 0.5f).setEase(LeanTweenType.easeInBack); // Animate up
            LeanTween.alphaCanvas(notification.canvasGroup, 0, 0.5f).setOnComplete(() =>
            {
                notificationsList.Remove(notification.gameObject);
                Destroy(notification.gameObject);
            }); // Fade out
        }
    }

    private void AdjustNotificationPositions(List<GameObject> notificationsList)
    {
        float offset = 200f; // Adjust offset between notifications

        for (int i = 0; i < notificationsList.Count; i++)
        {
            GameObject notification = notificationsList[i];
            LeanTween.moveLocalY(notification, -((notificationsList.Count - 1 - i) * offset), 0.3f).setEase(LeanTweenType.easeOutExpo); // Adjust Y position
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
