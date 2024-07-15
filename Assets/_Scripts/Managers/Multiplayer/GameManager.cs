using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
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

    [Networked] private TickTimer gameStartTimer { get; set; }
    [Networked] private TickTimer raceCountdownTimer { get; set; }
    [Networked] private TickTimer postRaceCountdownTimer { get; set; }
    [Networked] private int gameState { get; set; } // 0: Waiting, 1: Countdown, 2: Racing, 3: Finished
    [Networked] public float FinishTime { get; set; }
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
            InitializeGame();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void Spawned()
    {
        if (Runner.IsSharedModeMasterClient || HasStateAuthority)
        {
            gameStartTimer = TickTimer.CreateFromSeconds(Runner, 20); // 20 seconds countdown to game start
            gameState = 0;
        }
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
            case 0: // Waiting for players to ready up
                if (gameStartTimer.Expired(Runner))
                {
                    StartGameCountdown();
                }
                break;

            case 1: // Countdown before race start
                if (raceCountdownTimer.Expired(Runner))
                {
                    StartRace();
                }
                break;

            case 2: // Race in progress
                if (!isRaceStarted)
                {
                    isRaceStarted = true;
                    raceDuration = Time.time;
                }
                //UpdatePlayerRankings();
                break;

            case 3: // Race finished
                if (postRaceCountdownTimer.Expired(Runner))
                {
                    EndGame();
                }
                break;
        }
    }

    private void StartGameCountdown()
    {
        raceCountdownTimer = TickTimer.CreateFromSeconds(Runner, 3); // 3 seconds countdown
        gameState = 1;
        RpcGameCountdown();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcGameCountdown()
    {
        // Display countdown UI on all clients
    }

    private void StartRace()
    {
        gameState = 2;
        RpcStartRace();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcStartRace()
    {
        // Start race UI on all clients
    }

    public void PlayerFinished(PlayerRef player)
    {
        if (!playerControllers.ContainsKey(player)) return;

        //playerControllers[player].FinishTime = Time.time - raceDuration;
        //RpcPlayerFinished(playerControllers[player].FinishTime);

        if (playerControllers.Count == players.Count) // All players finished
        {
            postRaceCountdownTimer = TickTimer.CreateFromSeconds(Runner, 25); // 25 seconds countdown for post-race
            gameState = 3;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcPlayerFinished(float finishTime)
    {
        // Update finish time UI for all players
    }

    private void EndGame()
    {
        gameState = 0;
        RpcEndGame();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcEndGame()
    {
        // Display end game stats and return to lobby
    }

/*    private void UpdatePlayerRankings()
    {
        players.Sort((a, b) => Vector3.Distance(playerControllers[a].transform.position, finishLine.position)
                             .CompareTo(Vector3.Distance(playerControllers[b].transform.position, finishLine.position)));

        //RpcUpdatePlayerRankings(players);
    }*/

    // sortedPlayers not acceptable in RPC calls?!
/*
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcUpdatePlayerRankings(NetworkLinkedList<PlayerRef> sortedPlayers)
    {
        // Update player ranking UI
    }*/

    public enum GameMode
    {
        FFA,    // Free For All
        TVT,    // Team vs Team
        Custom  // PvP or custom setting
    }

    public GameMode CurrentGameMode { get; private set; }

    public readonly GameMode[] gameModes =
    {
        GameMode.FFA,
        GameMode.TVT,
        GameMode.Custom
    };

    public void SetGameMode(GameMode mode)
    {
        CurrentGameMode = mode;
    }

    private void InitializeGame()
    {
        SetGameMode(GameMode.FFA);
    }

}
