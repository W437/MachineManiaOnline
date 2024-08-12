using UnityEngine;
using TMPro;
using Fusion;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [SerializeField] public TextMeshProUGUI raceTimerText;
    [SerializeField] TextMeshProUGUI countdownText;
    [SerializeField] TextMeshProUGUI endGameStatsText;
    [SerializeField] TextMeshProUGUI postRaceTimerText;
    [SerializeField] Transform playerProgressBar;
    [SerializeField] Transform playerProgressPrefab;

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

    private ButtonHandler buttonHandler;

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
    // bass belka3 fik tibne asas llmostakbal
    private void Start()
    {
        buttonHandler = gameObject.AddComponent<ButtonHandler>();

        var gameBtnConfig = new ButtonConfig(activateOnPress: true, cooldownEnabled: false, returnTime: 0.05f, animationTime: 0.05f, rotationLock: true, realTimeUpdate: false);

        buttonHandler.AddButtonEventTrigger(jumpButton, _ => { } , gameBtnConfig);
        buttonHandler.AddButtonEventTrigger(slideButton, _ => { }, gameBtnConfig);
        buttonHandler.AddButtonEventTrigger(slotButton, _ => { }, gameBtnConfig);

        var normalBtnConfig = new ButtonConfig(yOffset: -3f, rotationLock: true, animationTime: 0.1f, thresholdDistance: 300);

        buttonHandler.AddButtonEventTrigger(leaveMatchButton, OnLeaveMatchButtonClicked, new ButtonConfig(yOffset: 0, shrinkScale: 0.95f, rotationLock: true, animationTime: 0.1f));
        buttonHandler.AddButtonEventTrigger(stayButton, OnStayButtonClicked, normalBtnConfig);
        buttonHandler.AddButtonEventTrigger(leaveButton, OnLeaveButtonClicked, normalBtnConfig);
        buttonHandler.AddButtonEventTrigger(secondaryClosePanelButton, OnStayButtonClicked, new ButtonConfig(yOffset: 0, shrinkScale: 0.95f, rotationLock: true, animationTime: 0.1f));

        leaveMatchPanel.SetActive(false);
    }

    private void OnLeaveMatchButtonClicked(Button button)
    {
        leaveMatchPanel.SetActive(true);
        leaveMatchPanel.transform.localScale = Vector3.zero;
        LeanTween.scale(leaveMatchPanel, Vector3.one, 0.3f).setEase(LeanTweenType.easeOutBack);
    }

    private void OnStayButtonClicked(Button button)
    {
        LeanTween.scale(leaveMatchPanel, Vector3.zero, 0.15f).setEase(LeanTweenType.easeInBack).setOnComplete(() =>
        {
            leaveMatchPanel.SetActive(false);
        });
    }

    private void OnLeaveButtonClicked(Button button)
    {
        FusionLauncher.Instance.GetNetworkRunner().Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
    }

    public void DisplayCountdown(int seconds)
    {
        countdownText.gameObject.SetActive(true);
        countdownText.text = seconds > 0 ? seconds.ToString() : "GO!";
        if (seconds <= 0)
        {
            StartCoroutine(HideCountdownCoroutine());
        }
    }

    private IEnumerator HideCountdownCoroutine()
    {
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }

    private IEnumerator CountdownCoroutine(int seconds)
    {
        while (seconds > 0)
        {
            countdownText.text = seconds.ToString();
            yield return new WaitForSeconds(1f);
            seconds--;
        }
        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }

    public void HideCountdown()
    {
        countdownText.gameObject.SetActive(false);
    }

    public void StartRaceTimer()
    {
        raceTimerText.gameObject.SetActive(true);
    }

    public void UpdateRaceTimer(float elapsedTime)
    {
        raceTimerText.text = FormatTime(elapsedTime);
    }

    private IEnumerator RaceTimerCoroutine()
    {
        float startTime = Time.time;
        while (true)
        {
            float elapsedTime = Time.time - startTime;
            raceTimerText.text = FormatTime(elapsedTime);
            yield return null;
        }
    }

    public void DisplayPostRaceCountdown(float timeLeft)
    {
        postRaceTimerText.gameObject.SetActive(true);
        postRaceTimerText.text = "Next Race in: " + Mathf.CeilToInt(timeLeft).ToString();
    }

    private string FormatTime(float time)
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

    public void HideEndGameStats()
    {
        endGameStatsText.gameObject.SetActive(false);
    }
}
