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
    [SerializeField] public GameObject lobbyPlatformScreen;
    [SerializeField] Button lobbyReadyButton;
    [SerializeField] Button lobbyLeaveButton;
    public Transform PlayerPositionsParent;
    public TextMeshProUGUI gameStartLobbyTimer;

    [Header("Connecting Overlay")]
    [SerializeField] public GameObject ConnectingOverlay;
    [SerializeField] public TextMeshProUGUI ConnectingText;
    Coroutine connectingCoroutine;
    ButtonHandler buttonHandler;

    [Header("Mania News")]
    [SerializeField] LobbyManiaNews maniaNews;

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
        buttonHandler.AddButtonEventTrigger(lobbyReadyButton, OnLobbyReady, new ButtonConfig(toggle: true, yOffset: -14f, rotationLock: true));
        buttonHandler.AddButtonEventTrigger(lobbyLeaveButton, OnLobbyLeave, new ButtonConfig(callbackDelay: 0.1f, rotationLock: true));
    }

    private IEnumerator CheckIfLobbyIsSpawned()
    {
        connectingCoroutine = StartCoroutine(AnimateConnectingText());
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (LobbyManager.Instance != null && LobbyManager.Instance.isSpawned)
            {
                HideConnectingOverlay();
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void ConnectToLobby()
    {
        lobbyPlatformScreen.SetActive(true);
        ConnectingOverlay.SetActive(true);
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

        PlayerRef player = FusionLauncher.Instance.GetNetworkRunner().LocalPlayer;
        //LobbyManager.Instance.RPC_StartGame();

        if (!LobbyManager.Instance.IsPlayerReady(player))
        {
            LobbyManager.Instance.SetPlayerReadyState(FusionLauncher.Instance.GetNetworkRunner().LocalPlayer);
        }
        else
        {
            LobbyManager.Instance.SetPlayerReadyState(FusionLauncher.Instance.GetNetworkRunner().LocalPlayer);
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
