using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using Fusion.Addons.Physics;
using System.Xml.Schema;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;

public class PrivateLobbyManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static PrivateLobbyManager Instance;

    public PrivateLobbyPosition[] PrivateLobbyPositions { get; private set; }

    public Transform positionsParent;
    private Transform[] positionMarkers;

    private Dictionary<PlayerRef, NetworkObject> playerObjects = new Dictionary<PlayerRef, NetworkObject>();
    private Dictionary<PlayerRef, int> playerPositions = new Dictionary<PlayerRef, int>();



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

        InitializePositionMarkers();
    }

    void InitializePositionMarkers()
    {
        positionsParent = HomeUI.Instance.PositionsParent;
        if (positionsParent != null)
        {
            int childCount = positionsParent.childCount;
            positionMarkers = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                positionMarkers[i] = positionsParent.GetChild(i);
            }
        }
        else
        {
            Debug.LogError("PositionsParent is not assigned!");
        }
    }

    public override void Spawned()
    {
        //Debug.Log("Adding callbacks");
        Runner.AddCallbacks(this);


        if (HasStateAuthority)
        {
            OnPlayerJoined(Runner, Runner.LocalPlayer);
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
    void RpcPrepareForMenu(NetworkObject playerObject)
    {
        PrepareForMenu(playerObject.gameObject);
    }

    void PrepareForMenu(GameObject playerObject)
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
    public void RpcPositionPlayer(PlayerRef player, int positionIndex)
    {
        if (positionIndex < positionMarkers.Length)
        {
            Transform positionMarker = positionMarkers[positionIndex];
            Vector3 position = positionMarker.position + new Vector3(0, -.8f, 0);
            Vector3 scale = GetScaleForPosition(positionIndex);

            if (!playerObjects.ContainsKey(player))
            {
                NetworkPrefabRef playerPrefab = FusionLauncher.Instance.GetPlayerNetPrefab();
                var playerObject = Runner.Spawn(playerPrefab, position, Quaternion.identity, player);
                playerObjects[player] = playerObject;

                playerObject.transform.localScale = scale;
                playerObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

                // Platform color
                Transform platformImg = positionMarker.Find("platformImg");
                if (platformImg != null)
                {
                    Image platformImageComponent = platformImg.GetComponent<Image>();
                    if (platformImageComponent != null)
                    {
                        platformImageComponent.color = ColorHelper.HexToColor("ffffff");
                    }
                }

                // Platform color
                Transform inviteBtn = positionMarker.Find("InviteButton");
                if (inviteBtn != null)
                {
                    inviteBtn.gameObject.SetActive(false);
                }

                // Player name
                PlayerManager playerManager = playerObject.GetComponent<PlayerManager>();
                Transform playerName = positionMarker.Find("PlayerName/NameTxt");
                if (playerName != null)
                {
                    TextMeshProUGUI playerNameText = playerName.GetComponent<TextMeshProUGUI>();
                    if (playerNameText != null && playerManager != null)
                    {
                        playerNameText.text = playerManager.PlayerName;
                    }
                }

                // Stop movement
                PlayerController playerController = playerObject.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.CanMove = false;
                }

                playerPositions[player] = positionIndex;
            }
            else
            {
                if (player != Runner.LocalPlayer)
                {
                    playerObjects[player].transform.position = position;
                }
            }
        }
    }

    private Vector3 GetScaleForPosition(int positionIndex)
    {
        switch (positionIndex)
        {
            case 0: return new Vector3(0.85f, 0.85f, 0.85f);
            case 1: case 2: return new Vector3(0.75f, 0.75f, 0.75f);
            case 3: return new Vector3(0.68f, 0.68f, 0.68f);
            default: return new Vector3(0.85f, 0.85f, 0.85f);
        }
    }

    private int GetNextAvailablePosition()
    {
        for (int i = 0; i < positionMarkers.Length; i++)
        {
            if (!playerPositions.ContainsValue(i)) 
            {
                return i;
            }
        }
        return -1;
    }

    private void ReturnAllPlayersToPrivateLobbies()
    {
        foreach (var playerRef in playerObjects.Keys)
        {
            if (playerRef != Runner.LocalPlayer)
            {
                GameLauncher.Instance.StartInitialGameSession();
            }
        }
        Runner.Shutdown();
    }


    /// Callbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        int positionIndex = GetNextAvailablePosition();

        if (positionIndex != -1)
        {
            Debug.Log($"Player Joined {player} at position {positionIndex}");
            RpcPositionPlayer(player, positionIndex);
        }
        else
        {
            Debug.LogWarning("No available positions for the player");
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (playerObjects.ContainsKey(player))
        {
            var playerObject = playerObjects[player];
            if (playerObject != null)
            {
                int positionIndex = playerPositions[player];
                Transform positionMarker = positionMarkers[positionIndex];
                Transform platformImg = positionMarker.Find("platformImg");
                if (platformImg != null)
                {
                    Image platformImageComponent = platformImg.GetComponent<Image>();
                    if (platformImageComponent != null)
                    {
                        platformImageComponent.color = ColorHelper.HexToColor("6C6C6C");
                    }
                }

                Transform playerName = positionMarker.Find("PlayerName");
                if (playerName != null)
                {
                    playerName.gameObject.SetActive(false);
                }

                runner.Despawn(playerObject);
            }

            playerObjects.Remove(player);
            playerPositions.Remove(player);
        }

        // If State Authority leaves, return all players to their private lobbies
        if (HasStateAuthority && player == Runner.LocalPlayer)
        {
            ReturnAllPlayersToPrivateLobbies();
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        //OnPlayerLeft(runner, runner.LocalPlayer);
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
