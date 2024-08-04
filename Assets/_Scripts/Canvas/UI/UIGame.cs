using UnityEngine;
using TMPro;
using Fusion;
using System.Collections.Generic;
using System.Collections;

public class UIGame : MonoBehaviour
{
    public static UIGame Instance;

    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI raceTimerText;
    [SerializeField] private TextMeshProUGUI endGameStatsText;
    [SerializeField] private Transform playerProgressBar;
    [SerializeField] private Transform playerProgressPrefab;

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

    public void DisplayCountdown(int seconds)
    {
        countdownText.gameObject.SetActive(true);
        StartCoroutine(CountdownCoroutine(seconds));
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
        StartCoroutine(RaceTimerCoroutine());
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
