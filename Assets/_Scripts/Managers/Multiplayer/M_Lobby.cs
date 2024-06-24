using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Cinemachine;


// Current:
// Figure Simulation on PlayerManager
// Move logic to LobbyManager
// 

// NetworkManager handles initial connections to the internet, sessions, and games.
// All callbacks happen here (player joins, leave, scene load, etc..)
public class M_Lobby : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Button readyButton;
    [SerializeField] private TextMeshProUGUI playerListText;
    [SerializeField] private SceneRef gameScene;
    private List<string> playerNames = new List<string>();
    private Dictionary<int, bool> playerReadyStates = new Dictionary<int, bool>();
    private M_Player M_Player;
    public NetworkRunner GetNetworkRunner() { return ServiceLocator.GetNetworkManager().GetNetworkRunner(); }

    private void Awake()
    {
        ServiceLocator.RegisterLobbyManager(this);
        readyButton.onClick.AddListener(OnReadyButtonClicked);
    }

    private void Start()
    {
        StartCoroutine(WaitForPlayerManager());
    }

    private IEnumerator WaitForPlayerManager()
    {
        while (M_Player == null)
        {
            M_Player = ServiceLocator.GetPlayerManager();
            yield return null;
        }

        UpdatePlayerList();
    }

    public void ShowLoadingScreen()
    {
        loadingScreen.SetActive(true);
        StartSession();
        UpdatePlayerList();
    }

    public async void StartSession()
    {
        var networkManager = ServiceLocator.GetNetworkManager();

        if (networkManager == null)
        {
            Debug.LogError("NetworkManager is not initialized.");
            return;
        }

        var runner = networkManager.GetNetworkRunner();

        if (runner == null)
        {
            Debug.LogError("NetworkRunner is not initialized.");
            return;
        }

        if (runner.IsRunning)
        {
            Debug.LogError("NetworkRunner is already running. Cannot start another session.");
            return;
        }

        var config = new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = "RaceSession",
        };

        var result = await runner.StartGame(config);

        if (result.Ok)
        {
            Debug.Log("Session started successfully.");
            UpdatePlayerList();
        }
        else
        {
            Debug.LogError("Failed to start session: " + result.ShutdownReason);
            Debug.LogError("Detailed Error: " + result.ErrorMessage);
        }
    }

    public void AddPlayer(int playerIndex, string playerName)
    {
        if (M_Player != null && !playerReadyStates.ContainsKey(playerIndex))
        {
            playerReadyStates.Add(playerIndex, false);
        }
        UpdatePlayerList();
    }

    public void RemovePlayer(int playerIndex)
    {
        if (M_Player != null && M_Player.ContainsKey(playerIndex))
        {
            M_Player.RemoveReady(playerIndex);
        }
        UpdatePlayerList();
    }

    public void UpdatePlayerList()
    {
        if (M_Player == null)
        {
            Debug.LogWarning("Player manager is not ready yet.");
            return;
        }

        playerListText.text = "Players:\n";
        foreach (var playerState in M_Player.playerReadyStates)
        {
            playerListText.text += $"Players {playerState.Key}" + (playerState.Value ? " (Ready)\n" : " (Not Ready)\n");
        }
        Debug.Log("Updated players list");
    }

    private void OnReadyButtonClicked()
    {
        /*        if (!GetNetworkRunner().LocalPlayer.IsNone)
                {
                    int localPlayerIndex = GetNetworkRunner().LocalPlayer.AsIndex;
                    SetPlayerReadyState(localPlayerIndex, true);
                    readyButton.interactable = false;
                }
                else
                {
                    Debug.LogError("Local player is not valid in LobbyManager.OnReadyButtonClicked");
                }*/
        StartGame();
    }

    public void SetPlayerReadyState(int playerIndex, bool isReady)
    {
        if (M_Player == null)
        {
            Debug.LogWarning("Player manager is not ready yet.");
            return;
        }

        if (!M_Player.ContainsKey(playerIndex))
        {
            M_Player.SetReady(playerIndex, false);
        }

        M_Player.SetReady(playerIndex, isReady);
        UpdatePlayerList();
        CheckAllPlayersReady();
    }

    private void CheckAllPlayersReady()
    {
        foreach (var readyState in M_Player.playerReadyStates)
        {
            if (!readyState.Value) return;
        }

        InitiateGame();
    }

    public void InitiateGame()
    {
        StartGame();
    }

    public void StartGame()
    {
        if (GetNetworkRunner().IsRunning)
        {
            GetNetworkRunner().LoadScene(gameScene, LoadSceneMode.Single);
        }
    }

}