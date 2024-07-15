using UnityEngine;
using Fusion;
using System.Threading.Tasks;
using Fusion.Addons.Physics;
using System.Linq;

// Starts any fusion connection
public class FusionLauncher : MonoBehaviour
{
    public static FusionLauncher Instance;

    private NetworkRunner _runner;
    private NetworkInputHandler _inputHandler;
    public NetworkPrefabRef lobbyManagerPrefab;
    public NetworkPrefabRef chatManagerPrefab;
    public NetworkPrefabRef gameManagerPrefab;
    public GameObject cameraPrefab;
    public bool enableLoading = true;

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

    public async void InitializeNetwork(string sessionName, bool isInitialStart)
    {
        Debug.Log($"Joining Session: {sessionName}");

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
                Scene = SceneRef.FromIndex(0) 
            };

            if (isInitialStart)
            {
                var result = await StartGameWithProgress(startGameArgs);

                if (result.Ok)
                {
                    SetConnectionStatus(ConnectionStatus.Connected, "Initiated!");
                    await WaitForRunnerToBeReady();
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
                    await WaitForRunnerToBeReady();
                }
                else
                {
                    Debug.LogError($"Connection failed: {result.ShutdownReason.ToString()}");
                }
            }
        }
    }


    private async Task WaitForRunnerToBeReady()
    {
        while (!_runner.IsRunning)
        {
            await Task.Yield();
        }

        if (_runner.IsRunning && LobbyManager.Instance == null && lobbyManagerPrefab != null && _runner.IsSharedModeMasterClient)
        {
            _runner.Spawn(lobbyManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
            _runner.Spawn(chatManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
            _runner.Spawn(gameManagerPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
        }

/*        playerObject = _runner.Spawn(playerPrefab, new Vector3(0, 2000f, 5f), Quaternion.identity, _runner.LocalPlayer);
        _runner.SetPlayerObject(_runner.LocalPlayer, playerObject);

        DisableMenuComponents(playerObject);*/
        //playerObject.transform.position += new Vector3(0, 0, 5f);
        //playerObject.transform.localScale = new Vector3(.8f, .8f, .8f);
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
