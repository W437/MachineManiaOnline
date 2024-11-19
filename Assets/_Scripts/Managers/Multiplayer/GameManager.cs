using Cinemachine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using static Fusion.NetworkBehaviour;
using static Unity.Collections.Unicode;

public class GameManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static GameManager Instance { get; private set; }
    [SerializeField] NetworkPrefabRef playerPrefab;
    [SerializeField] public CinemachineVirtualCamera playerCamera;
    private ChangeDetector _changeDetector;

    // Game Vars
    [Networked] float countdownTime { get; set; }
    [Networked] float waitingCountdownTime { get; set; }
    [Networked] public float raceElapsedTime { get; set; }
    [Networked] public float postRaceTime { get; set; }
    [Networked] public int gameState { get; set; } // 0: Waiting, 1: Intro, 2: Countdown, 3: Racing, 4: Finished

    [Networked] public TickTimer gameTimer { get; set; }
    [Networked] public TickTimer postGameTimer { get; set; }
    private bool isTimerRunning = false;

    // Game Players
    [Networked, Capacity(6)] public NetworkLinkedList<PlayerRef> Players => default;


    // Game data
    private Transform startLine;
    private Transform finishLine;
    [Networked] private Vector3 nextSpawnPosition { get; set; }
    public float spawnMargin = 10f; // Distance between players on the start line
    public float postRaceDuration;
    public float waitingDuration;

    public float IntroTransitionTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void Spawned()
    {
        Runner.AddCallbacks(this);

        if (HasStateAuthority)
        {
            FindLevelObjects();
            nextSpawnPosition = startLine.position - new Vector3(spawnMargin*1.25f, 0f, 0);
        }

        // Initial vals
        GameUI.Instance.raceTimerText.text = "00:00:00";

        // Check if this client is the local client
        if (Runner.LocalPlayer != null && Runner.GetPlayerObject(Runner.LocalPlayer) == null)
        {
            SpawnPlayer(Runner, Runner.LocalPlayer);
        }

        if (HasStateAuthority)
        {
            InitializeGame();
        }
    }

    public override void Render()
    {
        if(isTimerRunning && HasStateAuthority)
        {
             //GameUI.Instance.UpdateRaceTimer(GetRaceTime());
        }

    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority || Runner.IsSharedModeMasterClient)
        {
            HandleGameFlow();
        }
    }

    public void StartTimer()
    {
        if (Object.HasStateAuthority)
        {
            gameTimer = TickTimer.CreateFromSeconds(Runner, 0);
            isTimerRunning = true;
            Debug.Log("Timer started");
        }
    }

    public void StopTimer()
    {
        if (Object.HasStateAuthority)
        {
            isTimerRunning = false;
        }
    }

    public void ResetTimer()
    {
        if (Object.HasStateAuthority)
        {
            gameTimer = TickTimer.None;
            isTimerRunning = false;
        }
    }

    public string GetRaceTime()
    {
        if (gameTimer.IsRunning)
        {
            float elapsedTime = (float)(Runner.Tick - gameTimer.TargetTick) * Runner.DeltaTime;
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            int milliseconds = Mathf.FloorToInt((elapsedTime * 1000f) % 1000f);

            return $"{minutes:00}:{seconds:00}:{milliseconds:000}";
        }
        return "0";
    }

    private void FindLevelObjects()
    {
        GameObject startLineObj = GameObject.Find("Level/_StartLine");
        GameObject finishLineObj = GameObject.Find("Level/_FinishLine");

        if (startLineObj != null)
        {
            startLine = startLineObj.transform;
        }
        else
        {
            Debug.LogError("_StartLine object not found in the scene!");
        }

        if (finishLineObj != null)
        {
            finishLine = finishLineObj.transform;
        }
        else
        {
            Debug.LogError("_FinishLine object not found in the scene!");
        }
    }

    /*    public void InitializeGame()
        {
            waitingCountdownTime = waitingDuration;
            gameState = 0; // Waiting for players
            RpcShowWaitingOverlay(true);
            StartCoroutine(WaitForCountdown());
        }*/

    private void InitializeGame()
    {
        gameState = 0; // Waiting state
        StartCoroutine(HandleGameStartSequence());
    }

    private IEnumerator HandleGameStartSequence()
    {
        if (!HasStateAuthority) yield break;

        // step 1: Trigger Intro Animation
        RpcTriggerIntroAnimation();
        gameState = 1; // Intro state
        yield return new WaitForSeconds(IntroTransitionTime); // Allow intro animations and camera hold (3s camera hold included)

        // step 2: Transition camera to player view
        RpcTransitionToPlayerCameras();
        yield return new WaitForSeconds(1.5f);

        // step 3: Start race Countdown
        StartRaceCountdown();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcTransitionToPlayerCameras()
    {
        foreach (PlayerRef player in Players)
        {
            NetworkObject playerObject = Runner.GetPlayerObject(player);
            AttachCamera(playerObject.gameObject);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcTriggerIntroAnimation()
    {
        GameUI.Instance.StartIntroSequence();
    }

    void HandleGameFlow()
    {
        switch (gameState)
        {
            case 0: // Intro, waiting players
                break;

            case 2: // Countdown state
                UpdateCountdown();
                break;

            case 3: // Racing state
                UpdateRaceTimer();
                UpdatePostRaceTimer();
                break;

            case 4: // Finished state
                
                break;
        }
    }


    private IEnumerator WaitForCountdown()
    {
        while (waitingCountdownTime > 0f)
        {
            waitingCountdownTime -= Runner.DeltaTime * 100;
            RpcUpdateWaitingOverlay(Mathf.CeilToInt(waitingCountdownTime));
            yield return null;
        }

        RpcShowWaitingOverlay(false);
        StartRaceCountdown();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcShowWaitingOverlay(bool value)
    {
        GameUI.Instance.ShowWaitingOverlay(value);

        // If showing, update the text
        if (value)
        {
            GameUI.Instance.UpdateWaitingText("Waiting for players...\n5"); // Initial text
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcUpdateWaitingOverlay(int timeLeft)
    {
        GameUI.Instance.UpdateWaitingText($"Waiting for players...\n{timeLeft}");
    }

    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        // Use nextSpawnPosition for spawning
        NetworkObject playerObject = runner.Spawn(playerPrefab, nextSpawnPosition, Quaternion.identity, player);
        if (playerObject != null)
        {
            runner.SetPlayerObject(player, playerObject);

/*            if (player == runner.LocalPlayer)
            {
                AttachCamera(playerObject.gameObject);
            }*/

            DisablePlayerMovement(playerObject);

            // Update spawn position for the next player
            nextSpawnPosition -= new Vector3(spawnMargin, 0, 0);
        }
        else
        {
            Debug.LogError($"Failed to spawn Player {player}.");
        }
    }

    private void DisablePlayerMovement(NetworkObject playerObject)
    {
        if (playerObject == null)
        {
            Debug.LogError("PlayerObject is null. Cannot disable movement.");
            return;
        }

        PlayerController playerController = playerObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.CanMove = false;
        }
        else
        {
            Debug.LogWarning($"PlayerController not found on {playerObject.name}. Cannot disable movement.");
        }
    }

    private void SetPlayerPosition(NetworkObject playerObject, Vector3 position)
    {
        if (playerObject == null)
        {
            Debug.LogError("PlayerObject is null. Cannot set position.");
            return;
        }

        playerObject.transform.position = position;
        Debug.Log($"Position set for Player {playerObject.name} to {position}.");
    }


    void StartRaceCountdown()
    {
        gameState = 2;
        countdownTime = 3f;
        RpcGameCountdown(countdownTime);
    }

    public void OnPlayerFinished(PlayerRef player)
    {
        if (!Object.HasStateAuthority)
            return;

        Debug.Log($"Player {player} reached the finish line!");

        // post race countdown - after 1st player finishes
        if (!postGameTimer.IsRunning && HasStateAuthority)
        {
            Debug.Log("Starting post race timer");
            postGameTimer = TickTimer.CreateFromSeconds(Runner, postRaceDuration);
        }

        // Store finish time for player 
        NetworkObject playerObject = Runner.GetPlayerObject(player);
        if (playerObject != null && playerObject.TryGetComponent<PlayerManager>(out var playerManager))
        {
            playerManager.FinishTime = GetRaceTime();
            Debug.Log($"Player {player} finished at time: {playerManager.FinishTime}");
        }

        // Disable movement for the player who finished
        if (playerObject != null && playerObject.TryGetComponent<PlayerController>(out var playerController))
        {
            playerController.CanMove = false;
        }
    }



    void UpdateCountdown()
    {
        countdownTime -= Runner.DeltaTime;
        RpcGameCountdown(countdownTime);

        if (countdownTime <= 0f)
        {
            StartRace();
        }
    }

    void StartRace()
    {
        gameState = 3; // Race started
        Debug.Log("RACE STARTED");
        raceElapsedTime = 0f;
        StartTimer();
        RpcStartRace();
    }

    void UpdateRaceTimer()
    {
        if (gameTimer.IsRunning)
        {
            raceElapsedTime += Runner.DeltaTime;
            RpcUpdateRaceTimer(raceElapsedTime);
        }
    }


    void UpdatePostRaceTimer()
    {
        if (!postGameTimer.IsRunning) return;

        if (postGameTimer.Expired(Runner))
        {
            RpcUpdatePostRaceTimer("Time's up..");
            if(gameState != 4) EndRace();
            return;
        }

        float timeLeft = postGameTimer.RemainingTime(Runner).Value;

        if (HasStateAuthority)
        {
            Debug.Log($"Time left: {timeLeft}");
            RpcUpdatePostRaceTimer(timeLeft.ToString()); // Send time as a string
        }
    }




    public void PlayerDied(PlayerRef player)
    {
        NetworkObject playerObject = Runner.GetPlayerObject(player);
        if (playerObject != null)
        {
            PlayerManager playerManager = playerObject.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.DeathCount++;
            }
        }
    }

    void EndRace()
    {
        if(gameState == 4) return;

        gameState = 4;
        Debug.Log("GAME ENDED");
        RpcEndRace();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcGameCountdown(float timeLeft)
    {
        GameUI.Instance.DisplayCountdown(Mathf.CeilToInt(timeLeft));
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcStartRace()
    {
        foreach (PlayerRef player in Players)
        {
            NetworkObject playerObject = Runner.GetPlayerObject(player);
            if (playerObject != null)
            {
                PlayerController controller = playerObject.GetComponent<PlayerController>();
                if (controller != null)
                {
                    controller.CanMove = true;
                }
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcUpdateRaceTimer(float elapsedTime)
    {
        GameUI.Instance.UpdateRaceTimer(elapsedTime);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcUpdatePostRaceTimer(string timeLeft)
    {
        if (float.TryParse(timeLeft, out float time))
        {
            GameUI.Instance.DisplayPostRaceCountdown(Mathf.CeilToInt(time).ToString());
        }
        else
        {
            GameUI.Instance.DisplayPostRaceCountdown(timeLeft);
        }
    }



    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcPlayerFinished(PlayerRef player, float finishTime)
    {
        //GameUI.Instance.DisplayPlayerFinished(player, finishTime);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcEndRace()
    {
        // Trigger end game UI sequence
        EndGameUI.Instance.ShowEndGameUI();

        // Sort players by finish times and pass them to EndGameUI
        List<NetworkObject> sortedPlayers = GetSortedPlayerObjects();
        //EndGameUI.Instance.SetupRankings(sortedPlayers);
    }

    private List<NetworkObject> GetSortedPlayerObjects()
    {
        List<NetworkObject> playerObjects = new List<NetworkObject>();

        // Collect and sort player objects based on their finish times
        foreach (PlayerRef player in Players)
        {
            NetworkObject playerObject = Runner.GetPlayerObject(player);
            if (playerObject.TryGetComponent(out PlayerManager playerManager))
            {
                playerObjects.Add(playerObject);
            }
        }

        playerObjects.Sort((p1, p2) =>
        {
            float finishTime1 = ParseFinishTime(p1.GetComponent<PlayerManager>().FinishTime);
            float finishTime2 = ParseFinishTime(p2.GetComponent<PlayerManager>().FinishTime);
            return finishTime1.CompareTo(finishTime2); // Ascending order
        });

        return playerObjects;
    }

    private float ParseFinishTime(string finishTime)
    {
        // Assuming format is MM:SS:MS (e.g., "01:23:456")
        string[] parts = finishTime.Split(':');
        if (parts.Length != 3)
        {
            Debug.LogWarning($"Invalid finish time format: {finishTime}");
            return float.MaxValue; // Return a large value to ensure invalid times are sorted last
        }

        if (int.TryParse(parts[0], out int minutes) &&
            int.TryParse(parts[1], out int seconds) &&
            int.TryParse(parts[2], out int milliseconds))
        {
            return minutes * 60 + seconds + milliseconds / 1000f;
        }

        Debug.LogWarning($"Failed to parse finish time: {finishTime}");
        return float.MaxValue; // Fallback for invalid formats
    }


    void AttachCamera(GameObject player)
    {
        var cameraInstance = Instantiate(playerCamera);
        cameraInstance.Follow = player.transform;
    }


    public void PositionPlayers()
    {
        Debug.Log("Positioning Players");
        // Position players at the start line, spaced behind each other
        for (int i = 0; i < Players.Count; i++)
        {
            Debug.Log("For loop");
            PlayerRef player = Players[i];
            NetworkObject playerObject = Runner.GetPlayerObject(player);
            if (playerObject != null)
            {
                if (playerObject.TryGetComponent<PlayerController>(out var controller))
                {
                    Vector3 startPosition = startLine.position - new Vector3(i * spawnMargin, 0, 0);
                    controller.transform.position = startPosition;
                    controller.CanMove = false; // Disable movement until race starts
                }
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
        Debug.Log($"Player {player} joined the game session.");

        // Add player to the shared player list
        if (!Players.Contains(player))
        {
            Players.Add(player);
            Debug.Log($"Player {player} added to player list.");
        }
    }


    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
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

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }
}