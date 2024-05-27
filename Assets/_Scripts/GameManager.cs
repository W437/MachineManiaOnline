using UnityEngine;
using Fusion;
using static Unity.Collections.Unicode;

public class GameManager : MonoBehaviour
{
    [SerializeField] private NetworkRunner networkRunnerPrefab;
    [SerializeField] private GameObject playerPrefab;

    private NetworkRunner runner;

    void Start()
    {
        StartGame();
    }

    async void StartGame()
    {
        runner = Instantiate(networkRunnerPrefab);
        runner.ProvideInput = false;
        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "TestRoom",
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        // Spawn the player
        runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, runner.LocalPlayer);
    }
}
