using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Rendering;
using Fusion.Addons.Physics;
using System.Collections;

public class PublicLobbyManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static PublicLobbyManager Instance;
    private static LobbyHubManager _hubManager;

    private const int MAX_PLAYERS = 6;
    private const int LOBBY_TIMER_START = 10;
    [Networked] public bool net_IsSpawned { get; set; }

    private SceneRef _gameScene;

    [Networked] private bool net_isTimerRunning { get; set; }
    [Networked] private float net_remainingTime { get; set; }

    public PublicLobbyPosition[] lobbyPositionMarkers;
    private Dictionary<PlayerRef, NetworkObject> playerObjects = new Dictionary<PlayerRef, NetworkObject>();

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

        InitializeLobbyPositions();
        _gameScene = SceneRef.FromIndex(1);
        _hubManager = new LobbyHubManager();
    }


    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            Runner.AddCallbacks(this);
            RpcSetIsSpawned();

            // handle local player spawning
            if (Runner.IsSharedModeMasterClient)
            {
                OnPlayerJoined(Runner, Runner.LocalPlayer);
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcSetIsSpawned()
    {
        net_IsSpawned = true;
    }

    private void InitializeLobbyPositions()
    {
        if (LobbyUI.Instance != null)
        {
            lobbyPositionMarkers = new PublicLobbyPosition[MAX_PLAYERS];
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                Transform slotTransform = LobbyUI.Instance.PlayerSlotsParent.GetChild(i);
                lobbyPositionMarkers[i] = new PublicLobbyPosition
                {
                    Position = slotTransform,
                    IsOccupied = false
                };
            }
        }
    }

    private void Update()
    {
        if (net_IsSpawned && HasStateAuthority)
        {
            UpdateLobbyTimer();
        }
    }

    public void StartLobbyTimer()
    {
        if (!net_isTimerRunning)
        {
            net_remainingTime = LOBBY_TIMER_START;
            net_isTimerRunning = true;
        }
    }

    public void StopLobbyTimer()
    {
        net_isTimerRunning = false;
        net_remainingTime = LOBBY_TIMER_START;
        UpdateTimerUI();
    }

    public void AddTimeToLobbyTimer(int seconds)
    {
        if (net_isTimerRunning)
        {
            net_remainingTime += seconds;
            UpdateTimerUI();
        }
    }

    private void UpdateLobbyTimer()
    {
        if (net_isTimerRunning)
        {
            net_remainingTime -= Time.deltaTime;
            if (net_remainingTime <= 0)
            {
                net_remainingTime = 0;
                StopLobbyTimer();
                ShowConnectingOverlay();
                StartGame();
            }
            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        int remainingSeconds = Mathf.CeilToInt(net_remainingTime);
        LobbyUI.Instance.gameStartLobbyTimer.text = remainingSeconds.ToString() + "s";

        // Broadcast the updated time to all clients
        RpcUpdateTimer(net_remainingTime);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcUpdateTimer(float newTime)
    {
        if (!HasStateAuthority)
        {
            net_remainingTime = newTime;
            int remainingSeconds = Mathf.CeilToInt(net_remainingTime);
            LobbyUI.Instance.gameStartLobbyTimer.text = remainingSeconds.ToString() + "s";
        }
    }

    private void ShowConnectingOverlay()
    {
        LobbyUI.Instance.ConnectingOverlay.SetActive(true);
        LobbyUI.Instance.ConnectingText.text = "Joining Game";
    }

    private void StartGame()
    {
        if (HasStateAuthority)
        {
            RpcStartGame();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcStartGame()
    {
        if (HasStateAuthority)
        {
            if (FusionLauncher.Instance.GetNetworkRunner().IsRunning)
            {
                FusionLauncher.Instance.GetNetworkRunner().LoadScene(_gameScene, LoadSceneMode.Single);
                FusionLauncher.Instance.GetNetworkRunner().Spawn(FusionLauncher.Instance.GetGameManagerNetPrefab());
                Destroy(gameObject);
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcToggleReadyState(PlayerRef player)
    {
        var playerObject = Runner.GetPlayerObject(player);
        if (playerObject != null)
        {
            var playerManager = playerObject.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                bool newReadyState = !playerManager.net_IsReady;
                playerManager.SetPlayerReady(newReadyState);

                // Update the UI for all clients
                UpdateReadyStateUI(player, newReadyState);
            }
        }
    }

    private void UpdateReadyStateUI(PlayerRef player, bool isReady)
    {
        int playerIndex = FindPlayerPosition(player);
        if (playerIndex >= 0)
        {
            Transform slotTransform = lobbyPositionMarkers[playerIndex].Position;
            var statusText = slotTransform.Find("statusText").GetComponent<TextMeshProUGUI>();
            if (statusText != null)
            {
                statusText.text = isReady ? "Ready" : "Not Ready";
            }
        }
    }

    public int FindPlayerPosition(PlayerRef player)
    {
        for (int i = 0; i < lobbyPositionMarkers.Length; i++)
        {
            if (playerObjects.ContainsKey(player) && playerObjects[player].transform.parent == lobbyPositionMarkers[i].Position)
            {
                return i;
            }
        }
        return -1;
    }

    public void SetPlayerReadyState(PlayerRef player)
    {
        if (HasStateAuthority)
        {
            RpcToggleReadyState(player);
        }
    }

    public bool IsPlayerReady(PlayerRef player)
    {
        var playerObject = Runner.GetPlayerObject(player);
        if (playerObject != null)
        {
            var playerManager = playerObject.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                return playerManager.net_IsReady;
            }
        }
        return false;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcShowEmoteAbovePlayer(PlayerRef player, string emote)
    {
        _hubManager.ShowEmote(player, emote);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcShowMessageAbovePlayer(PlayerRef player, string message)
    {
        _hubManager.ShowMessage(player, message);
    }

    private void UpdatePlayerUI(Transform slotTransform, PlayerRef player)
    {
        if (playerObjects.TryGetValue(player, out var playerObject))
        {
            var playerManager = playerObject.GetComponent<PlayerManager>();
            var nameText = slotTransform.Find("nameText").GetComponent<TextMeshProUGUI>();
            var statusText = slotTransform.Find("statusText").GetComponent<TextMeshProUGUI>();
            var messageText = slotTransform.Find("messageTxt").GetComponent<TextMeshProUGUI>();

            if (playerManager != null)
            {
                nameText.text = $"{playerManager.net_PlayerName} ({player.PlayerId})";
                statusText.text = playerManager.net_IsReady ? "Ready" : "Not Ready";
                messageText.text = ""; // Initially clear
            }
            else
            {
                Debug.LogError("PlayerManager component missing on the player object.");
            }
        }
        else
        {
            Debug.LogError($"Player object not found for player: {player.PlayerId}");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcSetPlayerPosition(PlayerRef player, int positionIndex)
    {
        if (lobbyPositionMarkers != null && positionIndex < lobbyPositionMarkers.Length)
        {
            Vector3 fixedYPos;
            Vector3 scale;

            // Organize positioning logic using a switch statement
            switch (positionIndex)
            {
                case 0:
                    fixedYPos = lobbyPositionMarkers[positionIndex].Position.position - new Vector3(0, 50f, 0);
                    scale = new Vector3(70, 70, 70);
                    break;
                case 1:
                case 2:
                    fixedYPos = lobbyPositionMarkers[positionIndex].Position.position - new Vector3(0, 50f, 0);
                    scale = new Vector3(70, 70, 70);
                    break;
                case 3:
                    fixedYPos = lobbyPositionMarkers[positionIndex].Position.position - new Vector3(0, 50f, 0);
                    scale = new Vector3(70, 70, 70);
                    break;
                case 4:
                    fixedYPos = lobbyPositionMarkers[positionIndex].Position.position - new Vector3(0, 50f, 0);
                    scale = new Vector3(70, 70, 70);
                    break;
                case 5:
                    fixedYPos = lobbyPositionMarkers[positionIndex].Position.position - new Vector3(0, 50f, 0);
                    scale = new Vector3(70, 70, 70);
                    break;
                default:
                    fixedYPos = lobbyPositionMarkers[positionIndex].Position.position;
                    scale = new Vector3(70, 70, 70);
                    break;
            }

            if (!playerObjects.ContainsKey(player))
            {
                NetworkPrefabRef playerPrefab = FusionLauncher.Instance.GetPlayerNetPrefab();
                Debug.Log($"Spawning player object for player: {player.PlayerId} at position index: {positionIndex}");

                var playerObject = Runner.Spawn(playerPrefab, fixedYPos, Quaternion.identity, player);

                if (playerObject != null)
                {
                    Runner.SetPlayerObject(player, playerObject);
                    playerObjects[player] = playerObject;

                    // Parent and position the player object on all clients
                    playerObject.transform.SetParent(lobbyPositionMarkers[positionIndex].Position);
                    playerObject.transform.localScale = scale;
                    playerObject.transform.localPosition = Vector3.zero; // Ensure it's correctly positioned relative to its parent

                    // Further adjustments
                    playerObject.transform.localPosition += new Vector3(0, -50f, 0);

                    var sortingGroup = playerObject.GetComponent<SortingGroup>();
                    if (sortingGroup != null)
                    {
                        sortingGroup.sortingOrder = 4;
                    }

                    var rb = playerObject.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.bodyType = RigidbodyType2D.Static;
                    }

                    // Mark the slot as occupied
                    lobbyPositionMarkers[positionIndex].IsOccupied = true;

                    // Update UI elements for the player
                    UpdatePlayerUI(lobbyPositionMarkers[positionIndex].Position, player);
                    Debug.Log($"Player object set successfully for player: {player.PlayerId}");
                }
                else
                {
                    Debug.LogError($"Failed to spawn player object for player: {player.PlayerId}");
                }
            }
            else
            {
                // Update the position of the player object if it already exists
                if (playerObjects.TryGetValue(player, out var existingPlayerObject))
                {
                    existingPlayerObject.transform.SetParent(lobbyPositionMarkers[positionIndex].Position);
                    existingPlayerObject.transform.localPosition = Vector3.zero;
                    existingPlayerObject.transform.localScale = scale;
                }
            }
        }
        else
        {
            Debug.LogError("Lobby position markers are null or index is out of range.");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (Runner.IsSharedModeMasterClient && HasStateAuthority)
        {
            int positionIndex = GetNextAvailablePosition(player);
            if (positionIndex != -1 && !playerObjects.ContainsKey(player))
            {
                RpcSetPlayerPosition(player, positionIndex);
            }

            StartLobbyTimer();
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (playerObjects.ContainsKey(player))
        {
            var playerObject = playerObjects[player];
            if (playerObject != null)
            {
                runner.Despawn(playerObject);
            }

            for (int i = 0; i < lobbyPositionMarkers.Length; i++)
            {
                if (playerObject.transform.position == lobbyPositionMarkers[i].Position.position)
                {
                    lobbyPositionMarkers[i].IsOccupied = false;
                    ClearUI(i);
                    break;
                }
            }

            playerObjects.Remove(player);
        }
    }

    private int GetNextAvailablePosition(PlayerRef player)
    {
        if (player == Runner.LocalPlayer && Runner.IsSharedModeMasterClient)
        {
            return 0;
        }

        for (int i = 1; i < lobbyPositionMarkers.Length; i++)
        {
            if (!lobbyPositionMarkers[i].IsOccupied)
            {
                return i;
            }
        }
        return -1;
    }

    private void ClearUI(int positionIndex)
    {
        Transform position = lobbyPositionMarkers[positionIndex].Position;
        var nameText = position.Find("nameText").GetComponent<TextMeshProUGUI>();
        var statusText = position.Find("statusText").GetComponent<TextMeshProUGUI>();
        var messageText = position.Find("messageTxt").GetComponent<TextMeshProUGUI>();

        nameText.text = "";
        statusText.text = "";
        messageText.text = "";
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        OnPlayerLeft(runner, runner.LocalPlayer);
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("[Lobby] Lobby scene loaded.");
    }

    #region unused callbacks
    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
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

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }
    #endregion
}
