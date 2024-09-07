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

    public async Task InitializeNetworkAsync(string sessionName, bool isInitialStart = false, SessionType sessionType = SessionType.Public, int maxPlayers = 6)
    {
        LoadingUI.Instance.PlayerUniqueID.text = sessionName;

        if (isInitialStart)
        {
            LoadingUI.Instance.SetConnectionStatus(ConnectionStatus.Connecting, "Connecting to network");
        }

        if (_runner != null)
        {
            Debug.Log("Shutting down current session...");
            await _runner.Shutdown();
            Destroy(_runner.gameObject);
            _runner = null; 
        }

        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.name = name;
            _runner.ProvideInput = true;

            var inputHandler = FindObjectOfType<NetInputHandler>();

            if (inputHandler != null)
            {
                _runner.AddCallbacks(inputHandler);
            }

            var startGameArgs = new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = sessionName,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                Scene = SceneRef.FromIndex(0),
                PlayerCount = maxPlayers
            };

            if (isInitialStart)
            {
                var result = await StartGameWithProgressAsync(startGameArgs);

                if (result.Ok)
                {
                    LoadingUI.Instance.SetConnectionStatus(ConnectionStatus.Connected, "Connected to network");
                    await WaitForRunnerToBeReadyAsync(sessionType);
                }
                else
                {
                    LoadingUI.Instance.SetConnectionStatus(ConnectionStatus.Failed, result.ShutdownReason.ToString());
                }
            }
            else
            {
                Debug.Log("Starting game normally...");
                var result = await _runner.StartGame(startGameArgs);

                if (result.Ok)
                {
                    await WaitForRunnerToBeReadyAsync(sessionType);
                }
                else
                {
                    Debug.LogError($"Connection failed: {result.ShutdownReason.ToString()}");
                }
            }
        }
    }

    private async Task WaitForRunnerToBeReadyAsync(SessionType sessionType)
    {
        while (!_runner.IsRunning)
        {
            await Task.Yield();
        }


        // REVERTED BACK TO SPAWNING MANAGER ON ALL CLIENTS FOR EASE OF USE
        if (sessionType == SessionType.Public && PublicLobbyManager.Instance == null && net_PublicLobbyManagerPrefab != null)
        {
            //if(_runner.IsSharedModeMasterClient)
                _runner.Spawn(net_PublicLobbyManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
        }
        else if (sessionType == SessionType.Private && PrivateLobbyManager.Instance == null && net_PrivateLobbyManagerPrefab != null)
        {
                _runner.Spawn(net_PrivateLobbyManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
                _runner.Spawn(net_ChatManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
        }
    }

    private async Task<StartGameResult> StartGameWithProgressAsync(StartGameArgs startGameArgs)
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
