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
    [Networked] private TickTimer raceCountdownTimer { get; set; }
    [Networked] private TickTimer postRaceCountdownTimer { get; set; }
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
        Application.targetFrameRate = 60;

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
        InitializeGame();
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority || FusionLauncher.Instance.GetNetworkRunner().IsSharedModeMasterClient)
        {
            HandleGameFlow();
        }
    }

    private void HandleGameFlow()
    {
        switch (gameState)
        {
            case 1: 
                if (raceCountdownTimer.Expired(Runner))
                {
                    StartRace();
                }
            break;

            case 2:
                if (!isRaceStarted)
                {
                    isRaceStarted = true;
                    raceDuration = Time.time;
                }
                // UpdatePlayerRankings();
            break;

            case 3:
                if (postRaceCountdownTimer.Expired(Runner))
                {
                    var playerFinishTimes = new Dictionary<PlayerRef, float>();
                    foreach (var player in players)
                    {
                        if (playerControllers.ContainsKey(player))
                        {
                            playerFinishTimes[player] = playerControllers[player].FinishTime;
                        }
                    }
                    EndGame(playerFinishTimes);
                }
                break;
        }
    }


    public void InitializeGame()
    {
        gameState = 0;
    }

    private void StartGameCountdown()
    {
        raceCountdownTimer = TickTimer.CreateFromSeconds(Runner, 3);
        gameState = 1;
        RpcGameCountdown();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcGameCountdown()
    {
        UIGame.Instance.DisplayCountdown(3);
    }

    private void StartRace()
    {
        gameState = 2;
        RpcStartRace();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcStartRace()
    {
        UIGame.Instance.HideCountdown(); 
        UIGame.Instance.StartRaceTimer();
    }

    public void PlayerFinished(PlayerRef player)
    {
        if (!playerControllers.ContainsKey(player)) return;

        playerControllers[player].FinishTime = Time.time - raceDuration;
        RpcPlayerFinished(playerControllers[player].FinishTime);

        if (playerControllers.Count == players.Count) 
        {
            postRaceCountdownTimer = TickTimer.CreateFromSeconds(Runner, 25);
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

        UIGame.Instance.DisplayEndGameStats(playerFinishTimes);
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
        runner.Spawn(FusionLauncher.Instance.GetPlayerNetPrefab());
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
