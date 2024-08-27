using Cinemachine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour, INetworkRunnerCallbacks
{

    /// <summary>
    /// must continue linking stuff together, playermanager, gamemanager, lobby
    /// move Spawned() start game timer to Lobby. Whoever has state authority of lobby, spawns the gamemanager.
    /// figure other playermanager stats.
    /// 
    /// NEW::
    /// 
    /// gamemanager has to talk with level manager to get:
    /// 1. Finish line
    /// 2. Start Line
    /// 3. Player Start Positionss
    /// </summary>
    public static GameManager Instance { get; private set; }
    [Networked] private float countdownTime { get; set; } // Time remaining for countdown
    [Networked] private float elapsedTime { get; set; } // Elapsed game time
    [Networked] private float postRaceTime { get; set; } // Time remaining after race finishes

    [Networked] private int gameState { get; set; }  // 0: Waiting, 1: Countdown, 2: Racing, 3: Finished
    [Networked, Capacity(6)] public NetworkLinkedList<PlayerRef> players => default;

    public List<Transform> playerStartPositions = new List<Transform>();
    public Transform finishLine;

    private Dictionary<PlayerRef, PlayerController> playerControllers = new Dictionary<PlayerRef, PlayerController>();

    public float raceDuration;
    private bool isRaceStarted = false;

    private void Awake()
    {
        // Cap fps
        //Application.targetFrameRate = 60;

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

    public override void Spawned()
    {
        //Debug.Log($"Spawned {this}");
        InitializeGame();

        // Spawn the player prefab
        NetworkObject playerObject = Runner.Spawn(FusionLauncher.Instance.GetPlayerNetPrefab());
        var cameraPrefab = Instantiate(FusionLauncher.Instance.GetCameraPrefab());

        var virtualCam = cameraPrefab.GetComponent<CinemachineVirtualCamera>();
        virtualCam.Follow = playerObject.transform;
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority || Runner.IsSharedModeMasterClient)
        {
            HandleGameFlow();
        }
    }

    private void HandleGameFlow()
    {
        switch (gameState)
        {
            case 1: // Countdown state
                UpdateCountdown();
            break;

            case 2: // Racing state
                if (!isRaceStarted)
                {
                    isRaceStarted = true;
                    elapsedTime = 0f;
                }
                UpdateRaceTimer();
            break;

            case 3: // Finished state
                UpdatePostRaceTimer();
            break;
        }
    }

    public void InitializeGame()
    {
        gameState = 0;
        countdownTime = 3f;
        GameUI.Instance.raceTimerText.text = "0:00:00";
    }

    private void UpdateCountdown()
    {
        countdownTime -= Time.deltaTime;
        RpcGameCountdown(countdownTime);

        if (countdownTime <= 0f)
        {
            StartRace();
        }
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcGameCountdown(float timeLeft)
    {
        GameUI.Instance.DisplayCountdown(Mathf.CeilToInt(timeLeft));
    }

    private void StartRace()
    {
        gameState = 2;
        RpcStartRace();
    }

    private void UpdateRaceTimer()
    {
        elapsedTime += Time.deltaTime;
        RpcUpdateRaceTimer(elapsedTime);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcUpdateRaceTimer(float elapsedTime)
    {
        GameUI.Instance.UpdateRaceTimer(elapsedTime);
    }

    private void UpdatePostRaceTimer()
    {
        postRaceTime -= Time.deltaTime;
        RpcUpdatePostRaceTimer(postRaceTime);

        if (postRaceTime <= 0f)
        {
            // Handle post-race logic here
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcUpdatePostRaceTimer(float timeLeft)
    {
        GameUI.Instance.DisplayPostRaceCountdown(timeLeft);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcStartRace()
    {
        GameUI.Instance.HideCountdown(); 
        GameUI.Instance.StartRaceTimer();
    }

    public void PlayerFinished(PlayerRef player)
    {
        if (!playerControllers.ContainsKey(player)) return;

        playerControllers[player].FinishTime = elapsedTime;
        RpcPlayerFinished(playerControllers[player].FinishTime);

        if (playerControllers.Count == players.Count)
        {
            postRaceTime = 25f; // 25-second post-race countdown
            gameState = 3;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcPlayerFinished(float finishTime)
    {
        // Update finish time UI for all players
    }

    public void EndGame(Dictionary<PlayerRef, float> playerFinishTimes)
    {
        gameState = 0;

        var playerRefs = new PlayerRef[playerFinishTimes.Count];
        var finishTimes = new float[playerFinishTimes.Count];
        int index = 0;

        foreach (var kvp in playerFinishTimes)
        {
            playerRefs[index] = kvp.Key;
            finishTimes[index] = kvp.Value;
            index++;
        }

        RpcEndGame(playerRefs, finishTimes);
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcEndGame(PlayerRef[] playerRefs, float[] finishTimes)
    {
        var playerFinishTimes = new Dictionary<PlayerRef, float>();
        for (int i = 0; i < playerRefs.Length; i++)
        {
            playerFinishTimes[playerRefs[i]] = finishTimes[i];
        }

        GameUI.Instance.DisplayEndGameStats(playerFinishTimes);
    }


    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player joined: {player}");

        // Determine the player's start position
        int playerIndex = players.IndexOf(player);
        Vector3 startPosition = playerIndex >= 0 && playerIndex < playerStartPositions.Count
            ? playerStartPositions[playerIndex].position
            : Vector3.zero; // Default position if not defined

        // Spawn the player prefab
        NetworkObject playerObject = runner.Spawn(
            FusionLauncher.Instance.GetPlayerNetPrefab(),
            startPosition,
            Quaternion.identity,
            player
        );

        // Initialize player controller
        PlayerController controller = playerObject.GetComponent<PlayerController>();
        if (controller != null)
        {
            playerControllers[player] = controller;
        }

        Debug.Log($"Spawned player {player} at position {startPosition}");
    }


    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        throw new NotImplementedException();
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }
}
