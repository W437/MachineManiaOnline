using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Linq;
using Fusion.Addons.Physics;
using Random = UnityEngine.Random;
using Assets.Scripts.TypewriterEffects;
using System.Threading.Tasks;

public class LobbyManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static LobbyManager Instance;

    private SceneRef _gameScene;
    public bool isSpawned = false;


    // Lobby stuff
    [SerializeField] private NetworkPrefabRef net_PlayerPrefab;
    [Networked, Capacity(6)] private NetworkDictionary<PlayerRef, NetworkBool> net_ReadyStates => default;
    [Networked, Capacity(6)] private NetworkLinkedList<PlayerRef> net_PlayerList => default;
    [Networked, Capacity(6)] private NetworkDictionary<PlayerRef, int> net_PlayerPositionsMap => default;
    [Networked] private TickTimer net_GameStartTimer { get; set; }
    [Networked, Capacity(6)] private NetworkArray<PlayerInfo> net_PlayerPositions => default;
    [Networked] private NetworkBool net_NewsStarted { get; set; }
    [Networked] private int net_CurrentNewsIndex { get; set; }

    private LTDescr currentNewsTween;
    private bool isMessageCooldown = false;


    /// <summary>
    /// When game starts (from lobbymanager) we don't keep the lobby, we destroy it
    /// and the masterclient spawns the gamemanager, that handles spawning of all players
    /// GameManager spawns players by THEIR runner, providing them input/state authority
    /// 
    /// Lobby handles lobby stuff
    /// Game handles game stuff
    /// 
    /// Make sure that masterclient/authority over the managers (lobby and game) is handled well if the master leaves.
    /// 
    /// Linear architecture, no complexities
    /// </summary>
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

        // game scene is 1 (at least the first level)
        _gameScene = SceneRef.FromIndex(1);
    }

    public override void Spawned()
    {
        isSpawned = true;
        Runner.AddCallbacks(this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        isSpawned = false;
    }

    public void ShowLobbyUI()
    {
        AudioManager.Instance.SetCutoffFrequency(10000, 7000);
        UILobby.Instance.ConnectToLobby();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_StartGame()
    {
        if (HasStateAuthority)
        {
            if (FusionLauncher.Instance.GetNetworkRunner().IsRunning)
            {
                FusionLauncher.Instance.GetNetworkRunner().LoadScene(_gameScene, LoadSceneMode.Single);
            }
        }
    }

    public void SetPlayerReadyState(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            RPC_RequestToggleReadyState(player);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestToggleReadyState(PlayerRef player)
    {
        bool newReadyState;

        if (net_ReadyStates.ContainsKey(player))
        {
            bool currentReadyState = net_ReadyStates[player];
            newReadyState = !currentReadyState;
            net_ReadyStates.Set(player, newReadyState);
        }
        else
        {
            newReadyState = true;
            net_ReadyStates.Add(player, newReadyState);
        }

        RPC_SyncReadyState(player, newReadyState);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SyncReadyState(PlayerRef player, bool isReady)
    {
        if (net_ReadyStates.ContainsKey(player))
        {
            net_ReadyStates.Set(player, isReady);
        }
        else
        {
            net_ReadyStates.Add(player, isReady);
        }

        UpdatePlayerList();
    }

    public bool IsPlayerReady(PlayerRef player)
    {
        return net_ReadyStates.ContainsKey(player) && net_ReadyStates[player];
    }

    public void ShowMessageAbovePlayer(string message)
    {
        if (isMessageCooldown)
        {
            return;
        }

        RPC_TalkShit(Runner.LocalPlayer, message);
    }

    public void ShowEmoteAbovePlayer(string emote)
    {
        if (isMessageCooldown)
        {
            return;
        }

        RPC_SendEmote(Runner.LocalPlayer, emote);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_SendEmote(PlayerRef player, string emote)
    {
        isMessageCooldown = true;

        switch (emote)
        {
            case "<sprite name=\"ManiaMoji_4\">":
                PlayCustomAnimation(player, emote);
                break;
            default:
                PlayDefaultEmoteAnimation(player, emote);
                break;
        }
    }

    private void PlayDefaultEmoteAnimation(PlayerRef player, string emote)
    {
        var runner = FusionLauncher.Instance.GetNetworkRunner();
        int playerIndex = FindPlayerPosition(player);

        if (playerIndex >= 0 && playerIndex < net_PlayerPositions.Length)
        {
            Transform position = UILobby.Instance.PlayerPositionsParent.GetChild(playerIndex);
            var emoteText = position.Find("messageTxt").GetComponent<TextMeshProUGUI>();

            if (emoteText != null)
            {
                LeanTween.cancel(emoteText.gameObject);
                emoteText.gameObject.SetActive(false);

                Vector3 originalPosition = emoteText.transform.localPosition;
                Vector3 belowOriginalPosition = originalPosition + new Vector3(0, -100f, 0);

                emoteText.fontSize = 63;
                emoteText.transform.localPosition = belowOriginalPosition;
                emoteText.transform.localScale = Vector3.zero;

                emoteText.text = emote;
                emoteText.gameObject.SetActive(true);

                LeanTween.moveLocalY(emoteText.gameObject, originalPosition.y, 0.5f).setEase(LeanTweenType.easeInOutBack);
                LeanTween.scale(emoteText.gameObject, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBounce).setOnComplete(() =>
                {
                    LeanTween.scale(emoteText.gameObject, Vector3.one * 1.2f, 0.3f).setEase(LeanTweenType.easeInOutSine).setLoopPingPong(4).setOnComplete(() =>
                    {
                        LeanTween.moveLocalY(emoteText.gameObject, belowOriginalPosition.y, 0.5f).setEase(LeanTweenType.easeInOutBack);
                        LeanTween.scale(emoteText.gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInOutBack).setOnComplete(() =>
                        {
                            emoteText.gameObject.SetActive(false);
                            emoteText.transform.localPosition = originalPosition;
                            isMessageCooldown = false;
                        });
                    });
                });
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_TalkShit(PlayerRef player, string message)
    {
        isMessageCooldown = true;

        var runner = FusionLauncher.Instance.GetNetworkRunner();
        int playerIndex = FindPlayerPosition(player);

        if (playerIndex >= 0 && playerIndex < net_PlayerPositions.Length)
        {
            Transform position = UILobby.Instance.PlayerPositionsParent.GetChild(playerIndex);
            var messageText = position.Find("messageTxt").GetComponent<TextMeshProUGUI>();

            if (messageText != null)
            {
                var typewriter = messageText.GetComponent<Typewriter>();
                var textEffects = messageText.GetComponent<TextEffects>();
                LeanTween.cancel(messageText.gameObject);
                messageText.gameObject.SetActive(false);

                // Store original pos
                Vector3 originalPosition = messageText.transform.localPosition;
                Vector3 belowOriginalPosition = originalPosition + new Vector3(0, -100f, 0);

                messageText.fontSize = 28;

                messageText.gameObject.SetActive(true);
                messageText.text = "";

                typewriter.Animate(message);

                textEffects.SetOnTypingEnd(() =>
                {
                    LeanTween.scale(messageText.gameObject, Vector3.one * 1.1f, 0.3f)
                             .setEase(LeanTweenType.easeOutBack)
                             .setOnComplete(() =>
                             {
                                 LeanTween.scale(messageText.gameObject, Vector3.one, 0.2f)
                                          .setEase(LeanTweenType.easeInOutSine);
                             });
                });

                // Delay before removing msg
                LeanTween.value(messageText.gameObject, 0, 1, LobbyChatManager.MESSAGE_DISPLAY_TIME).setOnComplete(() =>
                {
                    // Animate the message popping down smoothly
                    //LeanTween.moveLocalY(messageText.gameObject, belowOriginalPosition.y, 0.5f).setEase(LeanTweenType.easeInCirc);
                    LeanTween.value(messageText.fontSize, 0, 0.3f).setEase(LeanTweenType.easeInOutSine).setOnUpdate((float value) => messageText.fontSize = value).setDelay(0.3f).setOnComplete(() =>
                    {
                        messageText.gameObject.SetActive(false);
                        messageText.text = "";
                        messageText.transform.localPosition = originalPosition;
                        messageText.transform.localScale = Vector3.one;
                        isMessageCooldown = false;
                    });
                });
            }
        }
    }

    public void StartManiaNews()
    {
        if (HasStateAuthority && !net_NewsStarted)
        {
            net_NewsStarted = true;
            net_CurrentNewsIndex = 0;
            RPC_RequestUpdateNews(net_CurrentNewsIndex);
        }
    }

    public void ScheduleNextNewsPage()
    {
        int newsCount = LobbyManiaNews.Instance.GetNewsCount();
        int nextIndex = LobbyManiaNews.Instance._randomOrder ? Random.Range(0, newsCount) : (net_CurrentNewsIndex + 1) % newsCount;
        net_CurrentNewsIndex = nextIndex;
        RPC_RequestUpdateNews(nextIndex);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestUpdateNews(int index)
    {
        if (net_NewsStarted)
        {
            RPC_UpdateManiaNews(index);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateManiaNews(int index)
    {
        LobbyManiaNews.Instance.ShowNews(index);

        if (currentNewsTween != null)
        {
            LeanTween.cancel(currentNewsTween.id);
        }

        currentNewsTween = LeanTween.delayedCall(LobbyManiaNews.Instance._displayDuration, ScheduleNextNewsPage);
    }

    private void PlayCustomAnimation(PlayerRef player, string emote)
    {
        var runner = FusionLauncher.Instance.GetNetworkRunner();
        int playerIndex = FindPlayerPosition(player);

        if (playerIndex >= 0 && playerIndex < net_PlayerPositions.Length)
        {
            Transform position = UILobby.Instance.PlayerPositionsParent.GetChild(playerIndex);
            var emoteText = position.Find("messageTxt").GetComponent<TextMeshProUGUI>();

            if (emoteText != null)
            {
                // Cancel any ongoing animations
                LeanTween.cancel(emoteText.gameObject);
                emoteText.gameObject.SetActive(false);

                // Store the original position
                Vector3 originalPosition = emoteText.transform.localPosition;
                Vector3 belowOriginalPosition = originalPosition + new Vector3(0, -100f, 0);

                // Set the font size for the emoji text
                emoteText.fontSize = 55;

                // Set initial position and scale
                emoteText.transform.localPosition = belowOriginalPosition;
                emoteText.transform.localScale = Vector3.zero;

                // Set the emote text and activate the game object
                emoteText.text = emote;
                emoteText.gameObject.SetActive(true);

                // Custom animation for "ManiaMoji_43"
                LeanTween.moveLocalY(emoteText.gameObject, originalPosition.y, 0.5f).setEase(LeanTweenType.easeOutElastic);
                LeanTween.scale(emoteText.gameObject, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutElastic).setOnComplete(() =>
                {
                    // Custom wiggle and ping-pong scale animation
                    LeanTween.rotateZ(emoteText.gameObject, 15f, 0.1f).setEase(LeanTweenType.easeInOutSine).setLoopPingPong(20).setOnComplete(() =>
                    {
                        // Move to center, scale up, and wiggle
                        Vector3 centerPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);
                        emoteText.transform.SetParent(null); // Detach from the parent to move to screen center
                        LeanTween.move(emoteText.gameObject, centerPosition, 2f).setEase(LeanTweenType.easeInOutExpo);
                        LeanTween.scale(emoteText.gameObject, Vector3.one * 2f, 2f).setEase(LeanTweenType.easeInOutExpo).setOnComplete(() =>
                        {
                            LeanTween.rotateZ(emoteText.gameObject, 10f, 0.05f).setEase(LeanTweenType.easeInOutSine).setLoopPingPong(10);
                            LeanTween.moveLocalY(emoteText.gameObject, Screen.height, 2f).setEase(LeanTweenType.easeInOutExpo).setOnComplete(() =>
                            {
                                emoteText.gameObject.SetActive(false);
                                //emoteText.transform.SetParent(PlayerPositions[playerIndex]);
                                emoteText.transform.localPosition = originalPosition;
                                emoteText.fontSize = 55; // Reset font size
                                isMessageCooldown = false;
                            });
                        });
                    });
                });
            }
        }
    }

    private void InitiateMasterClientTransfer()
    {
        if (net_PlayerList.Count > 0)
        {
            PlayerRef newMasterClient = net_PlayerList[0];
            RPC_NotifyNewMasterClient(newMasterClient);
        }
        else
        {
            Debug.Log("No players left to transfer authority to.");
            return;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotifyNewMasterClient(PlayerRef newMasterClient)
    {
        NetworkObject lobbyManager = FindObjectOfType<LobbyManager>().GetComponent<NetworkObject>();

        if (Runner.LocalPlayer == newMasterClient)
        {
            lobbyManager.RequestStateAuthority();
            Debug.Log($"Requested state authority for new master client: {newMasterClient}");
        }
    }

    private void UpdatePlayerList()
    {
        foreach (var player in net_PlayerPositionsMap)
        {
            int positionIndex = player.Value;
            Transform positionTransform = UILobby.Instance.PlayerPositionsParent.GetChild(positionIndex);
            UpdatePlayerUI(positionTransform, player.Key);
        }
    }

    private void PrepareForMenu(GameObject playerObject)
    {
        /*var spriteRenderer = playerObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sortingLayerName = "UI";
                    spriteRenderer.sortingOrder = 10;
                    //playerObject.AddComponent<NetworkTransform>();
                    Debug.Log($"Sort order: {spriteRenderer.sortingOrder}");
        }*/

        playerObject.transform.position += new Vector3(0, 0, 3);

        NetworkRigidbody2D networkRb = playerObject.GetComponent<NetworkRigidbody2D>();
        if (networkRb != null)
        {
            //Destroy(networkRb);
            //Destroy(playerObject.GetComponent<NetworkRigidbody2D>());
            //networkRb.enabled = false;
            // Disabling this prevents setting position
        }

/*        PlayerController playerController = playerObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }*/

        Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }

/*        RunnerSimulatePhysics2D runnerSimulate2D = playerObject.GetComponent<RunnerSimulatePhysics2D>();
        if (runnerSimulate2D != null)
        {
            Destroy(playerObject.GetComponent<RunnerSimulatePhysics2D>());
        }*/
    }

    private void UpdateUI(int positionIndex, PlayerRef player)
    {
        var playerInfo = net_PlayerPositions.Get(positionIndex);
        Transform position = UILobby.Instance.PlayerPositionsParent.GetChild(positionIndex);

        UpdatePlayerUI(position, player);
    }

    private void ClearUI(int positionIndex)
    {
        Transform position = UILobby.Instance.PlayerPositionsParent.GetChild(positionIndex);

        var nameText = position.Find("nameText").GetComponent<TextMeshProUGUI>();
        var statusText = position.Find("statusText").GetComponent<TextMeshProUGUI>();
        var messageText = position.Find("messageTxt").GetComponent<TextMeshProUGUI>();

        nameText.text = "";
        statusText.text = "";
        messageText.gameObject.SetActive(false);
    }

    private void UpdatePlayerUI(Transform position, PlayerRef player)
    {
        var nameText = position.Find("nameText").GetComponent<TextMeshProUGUI>();
        var statusText = position.Find("statusText").GetComponent<TextMeshProUGUI>();
        var messageText = position.Find("messageTxt").GetComponent<TextMeshProUGUI>();

        nameText.color = new Color(1f, 1f, 1f);
        bool isReady = net_ReadyStates.ContainsKey(player) && net_ReadyStates[player];
        nameText.text = $"Player {player.PlayerId}";
        statusText.text = isReady ? "Ready" : "Not Ready";
        messageText.gameObject.SetActive(false);
    }

    private int GetNextAvailablePosition()
    {
        for (int i = 0; i < net_PlayerPositions.Length; i++)
        {
            if (IsDefault(net_PlayerPositions.Get(i)))
            {
                return i;
            }
        }
        return -1; // Lobby is full
    }

    private bool IsDefault(PlayerInfo playerInfo)
    {
        return playerInfo.net_Player == default;
    }

    private int FindPlayerPosition(PlayerRef player)
    {
        for (int i = 0; i < net_PlayerPositions.Length; i++)
        {
            if (net_PlayerPositions.Get(i).net_Player == player)
            {
                return i;
            }
        }
        return -1;
    }


    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (true)
        {
            int positionIndex = GetNextAvailablePosition();
            if (positionIndex != -1)
            {
                net_PlayerPositions.Set(positionIndex, new PlayerInfo
                {
                    net_Player = player,
                    net_PlayerName = "Player " + player.PlayerId,
                    net_PlayerState = "Ready",
                    net_PlayerMessage = "Welcome"
                });

                NetworkObject playerObject;
                playerObject = FusionLauncher.Instance.GetNetworkRunner().Spawn(net_PlayerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                playerObject.GetComponent<NetworkRigidbody2D>().RBPosition += new Vector3(0, 0, 3);
                PrepareForMenu(playerObject.gameObject);
                FusionLauncher.Instance.GetNetworkRunner().SetPlayerObject(player, playerObject);

                UpdateUI(positionIndex, player);
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        bool wasMasterClient = player.IsMasterClient || runner.IsSharedModeMasterClient && runner.LocalPlayer == player;

        if (HasStateAuthority || runner.IsSharedModeMasterClient) // State authority or masterclient?
        {
            int positionIndex = FindPlayerPosition(player);
            if (positionIndex != -1)
            {
                net_PlayerPositions.Set(positionIndex, default);

                ClearUI(positionIndex);
            }
        }
        UpdatePlayerList();

        var playerObject = runner.GetPlayerObject(player);
        if (playerObject != null)
        {
            runner.Despawn(playerObject);
            Destroy(playerObject.gameObject);
        }

        if (wasMasterClient)
        {
            InitiateMasterClientTransfer();
        }
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("[Lobby] Lobby scene loaded.");
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {

    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {

    }
}
