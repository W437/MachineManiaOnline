using Coffee.UIExtensions;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance;

    [Header("Lobby")]
    [SerializeField] public GameObject lobbyPlatformScreen;
    public Transform PlayerSlotsParent;
    public TextMeshProUGUI gameStartLobbyTimer;
    [SerializeField] Button lobbyReadyButton;
    [SerializeField] Button lobbyLeaveButton;

    [Header("Connecting Overlay")]
    [SerializeField] public GameObject ConnectingOverlay;
    [SerializeField] public TextMeshProUGUI ConnectingText;
    Coroutine connectingCoroutine;
    ButtonHandler buttonHandler;

    [Header("Mania News")]
    [SerializeField] ManiaNews maniaNews;

    [Header("Private Lobby Chat")]
    public GameObject ChatPanel;
    public ScrollRect MessageScrollView;
    public TMP_InputField MessageInputField;
    public Transform MessageContent;
    public Button SendMessageButton;
    public bool ChatVisible = false;
    public GameObject ChatBG;
    public RectTransform ChatContainer;
    public float FadeDuration = 0.25f;
    public float SlideDuration = 0.5f;
    public Button BGExitButton;
    public Button ChatExitButton;

    public ButtonHandler ButtonHandler { get { return buttonHandler; } }

    private void Awake()
    {
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
        if(buttonHandler == null)
        {
            buttonHandler = gameObject.AddComponent<ButtonHandler>();
        }
        buttonHandler.AddButtonEventTrigger(lobbyReadyButton, OnLobbyReady, new ButtonConfig(toggle: true,  yOffset: -14f, rotationLock: false));
        buttonHandler.AddButtonEventTrigger(lobbyLeaveButton, OnLobbyLeave, new ButtonConfig(callbackDelay: 0.1f, rotationLock: true));
    }

    private IEnumerator CheckIfLobbyIsSpawned()
    {
        connectingCoroutine = StartCoroutine(AnimateConnectingText());
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            if (PublicLobbyManager.Instance != null && PublicLobbyManager.Instance.net_IsSpawned)
            {
                HideConnectingOverlay();
                yield break;
            }
        }
    }


    public void ConnectToLobby()
    {
        lobbyPlatformScreen.SetActive(true);
        ConnectingOverlay.SetActive(true);
        StartCoroutine(DelayedCheckIfLobbyIsSpawned());
    }

    private IEnumerator DelayedCheckIfLobbyIsSpawned()
    {
        // Adding a slight delay to allow everything to initialize properly
        Debug.Log($"Delayed check for lobby spawn");
        yield return new WaitForSeconds(1.0f);
        StartCoroutine(CheckIfLobbyIsSpawned());
    }


    public void HideConnectingOverlay()
    {
        if (ConnectingOverlay != null)
        {
            ConnectingOverlay.SetActive(false);
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

        // Run until stopped from elsewhere
        while (true)
        {
            ConnectingText.text = baseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnLobbyReady(Button button)
    {
        if (!(FusionLauncher.Instance.GetNetworkRunner() != null && FusionLauncher.Instance.GetNetworkRunner().IsRunning)) return;

        PlayerRef localPlayerRef = FusionLauncher.Instance.GetNetworkRunner().LocalPlayer;
        var playerObject = FusionLauncher.Instance.GetNetworkRunner().GetPlayerObject(localPlayerRef);
        Debug.Log($"Clicked ready {playerObject != null}");
        if (localPlayerRef != null)
        {
            var playerManager = playerObject.GetComponent<PlayerManager>();
            Debug.Log($"player manager {playerManager}");
            if (playerManager != null)
            {
                bool currentReadyState = playerManager.net_IsReady;
                playerManager.SetPlayerReady(!currentReadyState);

                Debug.Log($"[!] Status: {playerManager.net_IsReady}");
                UpdateReadyStateUI(localPlayerRef, !currentReadyState);
            }
        }
    }

    private void UpdateReadyStateUI(PlayerRef player, bool isReady)
    {
        int playerIndex = PublicLobbyManager.Instance.FindPlayerPosition(player);
        if (playerIndex >= 0)
        {
            Transform slotTransform = PlayerSlotsParent.GetChild(playerIndex);
            var statusText = slotTransform.Find("statusText").GetComponent<TextMeshProUGUI>();
            if (statusText != null)
            {
                statusText.text = isReady ? "Ready" : "Not Ready";
            }
        }
    }

    private void OnLobbyLeave(Button button)
    {
        AudioManager.Instance.SetCutoffFrequency(7000, 10000);
        buttonHandler.ResetButtonToggleState(lobbyLeaveButton);
        lobbyPlatformScreen.SetActive(false);
        string uniqueSessionName = GameLauncher.Instance.GenerateUniqueSessionName();
        GameLauncher.Instance.Launch(uniqueSessionName, false);
    }
}
