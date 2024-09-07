using System.Collections;
using UnityEngine;
using Fusion;

// This is the engine of the game. All initiations, carburetor, pistons, network, starts here.
public class GameLauncher : MonoBehaviour
{
    public static GameLauncher Instance;
    [SerializeField] private GameObject LauncherPrefab;
    FusionLauncher launcher;

    public static bool LoadingScreenActive { get; set; }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        LeanTween.init(2000);

        LoadingScreenActive = true;
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
        StartGameSession();
    }

    private void StartGameSession()
    {
        // Start a private session for the player's home lobby
        Launch(GenerateUniqueSessionName(), true, SessionType.Private, 4);
    }

    public void Launch(string sessionName, bool isInitialStart = false, SessionType sessionType = SessionType.Public, int maxPlayers = 6)
    {
        StartCoroutine(LaunchCoroutine(sessionName, isInitialStart, sessionType, maxPlayers));
    }

    private IEnumerator LaunchCoroutine(string sessionName, bool isInitialStart = false, SessionType sessionType = SessionType.Public, int maxPlayers = 6)
    {
        if (launcher != null)
        {
            Destroy(launcher.gameObject);
            Debug.Log("Destroying current session..");

            while (launcher != null)
            {
                yield return null;
            }
        }

        launcher = Instantiate(LauncherPrefab).GetComponent<FusionLauncher>();
        launcher.name = "[Session]" + sessionName;
        launcher.InitializeNetworkAsync(sessionName, isInitialStart, sessionType, maxPlayers);

        if (HomeUI.Instance != null)
            HomeUI.Instance.SetSessionNameUI(sessionName);
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
