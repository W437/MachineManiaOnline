using UnityEngine;
using Fusion;

public class PlayerSpawner : MonoBehaviour
{
    public NetworkPrefabRef playerPrefab; // Assign your player prefab here
    public Vector3 spawnPosition = Vector3.zero; // Default spawn position, can be set in the inspector

    private NetworkRunner _runner;

    private async void Start()
    {
        // Attempt to find an existing NetworkRunner in the scene
        _runner = FindObjectOfType<NetworkRunner>();

        if (_runner == null)
        {
            // If no NetworkRunner is found, create and start a new one
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;

            var startGameArgs = new StartGameArgs()
            {
                GameMode = GameMode.Single,
                SessionName = "TestSession",
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            };

            await _runner.StartGame(startGameArgs);
        }

        // Spawn the player prefab into the network
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (_runner != null && playerPrefab != null)
        {
            // Spawning the player prefab at the specified position
            _runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, _runner.LocalPlayer);
            Debug.Log("Player spawned in the network for testing.");
        }
        else
        {
            Debug.LogError("NetworkRunner or PlayerPrefab is not set.");
        }
    }
}
