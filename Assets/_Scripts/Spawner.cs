using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : SimulationBehaviour
{
    [SerializeField] NetworkRunner RunnerPrefab;
    private NetworkRunner _runnerInstance;

    // Start is called before the first frame update
    void Start()
    {
        StartGame();

        //Runner.Spawn(GameManager);
    }

    public async void StartGame()
    {
        _runnerInstance = Instantiate(RunnerPrefab);

        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));

        var startArguments = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "Sess1",
            Scene = sceneInfo,
        };

        var startTask = _runnerInstance.StartGame(startArguments);
        await startTask;

        if (startTask.Result.Ok)
        {
            //Debug.Log("Started game session");
        }
        else
        {
            //Debug.Log("Failed to start game");
        }
    }
}
