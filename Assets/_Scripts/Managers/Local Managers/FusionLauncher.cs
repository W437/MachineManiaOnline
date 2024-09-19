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
    [SerializeField] NetworkPrefabRef net_ChatManagerPrefab;
    [SerializeField] NetworkPrefabRef net_GameManagerPrefab;
    [SerializeField] NetworkPrefabRef net_PrivateLobbyPrefab;
    [SerializeField] NetworkPrefabRef net_PlayerPrefab;
    [SerializeField] GameObject cameraPrefab;

    NetworkRunner _runner;
    //NetInputHandler _inputHandler;

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

                    var privateLobby = Runner().Spawn(net_PrivateLobbyPrefab, Vector3.zero, Quaternion.identity, Runner().LocalPlayer);
                    var chatManager = Runner().Spawn(net_ChatManagerPrefab, Vector3.zero, Quaternion.identity, Runner().LocalPlayer);

                    //Runner().SetPlayerObject(Runner().LocalPlayer, privateLobby);
                    //Runner().SetPlayerObject(Runner().LocalPlayer, chatManager);

                    // await WaitForRunnerToBeReadyAsync(sessionType);
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
                    if(SessionType.Private == sessionType)
                    {
                        Debug.Log("Starting chat manager...");
                        Runner().Spawn(net_PrivateLobbyPrefab, Vector3.zero, Quaternion.identity, Runner().LocalPlayer);
                        Runner().Spawn(net_ChatManagerPrefab, Vector3.zero, Quaternion.identity, Runner().LocalPlayer);
                    }
//                    await WaitForRunnerToBeReadyAsync(sessionType);
                }
                else
                {
                    Debug.LogError($"Connection failed: {result.ShutdownReason.ToString()}");
                }
            }
        }
    }

    // what the heck is this?
    private async Task WaitForRunnerToBeReadyAsync(SessionType sessionType)
    {
        while (!_runner.IsRunning)
        {
            await Task.Yield();
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
                await Task.Delay(200);
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
            await Task.Delay(5);
            LoadingUI.Instance.SetProgress(i / totalSteps);
        }
    }

    public NetworkRunner Runner()
    {
        return _runner;
    }

    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Failed,
        Connected,
        Loading,
        Loaded
    }
}
