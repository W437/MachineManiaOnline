using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static NotificationManager;
using System;

public class LoadingUI : MonoBehaviour
{
    public static LoadingUI Instance { get; private set; }

    [SerializeField] private Slider loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private GameObject firstLaunchContainer;
    [SerializeField] private CanvasGroup loadingCanvasGroup;

    [Header("First time Launch")]
    [SerializeField] private TextMeshProUGUI welcomeText;
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private Button continueButton;

    public event Action<NotificationType, string> OnNotification;
    public event Action<string> OnPlayerNameSet; // Event to notify GameInitiator

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetProgress(float progress)
    {
        loadingBar.value = Mathf.Clamp01(progress);
    }

    public void SetLoadingMessage(string message)
    {
        if (loadingText != null)
        {
            loadingText.text = message;
        }
    }

    public void StartFirstLaunchSequence()
    {
        firstLaunchContainer.SetActive(true);
        welcomeText.text = "";

        AnimateFirstLaunch();
    }

    private void AnimateFirstLaunch()
    {
        loadingCanvasGroup.alpha = 1;
        LeanTween.alphaCanvas(loadingCanvasGroup, 0, 0.5f).setOnComplete(() =>
        {
            firstLaunchContainer.SetActive(true);
            welcomeText.text = "<size=90>Welcome to <color=#FF9E00>MMO!</color></size>\nEnter your name to continue...";

            usernameInput.transform.localScale = new Vector3(0, 0.1f, 1);
            LeanTween.scale(usernameInput.gameObject, new Vector3(1, 1, 1), 0.5f).setDelay(0.5f);
            LeanTween.alphaCanvas(continueButton.GetComponent<CanvasGroup>(), 1, 0.5f).setDelay(1.5f);

            continueButton.onClick.RemoveAllListeners(); // Clean up any existing listeners
            continueButton.onClick.AddListener(OnContinueButtonPressed);
        });
    }

    private void OnContinueButtonPressed()
    {
        string playerName = usernameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName) || playerName.Length > 10 || playerName.Contains(" "))
        {
            OnNotification?.Invoke(NotificationType.Warning, "Name must be non-empty, shorter than 10 characters, and contain no spaces.");
            return;
        }

        // Notify the GameInitiator about the player name
        OnPlayerNameSet?.Invoke(playerName);

        // Hide the first launch container and bring back the loading UI
        LeanTween.alphaCanvas(loadingCanvasGroup, 1, 0.5f).setOnComplete(() =>
        {
            firstLaunchContainer.SetActive(false);
            loadingCanvasGroup.alpha = 0;
        });
    }
}
