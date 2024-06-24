using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using Cinemachine;

public class M_Network : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner _runner;
    public NetworkSceneManagerDefault _SceneManager;
    public static M_Network Instance;

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
            return;
        }

        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            _runner.AddCallbacks(this);
        }

        if (_SceneManager == null)
        {
            _SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        ServiceLocator.RegisterNetworkManager(this);
    }

    public NetworkRunner GetNetworkRunner() { return _runner; }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} joined. IsRunnerInitialized: {runner != null}, LocalPlayer: {runner.LocalPlayer}, IsLocalPlayerValid: {!runner.LocalPlayer.IsNone}");
        ServiceLocator.GetLobbyManager().AddPlayer(player.AsIndex, $"Player {player}");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        ServiceLocator.GetLobbyManager().RemovePlayer(player.AsIndex);
    }


    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private GameObject cameraPrefab;
    public void SpawnPlayer(PlayerRef player)
    {
        if (_runner.LocalPlayer == player)
        {
            NetworkObject playerObject = _runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
            Debug.Log($"PlayerManager: Player {player} spawned with NetworkObject ID: {playerObject?.NetworkTypeId}");

            GameObject cameraInstance = Instantiate(cameraPrefab);
            CinemachineVirtualCamera virtualCamera = cameraInstance.GetComponent<CinemachineVirtualCamera>();
            if (virtualCamera != null)
            {
                virtualCamera.Follow = playerObject.transform;
            }
            else
            {
                Debug.LogError("PlayerManager: CinemachineVirtualCamera component not found on camera prefab.");
            }
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnSceneLoadDone(NetworkRunner runner) 
    {
        Debug.Log("Scene load done");
        SpawnPlayer(_runner.LocalPlayer); 
    }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
