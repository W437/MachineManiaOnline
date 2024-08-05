using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using Fusion.Addons.Physics;

public class PrivateLobbyManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static PrivateLobbyManager Instance;
    private Transform[] lobbyPositionMarkers;
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
    }

    private void InitializeLobbyPositions()
    {
        if (UIHome.Instance.PrivateLobbyPositions != null)
        {
            lobbyPositionMarkers = UIHome.Instance.PrivateLobbyPositions.GetComponentsInChildren<Transform>();
        }
    }

    public override void Spawned()
    {
        Runner.AddCallbacks(this);

        // Spawn local player in private lobby
        if (HasStateAuthority && !UILobby.Instance.lobbyPlatformScreen.activeSelf)
        {
            SpawnLocalPlayer();
        }
    }

    public void SpawnLocalPlayer()
    {
        if (!playerObjects.ContainsKey(Runner.LocalPlayer))
        {
            // local player's at pos1
            NetworkPrefabRef playerPrefab = FusionLauncher.Instance.GetPlayerNetPrefab();
            var playerObject = Runner.Spawn(playerPrefab, lobbyPositionMarkers[1].position, Quaternion.identity, Runner.LocalPlayer);
            playerObjects[Runner.LocalPlayer] = playerObject;
            PrepareForMenu(playerObject.gameObject);

            // Notify others 
            RpcSetPlayerPosition(Runner.LocalPlayer, 1); // pos1 is index 1 ALWAYS
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

        playerObject.transform.position += new Vector3(0, -0.38f, 3);
        playerObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

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

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RpcSetPlayerPosition(PlayerRef player, int positionIndex)
    {
        if (lobbyPositionMarkers != null && positionIndex < lobbyPositionMarkers.Length)
        {
            if (!playerObjects.ContainsKey(player))
            {
                // Spawn other players
                NetworkPrefabRef playerPrefab = FusionLauncher.Instance.GetPlayerNetPrefab();
                var playerObject = Runner.Spawn(playerPrefab, lobbyPositionMarkers[positionIndex].position, Quaternion.identity, player);
                playerObjects[player] = playerObject;
            }
            else
            {
                // Update position for existing player objects
                playerObjects[player].transform.position = lobbyPositionMarkers[positionIndex].position;
            }
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        int positionIndex = GetNextAvailablePosition();
        if (positionIndex != -1)
        {
            RpcSetPlayerPosition(player, positionIndex);
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
            playerObjects.Remove(player);
        }
    }

    private int GetNextAvailablePosition()
    {
        // Find the first available position starting from pos2 (index 2)
        for (int i = 2; i < lobbyPositionMarkers.Length; i++)
        {
            if (IsPositionAvailable(i))
            {
                return i;
            }
        }
        return -1;
    }

    private bool IsPositionAvailable(int index)
    {
        foreach (var playerObject in playerObjects.Values)
        {
            if (playerObject.transform.position == lobbyPositionMarkers[index].position)
            {
                return false;
            }
        }
        return true;
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
}
