﻿using UnityEngine;
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

public class LobbyManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static LobbyManager Instance;

    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private GameObject cameraPrefab;

    private SceneRef gameScene;
    public bool isSpawned = false;

    // Lobby stuff
    [NonSerialized] public List<Transform> playerPositions = new List<Transform>();
    private Transform playerPositionsParent;
    private Coroutine currentNewsCoroutine = null;

    // Properties in sync with all clients
    // superadmin is the solution to shared mode's authority rules
    // Since there's no server 'brain', and all players have authority over what they spawn (on runner) we need a ruler on system operations
    // and systems like LobbyManager
    // So:
    // 1. First player to join a 'session' defined by string, is the superadmin.
    // 2. Before player 1 intends to leave (only with other players in), the authority is automatically transfered to the first in the list.
    // 3. Lobby continues living.
    // 4. 

    //[Networked] private PlayerRef Superadmin { get; set; } // State Authority, The Big BOSS
    [Networked] private NetworkDictionary<PlayerRef, NetworkBool> NetworkedReadyStates { get; }
    [Networked, Capacity(16)] private NetworkLinkedList<PlayerRef> NetworkedPlayerList { get; }
    [Networked] private NetworkBool newsStarted { get; set; }
    [Networked] private int currentNewsIndex { get; set; }


    private LTDescr currentNewsTween;
    private bool isMessageCooldown = false;

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

        ServiceLocator.RegisterLobbyManager(this);
        // game scene is 1 (at least the first level)
        gameScene = SceneRef.FromIndex(1);

        // Initialize player positions
        foreach (Transform pos in UILobby.Instance.playerPositionsParent)
        {
            playerPositions.Add(pos);
        }

        LobbyManiaNews.Instance.InitializeNews();
    }

    private void Start()
    {
        StartCoroutine(WaitForRunners());
    }

    private IEnumerator WaitForRunners()
    {
        while (FusionLauncher.Instance == null || FusionLauncher.Instance.GetNetworkRunner() == null)
        {
            yield return null;
        }
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
        ServiceLocator.GetAudioManager().SetCutoffFrequency(10000, 7000);
        UILobby.Instance.ConnectToLobby();
    }

    public void StartGame()
    {
        if (FusionLauncher.Instance.GetNetworkRunner().IsRunning)
        {
            FusionLauncher.Instance.GetNetworkRunner().LoadScene(gameScene, LoadSceneMode.Single);
        }
    }

    public void UpdatePlayerList()
    {
        var runner = FusionLauncher.Instance.GetNetworkRunner();
        var activePlayers = runner.ActivePlayers.ToList(); // Convert to List to use indexing

        for (int i = 0; i < playerPositions.Count; i++)
        {
            var position = playerPositions[i];
            var nameText = position.Find("nameText").GetComponent<TextMeshProUGUI>();
            var statusText = position.Find("statusText").GetComponent<TextMeshProUGUI>();
            var messageText = position.Find("messageTxt").GetComponent<TextMeshProUGUI>();


            if (i < activePlayers.Count)
            {
                nameText.color = new Color(1f, 1f, 1f);
                var player = activePlayers[i];
                bool isReady = IsPlayerReady(player);
                nameText.text = $"Player {player.PlayerId}";
                statusText.text = isReady ? "Ready" : "Not Ready";
                messageText.gameObject.SetActive(false);
            }
            else
            {
                nameText.color = ColorUtility.TryParseHtmlString("#4480C3", out Color waitingColor) ? waitingColor : Color.white;
                nameText.text = "Waiting...";
                statusText.text = "";
                messageText.gameObject.SetActive(false);
            }
        }
    }

    private void DisableMenuComponents(NetworkObject playerObject)
    {
        // Disable the PlayerController component if the player is in the menu
        PlayerController playerController = playerObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Set Rigidbody2D to static if the player is in the menu
        Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }

        NetworkRigidbody2D networkRb = playerObject.GetComponent<NetworkRigidbody2D>();
        if (networkRb != null)
        {
            networkRb.enabled = false;
        }

        RunnerSimulatePhysics2D runnerSimulate2D = playerObject.GetComponent<RunnerSimulatePhysics2D>();
        if (runnerSimulate2D != null)
        {
            runnerSimulate2D.enabled = false;
        }

        // Ensure the player is rendered in front of the UI canvas
        var spriteRenderer = playerObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = "UI"; // Ensure you have a sorting layer named "UI"
            spriteRenderer.sortingOrder = 10; // Ensure this is high enough to be on top
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

        if (NetworkedReadyStates.ContainsKey(player))
        {
            bool currentReadyState = NetworkedReadyStates[player];
            newReadyState = !currentReadyState;
            NetworkedReadyStates.Set(player, newReadyState);
        }
        else
        {
            newReadyState = true;
            NetworkedReadyStates.Add(player, newReadyState);
        }

        RPC_SyncReadyState(player, newReadyState);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SyncReadyState(PlayerRef player, bool isReady)
    {
        if (NetworkedReadyStates.ContainsKey(player))
        {
            NetworkedReadyStates.Set(player, isReady);
        }
        else
        {
            NetworkedReadyStates.Add(player, isReady);
        }

        UpdatePlayerList();
    }

    public bool IsPlayerReady(PlayerRef player)
    {
        return NetworkedReadyStates.ContainsKey(player) && NetworkedReadyStates[player];
    }

    public void ShowMessageAbovePlayer(string message)
    {
        if (isMessageCooldown)
        {
            return;
        }

        RPC_ShowMessageAbovePlayer(Runner.LocalPlayer, message);
    }

    public void ShowEmoteAbovePlayer(string emote)
    {
        if (isMessageCooldown)
        {
            return;
        }

        RpcShowEmoteAbovePlayer(Runner.LocalPlayer, emote);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RpcShowEmoteAbovePlayer(PlayerRef player, string emote)
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
        int playerIndex = runner.ActivePlayers.ToList().IndexOf(player);

        if (playerIndex >= 0 && playerIndex < playerPositions.Count)
        {
            var emoteText = playerPositions[playerIndex].Find("messageTxt").GetComponent<TextMeshProUGUI>();
            if (emoteText != null)
            {
                // Cancel any ongoing animations
                LeanTween.cancel(emoteText.gameObject);
                emoteText.gameObject.SetActive(false);

                // Store the original position
                Vector3 originalPosition = emoteText.transform.localPosition;
                Vector3 belowOriginalPosition = originalPosition + new Vector3(0, -100f, 0);

                // Set the font size for the emoji text
                emoteText.fontSize = 63;

                // Set initial position and scale
                emoteText.transform.localPosition = belowOriginalPosition;
                emoteText.transform.localScale = Vector3.zero;

                // Set the emote text and activate the game object
                emoteText.text = emote;
                emoteText.gameObject.SetActive(true);

                // Animate the emote popping out with a bounce
                LeanTween.moveLocalY(emoteText.gameObject, originalPosition.y, 0.5f).setEase(LeanTweenType.easeInOutBack);
                LeanTween.scale(emoteText.gameObject, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBounce).setOnComplete(() =>
                {
                    // Wiggle and ping-pong scale animation
                    LeanTween.scale(emoteText.gameObject, Vector3.one * 1.2f, 0.3f).setEase(LeanTweenType.easeInOutSine).setLoopPingPong(4).setOnComplete(() =>
                    {
                        // Animate the emote popping down smoothly
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
    private void RPC_ShowMessageAbovePlayer(PlayerRef player, string message)
    {
        isMessageCooldown = true;

        var runner = FusionLauncher.Instance.GetNetworkRunner();
        int playerIndex = runner.ActivePlayers.ToList().IndexOf(player);

        if (playerIndex >= 0 && playerIndex < playerPositions.Count)
        {
            var messageText = playerPositions[playerIndex].Find("messageTxt").GetComponent<TextMeshProUGUI>();
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

                //messageText.transform.localPosition = belowOriginalPosition;
                //messageText.transform.localScale = Vector3.zero;

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


                //LeanTween.moveLocalY(messageText.gameObject, originalPosition.y, 0.55f).setEase(LeanTweenType.easeOutBack);
                //LeanTween.scale(messageText.gameObject, Vector3.one, 0.75f).setEase(LeanTweenType.easeOutBack);


                // Delay before removing msg
                LeanTween.value(messageText.gameObject, 0, 1, ChatboxManager.MESSAGE_DISPLAY_TIME).setOnComplete(() =>
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
        if (HasStateAuthority && !newsStarted)
        {
            newsStarted = true;
            currentNewsIndex = 0;
            RPC_RequestUpdateNews(currentNewsIndex);
        }
    }

    public void ScheduleNextNewsPage()
    {
        int newsCount = LobbyManiaNews.Instance.GetNewsCount();
        int nextIndex = LobbyManiaNews.Instance.randomOrder ? Random.Range(0, newsCount) : (currentNewsIndex + 1) % newsCount;
        currentNewsIndex = nextIndex;
        RPC_RequestUpdateNews(nextIndex);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestUpdateNews(int index)
    {
        if (newsStarted)
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

        currentNewsTween = LeanTween.delayedCall(LobbyManiaNews.Instance.displayDuration, ScheduleNextNewsPage);
    }

    private void PlayCustomAnimation(PlayerRef player, string emote)
    {
        var runner = FusionLauncher.Instance.GetNetworkRunner();
        int playerIndex = runner.ActivePlayers.ToList().IndexOf(player);

        if (playerIndex >= 0 && playerIndex < playerPositions.Count)
        {
            var emoteText = playerPositions[playerIndex].Find("messageTxt").GetComponent<TextMeshProUGUI>();
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
                                emoteText.transform.SetParent(playerPositions[playerIndex]);
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
        if (NetworkedPlayerList.Count > 0)
        {
            PlayerRef newMasterClient = NetworkedPlayerList[0]; // Assuming the first in the list
            RpcNotifyNewMasterClient(newMasterClient);
        }
        else
        {
            Debug.Log("No players left to transfer authority to.");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcNotifyNewMasterClient(PlayerRef newMasterClient)
    {
        NetworkObject lobbyManager = FindObjectOfType<LobbyManager>().GetComponent<NetworkObject>();

        // Check if the current player is the new master client
        if (Runner.LocalPlayer == newMasterClient)
        {
            lobbyManager.RequestStateAuthority();
            Debug.Log($"Requested state authority for new master client: {newMasterClient}");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcSpawnPlayerForAll(PlayerRef player)
    {
        Debug.Log("STate authority calls to spawn player for all");
        SpawnPlayer(player, true);
    }

    public void SpawnPlayer(PlayerRef player, bool isMenu = false)
    {
        var runner = FusionLauncher.Instance.GetNetworkRunner();
        NetworkObject playerObject = runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
        Debug.Log($"{player} spawned. NetworkObject ID: {playerObject?.NetworkTypeId}");

        if (isMenu)
        {
            int playerIndex = runner.ActivePlayers.ToList().IndexOf(player);
            if (playerIndex >= 0 && playerIndex < playerPositions.Count)
            {
                var spawnPosition = playerPositions[playerIndex].position;
                playerObject.transform.position = spawnPosition;
                playerObject.transform.localScale = new Vector3(.5f, .5f, .5f);
                DisableMenuComponents(playerObject);
            }
        }
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!NetworkedPlayerList.Contains(player))
        {
            NetworkedPlayerList.Add(player);
            if (HasStateAuthority)
            {
                RpcSpawnPlayerForAll(player);
            }
        }
        Debug.Log("OnPlayerJoined called");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} left. LocalPlayer: {runner.LocalPlayer}, IsLocalPlayerValid: {!runner.LocalPlayer.IsNone}");

        bool wasMasterClient = runner.IsSharedModeMasterClient && runner.LocalPlayer == player;

        // If the player who left was in the player list, remove them
        if (NetworkedPlayerList.Contains(player))
        {
            NetworkedPlayerList.Remove(player);
        }

        var playerObject = runner.GetPlayerObject(player);
        if (playerObject != null)
        {
            runner.Despawn(playerObject);
            Destroy(playerObject.gameObject);
        }

        UpdatePlayerList();

        if (wasMasterClient)
        {
            InitiateMasterClientTransfer();
        }
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("Scene load done!");
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