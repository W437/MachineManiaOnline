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
using System.Linq;


/// <summary>
/// MANAGER SPAWNS ON ALL CLIENTS
/// TRIED OTHERWISE, BUT HAVE TO REFACTOR THE WHOLE LOBBY MANAGER
/// FOR NOW, RUNS OK
/// - READY STATES NOT OK
/// </summary>
public class PublicLobbyManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static PublicLobbyManager Instance;
    public ManiaNews ManiaNews;
    public PublicLobbyPosition[] playerPosition;
    public Dictionary<PlayerRef, NetworkObject> playerObjects = new Dictionary<PlayerRef, NetworkObject>();

    const int MAX_PLAYERS = 6;
    const int LOBBY_TIMER_START = 25;
    static LobbyHubManager _hubManager;
    SceneRef _gameScene;

    [Networked] public bool net_isSpawned { get; set; }
    [Networked] private bool net_isTimerRunning { get; set; }
    [Networked] private float net_remainingTime { get; set; }



    void Awake()
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
        PublicLobbyUI.Instance.ConnectToLobby();
        _gameScene = SceneRef.FromIndex(1);
        _hubManager = new LobbyHubManager();
    }

    void Update()
    {
        if (net_isSpawned && HasStateAuthority)
        {
            UpdateLobbyTimer();
        }
    }
    
    public override void Spawned()
    {
        Runner.AddCallbacks(this);
        if (HasStateAuthority)
        {
            ManiaNews = new ManiaNews(PublicLobbyUI.Instance.maniaNewsParent, 3.5f);
            ManiaNews.OnNewsChanged += OnNewsChanged;
            net_isSpawned = true;
            SelectRandomNews();
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
   
    public int FindPlayerPosition(PlayerRef player)
    {
        for (int i = 0; i < playerPosition.Length; i++)
        {
            if (playerObjects.ContainsKey(player) && playerPosition[i].PlayerRef == player)
            {
                return i;
            }
        }
        return -1;
    }

    public void SetPlayerReadyState(PlayerRef player)
    {
        RpcToggleReadyState(player);
    }

    public bool IsPlayerReady(PlayerRef player)
    {
        var playerObject = Runner.GetPlayerObject(player);
        if (playerObject != null)
        {
            var playerManager = playerObject.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                return playerManager.IsReady;
            }
        }
        return false;
    }
    
    void InitializeLobbyPositions()
    {
        if (PublicLobbyUI.Instance != null)
        {
            playerPosition = new PublicLobbyPosition[MAX_PLAYERS];
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                Transform slotTransform = PublicLobbyUI.Instance.PlayerSlotsParent.GetChild(i);
                playerPosition[i] = new PublicLobbyPosition
                {
                    Position = slotTransform,
                    IsOccupied = false
                };
            }
        }
    }

    void UpdateLobbyTimer()
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

    void UpdateTimerUI()
    {
        int remainingSeconds = Mathf.CeilToInt(net_remainingTime);
        PublicLobbyUI.Instance.gameStartLobbyTimer.text = remainingSeconds.ToString() + "s";

        // Broadcast the updated time to all clients
        RpcUpdateTimer(net_remainingTime);
    }

    void ShowConnectingOverlay()
    {
        PublicLobbyUI.Instance.ConnectingOverlay.SetActive(true);
        PublicLobbyUI.Instance.ConnectingText.text = "Joining Game";
    }

    void StartGame()
    {
        if (HasStateAuthority)
        {
            RpcStartGame();
        }
    }

    void UpdatePlayerUI(Transform slotTransform, PlayerRef player)
    {
        if (playerObjects.TryGetValue(player, out var playerObject))
        {
            var playerManager = playerObject.GetComponent<PlayerManager>();
            var nameText = slotTransform.Find("nameText").GetComponent<TextMeshProUGUI>();
            var statusText = slotTransform.Find("statusText").GetComponent<TextMeshProUGUI>();
            var messageText = slotTransform.Find("messageTxt").GetComponent<TextMeshProUGUI>();

            if (playerManager != null)
            {
                nameText.text = $"{playerManager.PlayerName} ({player.PlayerId})";
                statusText.text = playerManager.IsReady ? "Ready" : "Not Ready";
                messageText.text = "";
            }
        }
        else
        {
            Debug.LogError($"Player object not found for player: {player.PlayerId}");
        }
    }

    void SpawnPlayer(PlayerRef player, int positionIndex)
    {
        if (playerPosition != null && positionIndex < playerPosition.Length)
        {
            NetworkPrefabRef playerPrefab = FusionLauncher.Instance.GetPlayerNetPrefab();
            Vector3 spawnPos = playerPosition[positionIndex].Position.position - new Vector3(0, 50f, 0);

            var playerObject = Runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);
            playerObjects.Add(player, playerObject);
            Runner.SetPlayerObject(player, playerObject);

            RpcPositionPlayer(player, positionIndex);
        }
        else
        {
            Debug.LogError("Lobby position markers are null or index is out of range.");
        }
    }

    int GetNextAvailablePosition(PlayerRef player)
    {
        if (player == Runner.LocalPlayer && HasStateAuthority)
        {
            return 0;
        }

        for (int i = 1; i < playerPosition.Length; i++)
        {
            if (!playerPosition[i].IsOccupied)
            {
                return i;
            }
        }
        return -1;
    }
    
    void ClearUI(int positionIndex)
    {
        Transform position = playerPosition[positionIndex].Position;
        var nameText = position.Find("nameText").GetComponent<TextMeshProUGUI>();
        var statusText = position.Find("statusText").GetComponent<TextMeshProUGUI>();
        var messageText = position.Find("messageTxt").GetComponent<TextMeshProUGUI>();

        nameText.text = "";
        statusText.text = "";
        messageText.text = "";
    }

    void InitiateMasterClientTransfer()
    {
        if (playerObjects.Count > 0)
        {
            PlayerRef newMasterClient = playerObjects.Keys.FirstOrDefault();

            if (newMasterClient != null)
            { 
                RpcNotifyNewMasterClient(newMasterClient);

            }
        }
        else
        {
            Debug.Log("No players left to transfer authority to.");
            return;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcUpdateTimer(float newTime)
    {
        if (!HasStateAuthority)
        {
            net_remainingTime = newTime;
            int remainingSeconds = Mathf.CeilToInt(net_remainingTime);
            PublicLobbyUI.Instance.gameStartLobbyTimer.text = remainingSeconds.ToString() + "s";
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcStartGame()
    {
        if (HasStateAuthority)
        {
            if (Runner.IsRunning)
            {
                Runner.LoadScene(_gameScene, LoadSceneMode.Single);
                Runner.Spawn(FusionLauncher.Instance.GetGameManagerNetPrefab());
                Destroy(gameObject);
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcToggleReadyState(PlayerRef player)
    {
        var playerObject = Runner.GetPlayerObject(player);

        if (playerObject != null)
        {
            var playerManager = playerObject.GetComponent<PlayerManager>();

            if (playerManager != null)
            {
                bool newReadyState = playerManager.IsReady;
                RpcUpdateReadyStateUI(player, newReadyState);
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcShowEmote(PlayerRef player, string emote)
    {
        var playerObject = Runner.GetPlayerObject(player);
        if (playerObject != null)
        {
            var playerManager = playerObject.GetComponent<PlayerManager>();
            if (playerManager != null && playerManager.CanInteract())
            {
                _hubManager.ShowEmote(player, emote);
                playerManager.UpdateCooldown();
            }
            else
            {
                Debug.Log($"Player {player.PlayerId} is on cooldown.");
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcShowMessage(PlayerRef player, string message)
    {
        _hubManager.ShowMessage(player, message);
    }
    
    [Rpc(RpcSources.All, RpcTargets.All)]
    void RpcUpdateReadyStateUI(PlayerRef player, bool isReady)
    {
        int playerIndex = FindPlayerPosition(player);
        if (playerIndex >= 0)
        {
            Transform slotTransform = playerPosition[playerIndex].Position;
            var statusText = slotTransform.Find("statusText").GetComponent<TextMeshProUGUI>();
            if (statusText != null)
            {
                statusText.text = isReady ? "Ready" : "Not Ready";
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RpcPositionPlayer(PlayerRef player, int posIndex)
    {
        if (playerObjects.TryGetValue(player, out var playerObj))
        { 
            var parentTransform = playerPosition[posIndex].Position;
            playerObj.transform.SetParent(parentTransform);

            playerObj.transform.localPosition = Vector3.zero;
            playerObj.transform.localScale = new Vector3(42, 42, 42);
            playerObj.transform.localPosition += new Vector3(0, -50f, 0);

            var sortingGroup = playerObj.GetComponent<SortingGroup>();
            if (sortingGroup != null) sortingGroup.sortingOrder = 6;

            var rb = playerObj.GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Static;

            playerPosition[posIndex].IsOccupied = true;
            playerPosition[posIndex].PlayerRef = player;
            UpdatePlayerUI(playerPosition[posIndex].Position, player);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcNotifyNewMasterClient(PlayerRef newMasterClient)
    {
        NetworkObject lobbyManager = FindObjectOfType<PublicLobbyManager>().GetComponent<NetworkObject>();

        if (Runner.LocalPlayer == newMasterClient)
        {
            lobbyManager.RequestStateAuthority();
            Debug.Log($"Requested state authority for new master: {newMasterClient}");
        }
    }

    #region Mania News
    void OnNewsChanged(int newsIndex)
    {
        if (HasStateAuthority && Runner.IsSharedModeMasterClient)
        {
            if (newsIndex == -1)
            {
                SelectRandomNews();
            }
            else
            {
                RpcShowNewsItem(newsIndex);
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcShowNewsItem(int newsIndex)
    {
        ManiaNews.ShowSpecificNews(newsIndex);
    }

    void SelectRandomNews()
    {
        if (HasStateAuthority && Runner.IsSharedModeMasterClient)
        {
            if (!ManiaNews.IsTransitioning())
            {
                int randomIndex = UnityEngine.Random.Range(0, ManiaNews.GetNewsPrefabsCount());
                RpcShowNewsItem(randomIndex); 
            }
        }
    }
    #endregion

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (HasStateAuthority)
        {
            int posIndex = GetNextAvailablePosition(player);
            if (posIndex != -1 && !playerObjects.ContainsKey(player))
            {
                Debug.Log($"Player {player} doesn't exist, spawning him");
                SpawnPlayer(player, posIndex);
            }
            if(Runner.IsSharedModeMasterClient)
                StartLobbyTimer();
        }
        else
        {
            Debug.Log($"Player joined. {player}");
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

                for (int i = 0; i < playerPosition.Length; i++)
                {
                    if (playerObject.transform.position == playerPosition[i].Position.position)
                    {
                        playerPosition[i].IsOccupied = false;
                        ClearUI(i);
                        break;
                    }
                }
            }

            playerObjects.Remove(player);
        }

        if (player.IsMasterClient)
        {
            InitiateMasterClientTransfer();
        }
    }

    // Callbacks
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        OnPlayerLeft(runner, runner.LocalPlayer);
    }

    #region unused callbacks
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

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        OnPlayerLeft(runner, runner.LocalPlayer);
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
