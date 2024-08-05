using UnityEngine;
using Fusion;
using System.Threading.Tasks;
using Fusion.Addons.Physics;
using System.Linq;
using System.Collections.Generic;

// Starts any fusion connection
public class FusionLauncher : MonoBehaviour
{
    public static FusionLauncher Instance;

    [SerializeField] private NetworkPrefabRef net_PublicLobbyManagerPrefab;
    [SerializeField] private NetworkPrefabRef net_PrivateLobbyManagerPrefab;
    [SerializeField] private NetworkPrefabRef net_ChatManagerPrefab;
    [SerializeField] private NetworkPrefabRef net_GameManagerPrefab;
    [SerializeField] private NetworkPrefabRef net_PlayerPrefab;
    [SerializeField] private GameObject cameraPrefab;
    [SerializeField] private bool enableLoading = true;

    private NetworkRunner _runner;
    private NetworkInputHandler _inputHandler;

    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Failed,
        Connected,
        Loading,
        Loaded
    }

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

    public NetworkPrefabRef GetGameManagerNetPrefab()
    {
        return net_GameManagerPrefab;
    }

    public NetworkPrefabRef GetPlayerNetPrefab()
    {
        return net_PlayerPrefab;
    }

    public async void InitializeNetwork(string sessionName, bool isInitialStart = false, SessionType sessionType = SessionType.Public, int maxPlayers = 6)
    {
        Debug.Log($"Joining Session: {sessionName}");
        UILoadingScreen.Instance.PlayerUniqueID.text = sessionName;

        if (isInitialStart)
            SetConnectionStatus(ConnectionStatus.Connecting, "Initiating Network");

        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.name = name;
            _runner.ProvideInput = true;

            _inputHandler = FindObjectOfType<NetworkInputHandler>();

            if (_inputHandler != null)
            {
                _runner.AddCallbacks(_inputHandler);
            }

            var startGameArgs = new StartGameArgs()
            {
                GameMode = GameMode.Shared,
                SessionName = sessionName,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                Scene = SceneRef.FromIndex(0),
                PlayerCount = maxPlayers
            };

            if (isInitialStart)
            {
                var result = await StartGameWithProgress(startGameArgs);

                if (result.Ok)
                {
                    SetConnectionStatus(ConnectionStatus.Connected, "Initiated!");
                    await WaitForRunnerToBeReady(sessionType);
                }
                else
                {
                    SetConnectionStatus(ConnectionStatus.Failed, result.ShutdownReason.ToString());
                }
            }
            else
            {
                var result = await _runner.StartGame(startGameArgs);

                if (result.Ok)
                {
                    await WaitForRunnerToBeReady(sessionType);
                }
                else
                {
                    Debug.LogError($"Connection failed: {result.ShutdownReason.ToString()}");
                }
            }
        }
    }

    private async Task WaitForRunnerToBeReady(SessionType sessionType)
    {
        while (!_runner.IsRunning)
        {
            await Task.Yield();
        }

        if (_runner.IsRunning && _runner.IsSharedModeMasterClient)
        {
            if (sessionType == SessionType.Public)
            {
                if (PublicLobbyManager.Instance == null && net_PublicLobbyManagerPrefab != null)
                {
                    _runner.Spawn(net_PublicLobbyManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
                }
            }
            else if (sessionType == SessionType.Private)
            {
                if (PrivateLobbyManager.Instance == null && net_PrivateLobbyManagerPrefab != null)
                {
                    _runner.Spawn(net_PrivateLobbyManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
                    _runner.Spawn(net_PublicLobbyManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
                }

                if (net_ChatManagerPrefab != null)
                {
                    _runner.Spawn(net_ChatManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
                }
            }
        }
    }

    private async Task<StartGameResult> StartGameWithProgress(StartGameArgs startGameArgs)
    {
        if (enableLoading)
        {
            UILoadingScreen.Instance.ShowLoadingScreen("Connecting");
        }

        var result = await _runner.StartGame(startGameArgs);
        if (result.Ok)
        {
            if (enableLoading)
            {
                UILoadingScreen.Instance.SetLoadingMessage("Loading Assets");
                await LoadAssetsWithProgress();
            }

            if (enableLoading)
            {
                UILoadingScreen.Instance.SetLoadingMessage("Finalizing");
                await Task.Delay(500);
            }
        }

        if (enableLoading)
            UILoadingScreen.Instance.HideLoadingScreen();

        return result;
    }

    private async Task LoadAssetsWithProgress()
    {
        float totalSteps = 33f;
        for (int i = 1; i <= totalSteps; i++)
        {
            await Task.Delay(13);
            UILoadingScreen.Instance.SetProgress(i / totalSteps);
        }
    }

    public NetworkRunner GetNetworkRunner()
    {
        return _runner;
    }

    public void SetConnectionStatus(ConnectionStatus status, string message)
    {
        Debug.Log($"Connection status: {status}, message: {message}");

        if (enableLoading)
            UILoadingScreen.Instance.ShowLoadingScreen(message);

        if (status == ConnectionStatus.Connected || (status == ConnectionStatus.Failed && enableLoading))
        {
            UILoadingScreen.Instance.HideLoadingScreen();
        }
    }
}
