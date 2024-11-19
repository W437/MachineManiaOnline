using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Fusion;
using UnityEngine.UI;
using TMPro;

public class EndGameUI : MonoBehaviour
{
    public static EndGameUI Instance;

    [SerializeField] private GameObject MainCanvas;
    [SerializeField] private GameObject EndCanvas;

    [Header("Cameras")]
    [SerializeField] private CinemachineVirtualCamera vEndCam;
    [SerializeField] private CinemachineVirtualCamera vEndCam2;
     
    [Header("Podium")]
    [SerializeField] private Transform podium1st;
    [SerializeField] private Transform podium2nd;
    [SerializeField] private Transform podium3rd;


    [Header("UI Elements")]
    [SerializeField] private GameObject raceGainedStats;
    [SerializeField] private GameObject leftButtons;
    [SerializeField] private GameObject continueBtnContainer;
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button emoteBtn;
    [SerializeField] private Button screenshotBtn;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI crystalsText;
    [SerializeField] private TextMeshProUGUI xpText;

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

    public void ShowEndGameUI()
    {
        // 1.
        MainCanvas.SetActive(false);
        EndCanvas.SetActive(true);

        // temp disable
        raceGainedStats.SetActive(false);
        leftButtons.SetActive(false);
        continueBtnContainer.SetActive(false);

        // 2.
        TransitionToEndCameras();

        // 3.
        AnimateEndUI();
    }

    private void AnimateEndUI()
    {
        LeanTween.delayedCall(1.5f, () =>
        {
            Vector3 originalStatsPosition = raceGainedStats.transform.localPosition;
            raceGainedStats.transform.localPosition = new Vector3(originalStatsPosition.x, originalStatsPosition.y + 100, originalStatsPosition.z);
            raceGainedStats.SetActive(true);

            LeanTween.moveLocalY(raceGainedStats, originalStatsPosition.y, 1.0f)
                .setEase(LeanTweenType.easeOutQuart);
        });

        LeanTween.delayedCall(2, () =>
        {
            Vector3 originalLeftButtonsPosition = leftButtons.transform.localPosition;
            leftButtons.transform.localPosition = new Vector3(originalLeftButtonsPosition.x - 150, originalLeftButtonsPosition.y, originalLeftButtonsPosition.z);
            leftButtons.SetActive(true);

            LeanTween.moveLocalX(leftButtons, originalLeftButtonsPosition.x, 1.0f)
                .setEase(LeanTweenType.easeOutQuart);
        });

        LeanTween.delayedCall(2.5f, () =>
        {
            Vector3 originalContinuePosition = continueBtnContainer.transform.localPosition;
            continueBtnContainer.transform.localPosition = new Vector3(originalContinuePosition.x, originalContinuePosition.y - 150, originalContinuePosition.z);
            continueBtnContainer.SetActive(true);

            LeanTween.moveLocalY(continueBtnContainer, originalContinuePosition.y, 1.0f)
                .setEase(LeanTweenType.easeOutQuart);
        });
    }

    public void SetupRankings(List<NetworkObject> sortedPlayerObjects)
    {
        if (sortedPlayerObjects.Count > 0)
        {
            PlacePlayerOnPodium(sortedPlayerObjects[0], podium1st);
        }
        if (sortedPlayerObjects.Count > 1)
        {
            PlacePlayerOnPodium(sortedPlayerObjects[1], podium2nd); 
        }
        if (sortedPlayerObjects.Count > 2)
        {
            PlacePlayerOnPodium(sortedPlayerObjects[2], podium3rd); 
        }
    }

    private void PlacePlayerOnPodium(NetworkObject playerObject, Transform podium)
    {
        if (playerObject != null)
        {
            playerObject.transform.position = podium.position;
            playerObject.transform.rotation = podium.rotation;
        }
    }

    private void TransitionToEndCameras()
    {
        vEndCam.Priority = 20;
        vEndCam2.Priority = 0;

        LeanTween.delayedCall(.1f, () =>
        {
            vEndCam.Priority = 0;
            vEndCam2.Priority = 20;
        });
    }
}
