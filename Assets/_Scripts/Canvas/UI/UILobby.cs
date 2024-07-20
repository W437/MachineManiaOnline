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
    [SerializeField] private GameObject lobbyPlatformScreen;
    [SerializeField] private Button lobbyReadyButton;
    [SerializeField] private Button lobbyLeaveButton;
    public Transform playerPositionsParent;

    [Header("Connecting Overlay")]
    [SerializeField] private GameObject connectingOverlay;
    [SerializeField] private TextMeshProUGUI connectingText;
    private Coroutine _connectingCoroutine;
    private ButtonHandler _buttonHandler;

    [Header("Mania News")]
    [SerializeField] private LobbyManiaNews maniaNews;

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
        _buttonHandler = gameObject.AddComponent<ButtonHandler>();
        _buttonHandler.AddEventTrigger(lobbyReadyButton, OnLobbyReadyButtonReleased, new ButtonConfig(toggle: true, yOffset: -14f, rotationLock: true));
        _buttonHandler.AddEventTrigger(lobbyLeaveButton, OnLobbyLeaveButtonReleased, new ButtonConfig(callbackDelay: 0.1f, rotationLock: true));
    }

    private IEnumerator CheckIfLobbyIsSpawned()
    {
        _connectingCoroutine = StartCoroutine(AnimateConnectingText());
        while (true)
        {
            yield return new WaitForSeconds(1f);
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
        connectingOverlay.SetActive(true);
        StartCoroutine(CheckIfLobbyIsSpawned());
    }

    public void HideConnectingOverlay()
    {
        if (connectingOverlay != null)
        {
            connectingOverlay.SetActive(false);
            if (_connectingCoroutine != null)
            {
                StopCoroutine(_connectingCoroutine);
                _connectingCoroutine = null;
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
            connectingText.text = baseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnLobbyReadyButtonReleased(Button button)
    {
        if (!(FusionLauncher.Instance.GetNetworkRunner() != null && FusionLauncher.Instance.GetNetworkRunner().IsRunning)) return;

        PlayerRef player = FusionLauncher.Instance.GetNetworkRunner().LocalPlayer;
        LobbyManager.Instance.RPC_StartGame();

        if (!LobbyManager.Instance.IsPlayerReady(player))
        {
            LobbyManager.Instance.SetPlayerReadyState(FusionLauncher.Instance.GetNetworkRunner().LocalPlayer);
        }
        else
        {
            LobbyManager.Instance.SetPlayerReadyState(FusionLauncher.Instance.GetNetworkRunner().LocalPlayer);
        }
    }

    private void OnLobbyLeaveButtonReleased(Button button)
    {
        AudioManager.Instance.SetCutoffFrequency(7000, 10000);
        _buttonHandler.ResetButtonToggleState(lobbyLeaveButton);
        lobbyPlatformScreen.SetActive(false);
        string uniqueSessionName = GameLauncher.Instance.GenerateUniqueSessionName();
        GameLauncher.Instance.Launch(uniqueSessionName, false);
    }
}
