using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using Fusion.Addons.Physics;
using System.Xml.Schema;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PrivateLobbyManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static PrivateLobbyManager Instance;
    PrivateLobbyPosition[] lobbyPositionMarkers;
    Dictionary<PlayerRef, NetworkObject> playerObjects = new Dictionary<PlayerRef, NetworkObject>();

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
    }

    private void InitializeLobbyPositions()
    {
        if (HomeUI.Instance != null)
        {
            lobbyPositionMarkers = HomeUI.Instance.PrivateLobbyPositions;
        }
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            InitializeLobbyPositions();
            Runner.AddCallbacks(this);

            // handle local player spawning
            if (Runner.IsSharedModeMasterClient)
            {
                OnPlayerJoined(Runner, Runner.LocalPlayer);
            }
        }
    }

    public void PrintPlayerObjects()
    {
        Debug.Log("Printing Player Objects:");
        foreach (var kvp in playerObjects)
        {
            Debug.Log($"PlayerRef: {kvp.Key}, NetworkObject: {kvp.Value}");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcPrepareForMenu(NetworkObject playerObject)
    {
        PrepareForMenu(playerObject.gameObject);
    }

    private void PrepareForMenu(GameObject playerObject)
    {
        NetworkRigidbody2D networkRb = playerObject.GetComponent<NetworkRigidbody2D>();
        if (networkRb != null)
        {

        }

        Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcSetPlayerPosition(PlayerRef player, int positionIndex)
    {
        if (lobbyPositionMarkers != null && positionIndex < lobbyPositionMarkers.Length)
        {
            Vector3 fixedYPos;
            Vector3 scale;

            switch (positionIndex)
            {
                case 0:
                    fixedYPos = lobbyPositionMarkers[positionIndex].Position.position - new Vector3(0, 0.75f, 0);
                    scale = new Vector3(1, 1, 1);
                break;
                case 1:
                case 2:
                    fixedYPos = lobbyPositionMarkers[positionIndex].Position.position - new Vector3(0, 0.50f, 0);
                    scale = new Vector3(1, 1, 1);
                break;
                case 3:
                    fixedYPos = lobbyPositionMarkers[positionIndex].Position.position - new Vector3(0, 1f, 0);
                    scale = new Vector3(1, 1, 1);
                break;
                default:
                    fixedYPos = lobbyPositionMarkers[positionIndex].Position.position;
                    scale = new Vector3(1, 1, 1);
                break;
            }

            if (!playerObjects.ContainsKey(player))
            {
                NetworkPrefabRef playerPrefab = FusionLauncher.Instance.GetPlayerNetPrefab();
                Debug.Log($"Position for {player} at index {positionIndex} pos {lobbyPositionMarkers[positionIndex].Position.position}");

                var playerObject = Runner.Spawn(playerPrefab, fixedYPos, Quaternion.identity, player);
                playerObjects[player] = playerObject;
                Debug.Log($"Spawned {playerObject}");
                //playerObject.transform.SetParent(lobbyPositionMarkers[positionIndex].Position);

                playerObject.transform.localScale = scale;

                Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Static;
                }

                var sg = playerObject.GetComponent<SortingGroup>();
                sg.sortingOrder = 2;

                lobbyPositionMarkers[positionIndex].IsOccupied = true;


                Transform childTransform = lobbyPositionMarkers[positionIndex].Position.transform.Find("platformImg");
                if (childTransform != null)
                {
                    Image imageComponent = childTransform.GetComponent<Image>();
                    if (imageComponent != null)
                    {
                        imageComponent.color = ColorHelper.HexToColor("ffffff");
                    }
                }
            }
            else
            {
                // Update players positions except local player
                if (player != Runner.LocalPlayer)
                {
                    playerObjects[player].transform.position = lobbyPositionMarkers[positionIndex].Position.position;
                }
            }
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        int positionIndex = GetNextAvailablePosition(player);

        if (positionIndex != -1 && !playerObjects.ContainsKey(player))
        {
            Debug.Log($"Setting pos for {player} at {positionIndex}");
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
            // Find and free the position occupied by this player
            foreach (var kvp in lobbyPositionMarkers)
            {
                if (playerObject.transform.position == kvp.Position.position)
                {
                    kvp.IsOccupied = false;
                    kvp.Position.transform.GetComponentInChildren<SpriteRenderer>().color = ColorHelper.HexToColor("585858");
                    break;
                }
            }
            playerObjects.Remove(player);
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        OnPlayerLeft(runner, runner.LocalPlayer);
    }

    private int GetNextAvailablePosition(PlayerRef player)
    {
        // Assign the master client to position 1, always
        if (player == Runner.LocalPlayer && Runner.IsSharedModeMasterClient)
        {
            return 0;
        }

        // Find the first available position starting from pos2
        for (int i = 1; i < lobbyPositionMarkers.Length; i++)
        {
            if (!lobbyPositionMarkers[i].IsOccupied)
            {
                return i;
            }
        }
        return -1;
    }

    #region unused callbacks
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
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
    #endregion
}
