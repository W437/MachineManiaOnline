using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Networked] float countdownTime { get; set; }
    [Networked] float raceElapsedTime { get; set; }
    [Networked] float postRaceTime { get; set; }

    [Networked] int gameState { get; set; } // 0: Waiting, 1: Countdown, 2: Racing, 3: Finished
    [Networked, Capacity(6)] public NetworkLinkedList<PlayerRef> players => default;

    public Transform startLine;
    public float spawnMargin = 2f; // Distance between players on the start line
    public float postRaceDuration = 25f;

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
        SpawnLocalPlayer(); // Only spawn the local player when the game starts
    }

    void InitializeGame()
    {
        gameState = 0; // Waiting for players
        countdownTime = 3f;
        raceElapsedTime = 0f;
        postRaceTime = postRaceDuration;
        GameUI.Instance.raceTimerText.text = "00:00:00";
    }

    // Spawn only the local player when they join
    public void SpawnLocalPlayer()
    {
        Vector3 playerPos = startLine.position; // Default spawn position for the local player
        NetworkObject playerObject = Runner.Spawn(FusionLauncher.Instance.GetPlayerNetPrefab(), playerPos, Quaternion.identity, Runner.LocalPlayer);

        if (playerObject != null)
        {
            Runner.SetPlayerObject(Runner.LocalPlayer, playerObject);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority || Runner.IsSharedModeMasterClient)
        {
            HandleGameFlow();
        }
    }

    void HandleGameFlow()
    {
        switch (gameState)
        {
            case 0: // Waiting for players to join
                if (players.Count == Runner.SessionInfo.MaxPlayers)
                {
                    StartCountdown();
                }
                break;

            case 1: // Countdown state
                UpdateCountdown();
                break;

            case 2: // Racing state
                UpdateRaceTimer();
                break;

            case 3: // Finished state (post-race countdown)
                UpdatePostRaceTimer();
                break;
        }
    }

    void StartCountdown()
    {
        gameState = 1; // Countdown phase
        countdownTime = 3f;
        RpcGameCountdown(countdownTime);
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

    void StartRace()
    {
        gameState = 2; // Race started
        raceElapsedTime = 0f;
        RpcStartRace();
    }

    void UpdateRaceTimer()
    {
        raceElapsedTime += Time.deltaTime;
        RpcUpdateRaceTimer(raceElapsedTime);
    }

    void UpdatePostRaceTimer()
    {
        postRaceTime -= Time.deltaTime;
        RpcUpdatePostRaceTimer(postRaceTime);

        if (postRaceTime <= 0f)
        {
            EndRace();
        }
    }

    public void PlayerFinished(PlayerRef player)
    {
        // Stop player's movement when they finish the race
        NetworkObject playerObject = Runner.GetPlayerObject(player);
        if (playerObject != null)
        {
            PlayerController controller = playerObject.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.CanMove = false;
                controller.FinishTime = raceElapsedTime;
            }
        }

        // Trigger post-race countdown if this is the first player
        if (gameState != 3)
        {
            gameState = 3; // Race finished
            postRaceTime = postRaceDuration;
        }

        RpcPlayerFinished(player, raceElapsedTime);
    }

    void EndRace()
    {
        gameState = 0; // Reset for the next race
        RpcEndRace();
    }

    // RPCs to sync state across all clients
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcGameCountdown(float timeLeft)
    {
        GameUI.Instance.DisplayCountdown(Mathf.CeilToInt(timeLeft));
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcStartRace()
    {
        GameUI.Instance.HideCountdown();
        GameUI.Instance.StartRaceTimer();

        // Enable movement for all players
        foreach (PlayerRef player in players)
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
    void RpcUpdatePostRaceTimer(float timeLeft)
    {
        GameUI.Instance.DisplayPostRaceCountdown(timeLeft);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcPlayerFinished(PlayerRef player, float finishTime)
    {
        //GameUI.Instance.DisplayPlayerFinished(player, finishTime);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcEndRace()
    {
        //GameUI.Instance.DisplayEndGameStats();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (HasStateAuthority)
        {
            players.Add(player);

            // Once all players have joined, position them at the start line
            PositionPlayers();
        }
    }

    public void PositionPlayers()
    {
        // Position players at the start line, spaced behind each other
        for (int i = 0; i < players.Count; i++)
        {
            PlayerRef player = players[i];
            NetworkObject playerObject = Runner.GetPlayerObject(player);
            if (playerObject != null)
            {
                PlayerController controller = playerObject.GetComponent<PlayerController>();
                if (controller != null)
                {
                    Vector3 startPosition = startLine.position - new Vector3(i * spawnMargin, 0, 0);
                    controller.transform.position = startPosition;
                    controller.CanMove = false; // Disable movement until race starts
                }
            }
        }
    }
}
