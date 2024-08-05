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
        // Start a private session for the player's home lobby
        Launch(GenerateUniqueSessionName(), true, SessionType.Private, 4);
    }

    // Main launcher of all network sessions (Lobby, MainSession)
    public void Launch(string sessionName, bool isInitialStart = false, SessionType sessionType = SessionType.Public, int maxPlayers = 6)
    {
        StartCoroutine(LaunchCoroutine(sessionName, isInitialStart, sessionType, maxPlayers));
    }

    private IEnumerator LaunchCoroutine(string sessionName, bool isInitialStart = false, SessionType sessionType = SessionType.Public, int maxPlayers = 6)
    {
        if (launcher != null)
        {
            Destroy(launcher.gameObject);

            while (launcher != null)
            {
                yield return null;
            }
        }

        Debug.Log($"Launching Fusion: {sessionName} ({sessionType})");
        launcher = Instantiate(LauncherPrefab).GetComponent<FusionLauncher>();
        launcher.name = "[Fusion]" + sessionName;
        launcher.InitializeNetwork(sessionName, isInitialStart, sessionType, maxPlayers);
    }

    public string GenerateUniqueSessionName()
    {
        return System.Guid.NewGuid().ToString();
    }
}

public enum SessionType
{
    Public,
    Private
}
