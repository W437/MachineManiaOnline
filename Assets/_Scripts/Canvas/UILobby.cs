using Coffee.UIExtensions;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILobby : MonoBehaviour
{
    public static UILobby Instance;

    [Header("Lobby")]
    public GameObject lobbyPlatformScreen;
    public Button lobbyReadyButton;
    public Button lobbyLeaveButton;
    public Transform playerPositionsParent;

    [Header("Connecting Overlay")]
    public GameObject connectingOverlay;
    public TextMeshProUGUI connectingText;

    private Coroutine connectingCoroutine;
    private ButtonHandler buttonHandler;

    [SerializeField] private LobbyManiaNews maniaNews;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        buttonHandler = gameObject.AddComponent<ButtonHandler>();
        buttonHandler.AddEventTrigger(lobbyReadyButton, OnLobbyReadyButtonReleased, new ButtonConfig(toggle: true, yOffset: -14f, rotationLock: true));
        buttonHandler.AddEventTrigger(lobbyLeaveButton, OnLobbyLeaveButtonReleased, new ButtonConfig(callbackDelay: 0.1f, rotationLock: true));
    }

    private IEnumerator CheckIfLobbyIsSpawned()
    {
        connectingCoroutine = StartCoroutine(AnimateConnectingText());
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (LobbyManager.Instance != null && LobbyManager.Instance.isSpawned)
            {
                HideConnectingOverlay();
                LobbyManager.Instance.UpdatePlayerList();
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void ConnectToLobby()
    {
        lobbyPlatformScreen.SetActive(true);
        connectingOverlay.SetActive(true);
        StartCoroutine(CheckIfLobbyIsSpawned());
    }

    public void HideConnectingOverlay()
    {
        if (connectingOverlay != null)
        {
            connectingOverlay.SetActive(false);
            if (connectingCoroutine != null)
            {
                StopCoroutine(connectingCoroutine);
                connectingCoroutine = null;
            }
        }
    }

    private IEnumerator AnimateConnectingText()
    {
        string baseText = "Starting Session";
        int dotCount = 0;

        // Run indefinitely until the coroutine is stopped from elsewhere
        while (true)
        {
            connectingText.text = baseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4; // Cycle dot count from 0 to 3
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnLobbyReadyButtonReleased(Button button)
    {
        if (!(FusionLauncher.Instance.GetNetworkRunner() != null && FusionLauncher.Instance.GetNetworkRunner().IsRunning)) return;

        PlayerRef player = FusionLauncher.Instance.GetNetworkRunner().LocalPlayer;

        if (!ServiceLocator.GetLobbyManager().IsPlayerReady(player))
        {
            ServiceLocator.GetLobbyManager().SetPlayerReadyState(FusionLauncher.Instance.GetNetworkRunner().LocalPlayer);
        }
        else
        {
            ServiceLocator.GetLobbyManager().SetPlayerReadyState(FusionLauncher.Instance.GetNetworkRunner().LocalPlayer);
        }

        ServiceLocator.GetLobbyManager().UpdatePlayerList();
    }

    private void OnLobbyLeaveButtonReleased(Button button)
    {
        ServiceLocator.GetAudioManager().SetCutoffFrequency(7000, 10000);
        buttonHandler.ResetButtonToggleState(lobbyLeaveButton);
        lobbyPlatformScreen.SetActive(false);
        string uniqueSessionName = GameLauncher.Instance.GenerateUniqueSessionName();
        GameLauncher.Instance.Launch(uniqueSessionName, false);
    }
}
