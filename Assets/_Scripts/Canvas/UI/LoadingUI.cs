using Assets.Scripts.TypewriterEffects;
using Coffee.UIEffects;
using Fusion;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    public static LoadingUI Instance { get; private set; }

    // Game Loading
    [SerializeField] GameObject loadingScreen;
    [SerializeField] TextMeshProUGUI loadingText;
    [SerializeField] Slider loadingBar;
    [SerializeField] GameObject loadingPanel;
    [SerializeField] GameObject loadingContainer; // New parent for loading UI
    [SerializeField] Image gameLogo;

    // First Time Launch
    [SerializeField] GameObject firstLaunchContainer; // Parent for first launch UI
    [SerializeField] CanvasGroup loadingCanvasGroup; // CanvasGroup for fading
    [SerializeField] Image bgShadeOverlay; // BG Shade Overlay Image
    [SerializeField] TextMeshProUGUI welcomeText; // Welcome Text
    [SerializeField] GameObject usernameContainer; // Username input container
    [SerializeField] TextMeshProUGUI usernameHelperText; // Welcome Text
    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] Button continueButton;
    [SerializeField] CanvasGroup continueButtonCanvasGroup; // Continue button CanvasGroup

    public TextMeshProUGUI PlayerUniqueID;
    UITransitionEffect _imageTransitionEffect;
    Coroutine _loadingDotsCoroutine;
    string _currentLoadingMessage;
    ButtonHandler buttonHandler;

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

        _imageTransitionEffect = loadingPanel.GetComponentInChildren<UITransitionEffect>();
    }

    private void Start()
    {
        if (buttonHandler == null)
        {
            buttonHandler = gameObject.AddComponent<ButtonHandler>();
        }
        buttonHandler.AddButtonEventTrigger(continueButton, OnContinue, new ButtonConfig(yOffset: -10f, returnTime: 0.05f, rotationLock: true));
    }


    private void OnContinue(Button button)
    {
        string playerName = usernameInput.text;

        if (string.IsNullOrEmpty(playerName) || playerName.Length > 10 || playerName.Contains(" "))
        {
            NotificationManager.Instance.ShowNotification(NotificationManager.NotificationType.Warning, "Name must be non-empty, shorter than 10 characters, and contain no spaces.");
            return;
        }

        Debug.Log($"{playerName} - {PlayerData.Instance != null}");
        PlayerData.Instance.PlayerName = playerName;

        SaveSystem.SetFirstLaunchComplete();

        PlayerData.Instance.SaveStats();


        LeanTween.value(1, 0, 0.5f).setOnUpdate(value => _imageTransitionEffect.effectFactor = value)
            .setOnComplete(() => { loadingScreen.SetActive(false); });

        if (_loadingDotsCoroutine != null)
        {
            StopCoroutine(_loadingDotsCoroutine);
            _loadingDotsCoroutine = null;
        }

        AudioManager.Instance.SetCutoffFrequency(1000, 5000);
    }

    public void ShowLoadingScreen(string message = "Connecting")
    {
        AudioManager.Instance.SetCutoffFrequency(22000, 1000, 0.1f);
        loadingScreen.SetActive(true);

        _currentLoadingMessage = message;
        loadingText.text = _currentLoadingMessage;
        loadingBar.value = 0f;

        _loadingDotsCoroutine ??= StartCoroutine(AnimateLoadingDots());
    }

    public void SetProgress(float progress)
    {
        loadingBar.value = progress;
    }

    public void SetLoadingMessage(string message)
    {
        _currentLoadingMessage = message;
        loadingText.text = _currentLoadingMessage;
    }

    public void HideLoadingScreen()
    {
        if (SaveSystem.IsFirstLaunch())
        {
            HandleFirstLaunch();
        }
        else
        {
            LeanTween.value(1, 0, 0.5f).setOnUpdate(value => _imageTransitionEffect.effectFactor = value)
                .setOnComplete(() => { loadingScreen.SetActive(false); });

            if (_loadingDotsCoroutine != null)
            {
                StopCoroutine(_loadingDotsCoroutine);
                _loadingDotsCoroutine = null;
            }
        }
    }

    public void HandleFirstLaunch()
    {
        LeanTween.alphaCanvas(loadingCanvasGroup, 0, 0.5f).setOnComplete(() =>
        {
            loadingContainer.SetActive(false);
            firstLaunchContainer.SetActive(true);
            usernameInput.transform.localScale = new Vector3(0, 0.1f, 1);
            continueButtonCanvasGroup.alpha = 0;
            welcomeText.text = "";

            var usernameHelperTextCanvasGroup = usernameHelperText.GetComponent<CanvasGroup>();
            usernameHelperTextCanvasGroup.alpha = 0;

            bgShadeOverlay.rectTransform.localScale = new Vector3(1, 0, 1);
            LeanTween.scaleY(bgShadeOverlay.gameObject, 1, 0.5f).setOnComplete(() =>
            {
                var typewriter = welcomeText.GetComponent<Typewriter>();
                typewriter.Animate("<size=90>welcome to <color=#FF9E00>mmo!</color></size>\r\nenter your name to continue..");
                
                LeanTween.scaleX(usernameInput.gameObject, 1, 0.3f).setDelay(1f).setOnComplete(() =>
                {
                    LeanTween.scaleY(usernameInput.gameObject, 1, 0.3f).setOnComplete(() =>
                    {
                        LeanTween.alphaCanvas(usernameHelperTextCanvasGroup, 1, 0.5f).setOnComplete(() =>
                        {
                            LeanTween.alphaCanvas(continueButtonCanvasGroup, 1, 0.5f).setDelay(1.5f);
                        });
                    });
                });
            });
        });
    }

    private IEnumerator AnimateLoadingDots()
    {
        int _dotCount = 0;

        while (true)
        {
            loadingText.text = _currentLoadingMessage + new string('.', _dotCount);
            _dotCount = (_dotCount + 1) % 4;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
