using UnityEngine;
using Fusion;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    private NetworkRunner networkRunnerInstance;
    private Dictionary<SceneType, string> sceneNameMap;

    public event Action<string> OnSessionChanged;

    [SerializeField] private NetworkPrefabRef privateLobbyManagerPrefab;
    [SerializeField] private NetworkPrefabRef publicLobbyManagerPrefab;
    [SerializeField] private NetworkPrefabRef gameManagerPrefab;

    private GameObject persistentGameManager;

    public void InitiateConnection(NetworkRunner runnerPrefab)
    {
        if (networkRunnerInstance == null)
        {
            networkRunnerInstance = Instantiate(runnerPrefab);
            DontDestroyOnLoad(networkRunnerInstance.gameObject);
            networkRunnerInstance.ProvideInput = true;
        }
    }

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

        BindScenes();
    }

    public async Task StartSession(SceneRef scene, string sessionName, int maxPlayers = 6)
    {
        if (networkRunnerInstance == null)
            return;

        if (networkRunnerInstance.IsRunning)
        {
            await networkRunnerInstance.Shutdown();
            Destroy(networkRunnerInstance.gameObject);
            networkRunnerInstance = null;
            networkRunnerInstance = Instantiate(networkRunnerInstance);
            DontDestroyOnLoad(networkRunnerInstance.gameObject);
        }

        var startGameArgs = new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            Scene = scene,
            SceneManager = networkRunnerInstance.GetComponent<NetworkSceneManagerDefault>(),
            PlayerCount = maxPlayers
        };

        var result = await networkRunnerInstance.StartGame(startGameArgs);

        OnSessionChanged?.Invoke(sessionName);

        if (!result.Ok)
            Debug.LogError($"Failed to start session: {result.ShutdownReason}");
    }

    public async void LoadScene(SceneType scene)
    {
        if (sceneNameMap.TryGetValue(scene, out string sceneName))
        {
            // Load the scene using Fusion's LoadScene
            await networkRunnerInstance.LoadScene(sceneName, LoadSceneMode.Single);

            // Spawn the appropriate manager based on the loaded scene
            SpawnSceneManager(scene);
        }
    }

    private void SpawnSceneManager(SceneType scene)
    {
        // Only the master client is allowed to spawn managers
        if (!networkRunnerInstance.IsSharedModeMasterClient)
            return;

        // Destroy the persistent GameManager if leaving game-related scenes
        if (scene != SceneType.GameScene && scene != SceneType.LevelEndScene && persistentGameManager != null)
        {
            Destroy(persistentGameManager);
            persistentGameManager = null;
        }

        // Spawn the appropriate manager
        switch (scene)
        {
            case SceneType.MenuScene:
                if (privateLobbyManagerPrefab != null)
                {
                    networkRunnerInstance.Spawn(privateLobbyManagerPrefab);
                }
                break;

            case SceneType.PublicLobbyScene:
                if (publicLobbyManagerPrefab != null)
                {
                    networkRunnerInstance.Spawn(publicLobbyManagerPrefab);
                }
                break;

            case SceneType.GameScene:
                if (gameManagerPrefab != null)
                {
                    if (persistentGameManager == null)
                    {
                        persistentGameManager = networkRunnerInstance.Spawn(gameManagerPrefab).gameObject;
                    }
                }
                break;

            case SceneType.LevelEndScene:
                // GameManager persists across game and level-end scenes
                break;
        }
    }

    public string GenerateUniqueSession()
    {
        return Guid.NewGuid().ToString();
    }

    public NetworkRunner Runner()
    {
        return networkRunnerInstance;
    }

    private void BindScenes()
    {
        sceneNameMap = new Dictionary<SceneType, string>()
        {
            { SceneType.LaunchScene, "Launch" },
            { SceneType.MenuScene, "Main-Menu" },
            { SceneType.GameScene, "Game" },
            { SceneType.PublicLobbyScene, "Public-Lobby" },
            { SceneType.LevelEndScene, "Level-End" }
        };
    }
}

public enum SceneType
{
    LaunchScene,
    MenuScene,
    GameScene,
    PublicLobbyScene,
    LevelEndScene
}
