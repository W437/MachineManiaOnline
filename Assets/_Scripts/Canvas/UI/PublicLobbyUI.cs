using Fusion;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PublicLobbyUI : MonoBehaviour
{
    public static PublicLobbyUI Instance;

    [Header("Lobby")]
    public GameObject lobbyPlatformScreen;
    public Transform maniaNewsParent;
    public Transform PlayerSlotsParent;
    public TextMeshProUGUI gameStartLobbyTimer;
    [SerializeField] Button lobbyReadyButton;
    [SerializeField] Button lobbyLeaveButton;

    [Header("Connecting Overlay")]
    [SerializeField] public GameObject ConnectingOverlay;
    [SerializeField] public TextMeshProUGUI ConnectingText;
    Coroutine connectingCoroutine;
    ButtonHandler buttonHandler;

    public ButtonHandler ButtonHandler { get { return buttonHandler; } }

    void Awake()
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

        buttonHandler = gameObject.GetComponent<ButtonHandler>();
    }

    void Start()
    {
        buttonHandler.AddButtonEventTrigger(lobbyReadyButton, OnLobbyReady, new ButtonConfig(toggle: true,  yOffset: -14f, rotationLock: false));
        buttonHandler.AddButtonEventTrigger(lobbyLeaveButton, OnLobbyLeave, new ButtonConfig(callbackDelay: 0.1f, rotationLock: true));
    }

    public void ConnectToLobby()
    {
        lobbyPlatformScreen.SetActive(true);
        ConnectingOverlay.SetActive(true);
        DelayedCheckIfLobbyIsSpawned();
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

    void OnLobbyReady(Button button)
    {
        PlayerRef localPlayerRef = FusionLauncher.Instance.Runner().LocalPlayer;
        var playerObject = FusionLauncher.Instance.Runner().GetPlayerObject(localPlayerRef);

        if (localPlayerRef != null)
        {
            var playerManager = playerObject.GetComponent<PlayerManager>();

            if (playerManager != null)
            {
                bool currentReadyState = playerManager.IsReady;
                Debug.Log($"current ready state {currentReadyState}");
                PublicLobbyManager.Instance.SetPlayerReadyState(localPlayerRef);
            }
        }
    }

    void OnLobbyLeave(Button button)
    {
        AudioManager.Instance.SetCutoffFrequency(7000, 1);
        FusionLauncher.Instance.Runner().LoadScene(SceneRef.FromIndex(0), LoadSceneMode.Single);
    }
  
    IEnumerator CheckIfLobbyIsSpawned()
    {
        connectingCoroutine = StartCoroutine(AnimateConnectingText());
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            if (PublicLobbyManager.Instance != null && PublicLobbyManager.Instance.net_isSpawned)
            {
                HideConnectingOverlay();
                yield break;
            }
        }
    }

    IEnumerator DelayedCheckIfLobbyIsSpawned()
    {
        // slight delay to allow everything to initialize properly
        yield return new WaitForSeconds(1.0f);
        StartCoroutine(CheckIfLobbyIsSpawned());
    }

    IEnumerator AnimateConnectingText()
    {
        string baseText = "Connecting to Session";
        int dotCount = 0;
 
        while (true)
        {
            ConnectingText.text = baseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
