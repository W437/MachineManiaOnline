using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    public const float DISPLAY_DURATION = 3f;
    public const float WARNING_DURATION = 3f;
    public const float GAME_DURATION = 2f;
    public const float SPACING = 100;

    [Header("Notification Settings")]
    [SerializeField] GameObject notificationPrefab;
    [SerializeField] Transform topCenterNotificationParent;
    [SerializeField] Transform inGameNotificationParent;

    [Header("Notification Icons")]
    [SerializeField] Sprite displayIcon;
    [SerializeField] Sprite warningIcon;
    [SerializeField] Sprite successIcon;

    [Header("Notification UIOutline Colors")]
    [SerializeField] Color displayOutlineColor = new Color32(153, 153, 153, 255);
    [SerializeField] Color warningOutlineColor = new Color32(229, 32, 32, 255);
    [SerializeField] Color successOutlineColor = new Color32(37, 185, 48, 255);

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
        Success
    }

    public void ShowNotification(NotificationType type, string message)
    {
        switch (type)
        {
            case NotificationType.Display:
                CreateNotification(topCenterNotificationParent, message, DISPLAY_DURATION, topCenterNotifications, displayIcon, displayOutlineColor);
                AudioManager.Instance.PlayMenuSFX(AudioManager.MenuSFX.Info);
                break;
            case NotificationType.Warning:
                CreateNotification(topCenterNotificationParent, message, WARNING_DURATION, topCenterNotifications, warningIcon, warningOutlineColor);
                AudioManager.Instance.PlayMenuSFX(AudioManager.MenuSFX.Warning);
                break;
            case NotificationType.Success:
                CreateNotification(topCenterNotificationParent, message, DISPLAY_DURATION, topCenterNotifications, successIcon, successOutlineColor);
                AudioManager.Instance.PlayMenuSFX(AudioManager.MenuSFX.Success);
                break;
        }
    }

    void CreateNotification(Transform parent, string message, float duration, List<GameObject> notificationsList, Sprite icon, Color outlineColor)
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

        // Update the outline color
        var uiOutline = notificationScript.IconParent.GetComponent<UIOutline>();
        uiOutline.color = outlineColor;

        notificationsList.Add(notification);
        StartCoroutine(DisplayNotification(notificationScript, duration, notificationsList));
        AdjustNotificationPositions(notificationsList);
    }

    void AdjustNotificationPositions(List<GameObject> notificationsList)
    {
        for (int i = 0; i < notificationsList.Count; i++)
        {
            GameObject notification = notificationsList[i];
            LeanTween.moveLocalY(notification, -((notificationsList.Count - 1 - i) * SPACING), 0.3f).setEase(LeanTweenType.easeOutExpo);
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
}
