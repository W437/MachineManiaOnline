using Cinemachine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static GameManager Instance { get; private set; }
    [Networked] float countdownTime { get; set; }
    [Networked] float elapsedTime { get; set; } 
    [Networked] float postRaceTime { get; set; } 

    [Networked] int gameState { get; set; } // 0: Waiting, 1: Countdown, 2: Racing, 3: Finished
    [Networked, Capacity(6)] public NetworkLinkedList<PlayerRef> players => default;

    public List<Transform> playerStartPositions = new List<Transform>();
    public Transform finishLine;

    Dictionary<PlayerRef, PlayerController> playerControllers = new Dictionary<PlayerRef, PlayerController>();

    public float raceDuration;
    bool isRaceStarted = false;

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
    }

    public override void Spawned()
    {
        InitializeGame();
        LoadLevel();
        AudioManager.Instance.PlaySpecificSoundtrack(AudioManager.Instance.yaketySax);
        AudioManager.Instance.SetCutoffFrequency(5500, 1.5f);
//
        // Spawn the player prefab
        Vector3 playerPos = new Vector3(-4.06118011f, 43.3153305f, 3);
        NetworkObject playerObject = Runner.Spawn(FusionLauncher.Instance.GetPlayerNetPrefab(), playerPos, Quaternion.identity, Runner.LocalPlayer);


        if (playerObject != null)
            Runner.SetPlayerObject(Runner.LocalPlayer, playerObject);

        // assign kamm
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

    public void InitializeGame()
    {
        gameState = 0;
        countdownTime = 3f;
        GameUI.Instance.raceTimerText.text = "0:00:00";
    }

    public void PlayerFinished(PlayerRef player)
    {
        if (!playerControllers.ContainsKey(player)) return;

        playerControllers[player].FinishTime = elapsedTime;
        RpcPlayerFinished(player, playerControllers[player].FinishTime);

        if (playerControllers.Count == players.Count)
        {
            gameState = 3;
            postRaceTime = 25f;
        }
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

    void StartRace()
    {
        gameState = 2;
        RpcStartRace();
    }

    void UpdateRaceTimer()
    {
        elapsedTime += Time.deltaTime;
        RpcUpdateRaceTimer(elapsedTime);
    }

    void LoadLevel()
    {
        // Load the level prefab from Resources/Levels
        string levelName = "Level1"; // Replace with logic to choose different levels
        var levelPrefab = Resources.Load<GameObject>($"Levels/{levelName}");
        if (levelPrefab != null)
        {
            Instantiate(levelPrefab);
            // Find Start and Finish lines within the level
            Transform startLine = GameObject.Find("StartLine").transform;
            finishLine = GameObject.Find("FinishLine").transform;

            // Populate the playerStartPositions list
            for (int i = 0; i < 6; i++)
            {
                Vector3 startPos = startLine.position - new Vector3(i * 2.0f, 0, 0); // Players spaced behind each other
                var tempPos = new GameObject($"PlayerStartPos{i}").transform;
                tempPos.position = startPos;
                playerStartPositions.Add(tempPos);
            }
        }
    }

    void HandleGameFlow()
    {
        switch (gameState)
        {
            case 0: // Waiting for players
                if (players.Count == Runner.SessionInfo.MaxPlayers)
                {
                    ShowWaitingScreen();
                    PositionPlayers();
                    StartCountdown();
                }
                break;
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

    void ShowWaitingScreen()
    {
        GameUI.Instance.DisplayCountdown(3); // Display "Waiting for Players" message
        // Disable player objects (hide them) until countdown finishes
        foreach (var playerController in playerControllers.Values)
        {
            playerController.gameObject.SetActive(false);
        }
    }

    void PositionPlayers()
    {
        for (int i = 0; i < players.Count; i++)
        {
            PlayerRef player = players[i];
            if (playerControllers.TryGetValue(player, out PlayerController controller))
            {
                controller.transform.position = playerStartPositions[i].position;
                controller.transform.rotation = playerStartPositions[i].rotation;
            }
        }
    }

    void StartCountdown()
    {
        gameState = 1;
        countdownTime = 3f;
    }

    void UpdateCountdown()
    {
        countdownTime -= Time.deltaTime;
        RpcGameCountdown(countdownTime);

        if (countdownTime <= 0f)
        {
            StartRace();
        }
    }
    
    void UpdatePostRaceTimer()
    {
        postRaceTime -= Time.deltaTime;
        RpcUpdatePostRaceTimer(postRaceTime);

        if (postRaceTime <= 0f)
        {
            // Handle post-race logic here
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcGameCountdown(float timeLeft)
    {
        GameUI.Instance.DisplayCountdown(Mathf.CeilToInt(timeLeft));
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcPlayerFinished(PlayerRef player, float finishTime)
    {
        GameUI.Instance.DisplayEndGameStats(new Dictionary<PlayerRef, float> { { player, finishTime } });
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcUpdateRaceTimer(float elapsedTime)
    {
        GameUI.Instance.UpdateRaceTimer(elapsedTime);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcUpdatePostRaceTimer(float timeLeft)
    {
        GameUI.Instance.DisplayPostRaceCountdown(timeLeft);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcStartRace()
    {
        GameUI.Instance.HideCountdown();
        GameUI.Instance.StartRaceTimer();

        // Enable player objects and allow them to move
        foreach (var playerController in playerControllers.Values)
        {
            playerController.gameObject.SetActive(true);
            playerController.CanMove = true; // Allow players to move
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcPlayerFinished(float finishTime)
    {
        // Update finish time UI for all players
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcEndGame(PlayerRef[] playerRefs, float[] finishTimes)
    {
        var playerFinishTimes = new Dictionary<PlayerRef, float>();
        for (int i = 0; i < playerRefs.Length; i++)
        {
            playerFinishTimes[playerRefs[i]] = finishTimes[i];
        }

        GameUI.Instance.DisplayEndGameStats(playerFinishTimes);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (HasStateAuthority)
        {
            players.Add(player);

            int playerIndex = players.IndexOf(player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (playerControllers.ContainsKey(player))
        {
            playerControllers.Remove(player);
        }
    }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        //Destroy(runner);
        Destroy(this);
    }

    #region unused callbacks
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
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
    #endregion
}
