using UnityEngine;
using TMPro;
using Fusion;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using Cinemachine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [SerializeField] public TextMeshProUGUI raceTimerText;
    [SerializeField] TextMeshProUGUI countdownText;
    [SerializeField] TextMeshProUGUI endGameStatsText;
    [SerializeField] TextMeshProUGUI postRaceTimerText;
    [SerializeField] Transform playerProgressBar;
    [SerializeField] Transform playerProgressPrefab;
    [SerializeField] GameObject BGShadeOverlay;

    [Header("Leave Match Panel")]
    [SerializeField] GameObject leaveMatchPanel;
    [SerializeField] Button leaveMatchButton;
    [SerializeField] Button stayButton;
    [SerializeField] Button leaveButton;
    [SerializeField] Button secondaryClosePanelButton;

    [Header("Game UI")]
    [SerializeField] Button jumpButton;
    [SerializeField] Button slideButton;
    [SerializeField] Button slotButton;
    [SerializeField] Image slotSprite;
    [SerializeField] GameObject leaveMatchBtnContainer;

    [Header("Intro")]
    [SerializeField] GameObject IntroObject;
    [SerializeField] GameObject RaceTimerContainer;
    [SerializeField] CinemachineVirtualCamera vCam1;
    [SerializeField] CinemachineVirtualCamera vCam2;
    [SerializeField] GameObject leftClouds, rightClouds;
    [SerializeField] GameObject waelLogo;

    ButtonHandler buttonHandler;

    public float transitionTime;

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

    void Start()
    {
        jumpButton.gameObject.SetActive(false);
        slideButton.gameObject.SetActive(false);
        slotButton.gameObject.SetActive(false);
        RaceTimerContainer.SetActive(false);
        playerProgressBar.gameObject.SetActive(false);
        leaveMatchBtnContainer.GetComponent<CanvasGroup>().alpha = 0f;


        FadeOutBGShadeOverlay();

        buttonHandler = gameObject.AddComponent<ButtonHandler>();

        var gameBtnConfig = new ButtonConfig(activateOnPress: true, cooldownEnabled: false, returnTime: 0.05f, animationTime: 0.05f, rotationLock: true);

        buttonHandler.AddButtonEventTrigger(jumpButton, _ => { }, gameBtnConfig);
        buttonHandler.AddButtonEventTrigger(slideButton, _ => { }, gameBtnConfig);
        buttonHandler.AddButtonEventTrigger(slotButton, _ => { }, gameBtnConfig);

        var normalBtnConfig = new ButtonConfig(yOffset: -3f, rotationLock: true, animationTime: 0.1f, thresholdDistance: 300);

        buttonHandler.AddButtonEventTrigger(leaveMatchButton, OnLeaveMatchButtonClicked, new ButtonConfig(yOffset: 0, shrinkScale: 0.95f, rotationLock: true, animationTime: 0.1f));
        buttonHandler.AddButtonEventTrigger(stayButton, OnStayButtonClicked, normalBtnConfig);
        buttonHandler.AddButtonEventTrigger(leaveButton, OnLeaveButtonClicked, normalBtnConfig);
        buttonHandler.AddButtonEventTrigger(secondaryClosePanelButton, OnStayButtonClicked, new ButtonConfig(yOffset: 0, shrinkScale: 0.95f, rotationLock: true, animationTime: 0.1f));

        leaveMatchPanel.SetActive(false);
        slotSprite.gameObject.SetActive(false);

        //StartIntroSequence();
    }

    public void StartIntroSequence()
    {
        AnimateGameStartSequence();
    }


    private void AnimateGameStartSequence()
    {
        TransitionCinemachineCameras(vCam1, vCam2);
        AnimateIntroObject();
        AnimateCloudsAndLogo();

        // Buttons and Race Timer start after 2.5s
        LeanTween.delayedCall(6.5f, () =>
        {
            AnimateButtons();
            AnimatePlayerProgressBar();
            AnimateRaceTimer();

            LeanTween.delayedCall(0.5f, () =>
            {
                FadeInLeaveMatchButton();
            });
        });
    }

    private void TransitionCinemachineCameras(CinemachineVirtualCamera fromCam, CinemachineVirtualCamera toCam)
    {
        // Set initial priorities
        fromCam.Priority = 10;
        toCam.Priority = 0;

        // Use LeanTween for a delay before swapping priorities
        LeanTween.delayedCall(0.5f, () =>
        {
            // Swap priorities after 1-second delay
            fromCam.Priority = 0;
            toCam.Priority = 10;
        });
    }


    private void AnimateIntroObject()
    {
        Vector3 originalPosition = IntroObject.transform.localPosition;
        IntroObject.transform.localPosition = new Vector3(originalPosition.x, -10f, originalPosition.z);

        LeanTween.moveLocalY(IntroObject, originalPosition.y, 2.5f)
            .setEase(LeanTweenType.easeOutBack);
    }

    private void AnimateCloudsAndLogo()
    {
        Vector3 leftCloudsTarget = leftClouds.transform.localPosition + new Vector3(-5f, 0f, 0f);
        LeanTween.moveLocal(leftClouds, leftCloudsTarget, 1.5f)
            .setEase(LeanTweenType.easeInQuad);

        Vector3 rightCloudsTarget = rightClouds.transform.localPosition + new Vector3(5f, 0f, 0f);
        LeanTween.moveLocal(rightClouds, rightCloudsTarget, 1.5f)
            .setEase(LeanTweenType.easeInQuad);

        Vector3 waelLogoTarget = waelLogo.transform.localPosition + new Vector3(0f, 5f, 0f);
        LeanTween.moveLocal(waelLogo, waelLogoTarget, 2.0f)
            .setEase(LeanTweenType.easeInQuad);
        LeanTween.value(waelLogo, 0f, -15f, 2.0f)
        .setEase(LeanTweenType.easeInQuad)
        .setOnUpdate((float angle) =>
        {
            waelLogo.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        });
    }

    private void AnimateButtons()
    {
        AnimateButton(jumpButton.gameObject);
        AnimateButton(slideButton.gameObject);
        AnimateButton(slotButton.gameObject);
    }

    private void FadeInLeaveMatchButton()
    {
        CanvasGroup canvasGroup = leaveMatchBtnContainer.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = leaveMatchBtnContainer.AddComponent<CanvasGroup>();
        }

        // Set initial alpha and activate the object
        canvasGroup.alpha = 0f;
        leaveMatchBtnContainer.SetActive(true);

        // Animate the alpha to 1
        LeanTween.value(leaveMatchBtnContainer, 0f, 1f, 1.0f)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnUpdate((float alpha) =>
            {
                canvasGroup.alpha = alpha;
            });
    }


    private void AnimatePlayerProgressBar()
    {
        playerProgressBar.gameObject.SetActive(true);
        Vector3 originalPosition = playerProgressBar.transform.localPosition;
        playerProgressBar.transform.localPosition = new Vector3(originalPosition.x, originalPosition.y - 100f, originalPosition.z);

        LeanTween.moveLocalY(playerProgressBar.gameObject, originalPosition.y, 0.7f).setDelay(0.5f)
            .setEase(LeanTweenType.easeOutBack);
    }

    private void AnimateButton(GameObject button)
    {
        button.gameObject.SetActive(true);
        Vector3 originalPosition = button.transform.localPosition;
        button.transform.localPosition = new Vector3(originalPosition.x, -200f, originalPosition.z);

        LeanTween.moveLocalY(button, originalPosition.y, 0.7f)
            .setEase(LeanTweenType.easeOutBack);
    }

    private void AnimateRaceTimer()
    {
        RaceTimerContainer.SetActive(true);

        Vector3 originalPosition = RaceTimerContainer.transform.localPosition;
        RaceTimerContainer.transform.localPosition = new Vector3(originalPosition.x, originalPosition.y + 50f, originalPosition.z);

        LeanTween.moveLocalY(RaceTimerContainer, originalPosition.y, 0.7f).setDelay(1f)
            .setEase(LeanTweenType.easeOutBack);
    }

    private void FadeOutBGShadeOverlay()
    {
        CanvasGroup canvasGroup = BGShadeOverlay.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = BGShadeOverlay.AddComponent<CanvasGroup>();
        }

        // Set initial alpha
        canvasGroup.alpha = 1f;

        // Fade out over 2 seconds
        LeanTween.value(BGShadeOverlay, 1f, 0f, 3.0f)
            .setEase(LeanTweenType.easeInOutBack)
            .setOnUpdate((float alpha) =>
            {
                canvasGroup.alpha = alpha;
            })
            .setOnComplete(() =>
            {
                BGShadeOverlay.SetActive(false); // Disable the overlay after fading out
            });
    }

    public void SetSlotSprite(Sprite sprite)
    {
        if (sprite != null)
        {
            Debug.Log(slotSprite.gameObject.activeSelf);
            slotSprite.sprite = sprite;
            slotSprite.gameObject.SetActive(true);
            Debug.Log(slotSprite.gameObject.activeSelf);
        }
    }

    public void ClearSlotSprite()
    {
        slotSprite.gameObject.SetActive(false);
    }

    void OnLeaveMatchButtonClicked(Button button)
    {
        leaveMatchPanel.SetActive(true);
        leaveMatchPanel.transform.localScale = Vector3.zero;
        LeanTween.scale(leaveMatchPanel, Vector3.one, 0.3f).setEase(LeanTweenType.easeOutBack);
    }

    void OnStayButtonClicked(Button button)
    {
        LeanTween.scale(leaveMatchPanel, Vector3.zero, 0.15f).setEase(LeanTweenType.easeInBack).setOnComplete(() =>
        {
            leaveMatchPanel.SetActive(false);
        });
    }

    void OnLeaveButtonClicked(Button button)
    {
        var game = NetworkManager.Instance;
        var audio = AudioManager.Instance;

        audio.PlayRandomMusic();
        //game.StartInitialGameSession();
        Destroy(gameObject);
    }


    int? _lastCountdownValue = null; // Track last displayed countdown val
    public void DisplayCountdown(int seconds)
    {
        countdownText.gameObject.SetActive(true);

        countdownText.text = seconds > 0 ? seconds.ToString() : "GO!";

        if (seconds <= 0)
        {
            countdownText.color = Color.white;
            StartCoroutine(HideCountdownCoroutine());
        }
        else
        {
            countdownText.color = Color.white; 
        }

        // Trigger the "POP" animation only if the val changes
        if (_lastCountdownValue != seconds)
        {
            _lastCountdownValue = seconds;
            AnimatePop(countdownText.gameObject);
        }
    }


    private void AnimatePop(GameObject target)
    {
        target.transform.localScale = Vector3.one;

        LeanTween.scale(target, Vector3.one * 1.5f, 0.2f) 
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                LeanTween.scale(target, Vector3.one, 0.2f)
                    .setEase(LeanTweenType.easeInQuad);
            });
    }


    IEnumerator HideCountdownCoroutine()
    {
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }

    public void UpdateRaceTimer(float elapsedTime)
    {
        raceTimerText.text = FormatTime(elapsedTime);
    }


    private LTDescr flashTween; // Single tween for syncd animation
    private float animationDuration = .55f; // Init duration

    public void DisplayPostRaceCountdown(string message)
    {
        if (float.TryParse(message, out float timeLeft))
        {
            postRaceTimerText.text = $"Hurry! {Mathf.CeilToInt(timeLeft)}";
        }
        else
        {
            postRaceTimerText.text = message;
        }

        if (message == "Time's up..")
        {
            StopFlashingText(permanentRed: true, finalPop: true);
        }
        else if (float.TryParse(message, out float timeLeftCheck) && timeLeftCheck <= 10f)
        {
            if (flashTween == null)
            {
                animationDuration = .55f; // Reset speed for new countdown
                StartFlashingText();
            }
        }
        else
        {
            StopFlashingText(permanentRed: false, finalPop: false);
        }
    }

    private void StartFlashingText()
    {
        StopFlashingText(permanentRed: false, finalPop: false);

        Vector3 originalScale = postRaceTimerText.transform.localScale;
        Vector3 popScale = originalScale * 1.2f;

        flashTween = LeanTween.value(gameObject, 0f, 1f, animationDuration)
            .setEaseInOutSine()
            .setOnUpdate((float t) =>
            {
                postRaceTimerText.color = Color.Lerp(Color.white, Color.red, t);

                postRaceTimerText.transform.localScale = Vector3.Lerp(originalScale, popScale, t);
            })
            .setLoopPingPong()
            .setOnComplete(() =>
            {
                animationDuration *= 0.94f;
            });
    }

    private void StopFlashingText(bool permanentRed, bool finalPop)
    {
        if (flashTween != null)
        {
            LeanTween.cancel(gameObject);
            flashTween = null;
        }

        if (finalPop)
        {
            Vector3 originalScale = postRaceTimerText.transform.localScale;
            Vector3 popScale = originalScale * 1.3f;

            LeanTween.scale(postRaceTimerText.gameObject, popScale, 0.85f)
                .setEaseOutBounce()
                .setOnComplete(() =>
                {
                    postRaceTimerText.color = Color.red;
                });
        }
        else if (permanentRed)
        {
            postRaceTimerText.color = Color.red;
        }
        else
        {
            postRaceTimerText.color = Color.white;
        }
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time - minutes * 60);
        float milliseconds = (time - minutes * 60 - seconds) * 1000;
        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, Mathf.Floor(milliseconds));
    }

    public void UpdatePlayerProgress(PlayerRef player, float progress)
    {
        Transform playerProgress = playerProgressBar.Find(player.PlayerId.ToString());
        if (playerProgress == null)
        {
            playerProgress = Instantiate(playerProgressPrefab, playerProgressBar);
            playerProgress.name = player.PlayerId.ToString();
        }
        RectTransform progressBarRect = playerProgressBar.GetComponent<RectTransform>();
        playerProgress.localPosition = new Vector3(progress * progressBarRect.rect.width, playerProgress.localPosition.y, playerProgress.localPosition.z);
    }

    public void DisplayEndGameStats(Dictionary<PlayerRef, float> playerFinishTimes)
    {
        endGameStatsText.gameObject.SetActive(true);
        endGameStatsText.text = "Game Over\n\n";
        int rank = 1;
        foreach (var playerFinishTime in playerFinishTimes)
        {
            endGameStatsText.text += $"{rank}. Player {playerFinishTime.Key.PlayerId}: {FormatTime(playerFinishTime.Value)}\n";
            rank++;
        }
    }

    public void UpdateRaceTimer(string timerString)
    {
        if (raceTimerText != null)
        {
            raceTimerText.text = timerString;
        }
    }

    public void HideEndGameStats()
    {
        endGameStatsText.gameObject.SetActive(false);
    }

    public void ShowWaitingOverlay(bool value)
    {
        BGShadeOverlay.gameObject.SetActive(value);
    }

    public void UpdateWaitingText(string message)
    {
        TextMeshProUGUI waitingText = BGShadeOverlay.GetComponentInChildren<TextMeshProUGUI>();
        if (waitingText != null)
        {
            waitingText.text = message;
        }
        else
        {
            Debug.LogWarning("WaitingText not found on WaitingOverlay.");
        }
    }

}
