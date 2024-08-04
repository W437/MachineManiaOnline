using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

// This is the engine of the game. All initiations, carburetor, pistons, network, starts here.
public class GameLauncher : MonoBehaviour
{
    public static GameLauncher Instance;
    [SerializeField] private GameObject LauncherPrefab;
    private FusionLauncher launcher;

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

    private void Start()
    {
        // Starts a unique session for each player, their own 'lobby'.
        Launch(GenerateUniqueSessionName(), true, 4);
    }

    // Main launcher of all network sessions (Lobby, MainSession)
    public void Launch(string sessionName, bool isInitialStart = false, int maxPlayers = 6)
    {
        StartCoroutine(LaunchCoroutine(sessionName, isInitialStart, maxPlayers));
    }

    private IEnumerator LaunchCoroutine(string sessionName, bool isInitialStart = false, int maxPlayers = 6)
    {
        if (launcher != null)
        {
            Destroy(launcher.gameObject);

            while (launcher != null)
            {
                yield return null;
            }
        }

        Debug.Log($"Launching Fusion: {sessionName}");
        launcher = Instantiate(LauncherPrefab).GetComponent<FusionLauncher>();
        launcher.name = "[Fusion]" + sessionName;
        launcher.InitializeNetwork(sessionName, isInitialStart, maxPlayers);
    }

    public string GenerateUniqueSessionName()
    {
        return System.Guid.NewGuid().ToString();
    }
}
