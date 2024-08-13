using UnityEngine;
using Fusion;
using Cinemachine;

public class PlayerSpawner : MonoBehaviour
{
    public NetworkPrefabRef playerPrefab;
    public Vector3 spawnPosition = Vector3.zero;
    [SerializeField] private CinemachineVirtualCamera _cam;
    private NetworkRunner _runner;

    private async void Start()
    {
        _runner = FindObjectOfType<NetworkRunner>();

        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;

            var startGameArgs = new StartGameArgs()
            {
                GameMode = GameMode.Shared,
                SessionName = "TestSession",
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            };

            await _runner.StartGame(startGameArgs);
        }

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (_runner != null && playerPrefab != null)
        {
            // Spawn the player and then attach the camera immediately after
            var playerObject = _runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, _runner.LocalPlayer);
            if (playerObject != null)
            {
                AttachCamera(playerObject.gameObject);
                Debug.Log("Player spawned in the network and camera attached.");
            }
            else
            {
                Debug.LogError("Failed to spawn player.");
            }
        }
        else
        {
            Debug.LogError("NetworkRunner or PlayerPrefab is not set.");
        }
    }

    private void AttachCamera(GameObject player)
    {
        var cameraInstance = Instantiate(_cam);
        cameraInstance.Follow = player.transform;
        Debug.Log("Camera attached to the player.");
    }
}
