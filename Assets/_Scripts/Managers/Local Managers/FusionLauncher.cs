using UnityEngine;
using Fusion;
using System.Threading.Tasks;
using Fusion.Addons.Physics;
using System.Linq;
using System.Collections.Generic;
using Cinemachine;

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

    private NetworkRunner _runner;
    private NetInputHandler _inputHandler;

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

    public GameObject GetCameraPrefab()
    {
        return cameraPrefab;
    }

    public async void InitializeNetwork(string sessionName, bool isInitialStart = false, SessionType sessionType = SessionType.Public, int maxPlayers = 6)
    {
        Debug.Log($"Joining Session: {sessionName}");
        LoadingUI.Instance.PlayerUniqueID.text = sessionName;

        if (isInitialStart)
            LoadingUI.Instance.SetConnectionStatus(ConnectionStatus.Connecting, "Initiating Network");

        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.name = name;
            _runner.ProvideInput = true;

            _inputHandler = FindObjectOfType<NetInputHandler>();

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
                    LoadingUI.Instance.SetConnectionStatus(ConnectionStatus.Connected, "Initiated!");
                    await WaitForRunnerToBeReady(sessionType);
                }
                else
                {
                    LoadingUI.Instance.SetConnectionStatus(ConnectionStatus.Failed, result.ShutdownReason.ToString());
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

        switch (sessionType)
        {
            case SessionType.Public:

                if (PublicLobbyManager.Instance == null && net_PublicLobbyManagerPrefab != null)
                    _runner.Spawn(net_PublicLobbyManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);

            break;

            case SessionType.Private:

                if (PrivateLobbyManager.Instance == null && net_PrivateLobbyManagerPrefab != null)
                    _runner.Spawn(net_PrivateLobbyManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);

                if (net_ChatManagerPrefab != null)
                    _runner.Spawn(net_ChatManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);

            break;

            default:
            break;
        }
    }

    private async Task<StartGameResult> StartGameWithProgress(StartGameArgs startGameArgs)
    {
        bool _loadingActive = GameLauncher.LoadingScreenActive;

        if (_loadingActive)
        {
            LoadingUI.Instance.ShowLoadingScreen("Connecting");
        }

        var result = await _runner.StartGame(startGameArgs);

        if (result.Ok)
        {
            if (_loadingActive)
            {
                LoadingUI.Instance.SetLoadingMessage("Loading Assets");
                await LoadAssetsWithProgress();
                LoadingUI.Instance.SetLoadingMessage("Finalizing");
                await Task.Delay(500);
            }
        }

        if (GameLauncher.LoadingScreenActive)
            LoadingUI.Instance.HideLoadingScreen();

        return result;
    }

    private async Task LoadAssetsWithProgress()
    {
        float totalSteps = 33f;
        for (int i = 1; i <= totalSteps; i++)
        {
            await Task.Delay(13);
            LoadingUI.Instance.SetProgress(i / totalSteps);
        }
    }

    public NetworkRunner Runner()
    {
        return _runner;
    }
}
