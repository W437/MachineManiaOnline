using UnityEngine;
using Fusion;
using Cinemachine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    public NetworkPrefabRef playerPrefab;
    public Vector3 spawnPosition = Vector3.zero;
    [SerializeField] CinemachineVirtualCamera _cam;
    NetworkRunner _runner;

    async void Start()
    {
        _runner = FindObjectOfType<NetworkRunner>();

        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));

        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;

            var startGameArgs = new StartGameArgs()
            {
                GameMode = GameMode.Shared,
                SessionName = "TestSession",
                Scene = sceneInfo
            };

            await _runner.StartGame(startGameArgs);
        }

        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        if (_runner != null && playerPrefab != null)
        {
            var playerObject = _runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, _runner.LocalPlayer);
            
            if (playerObject != null)
            {
                AttachCamera(playerObject.gameObject);
                _runner.SetPlayerObject(_runner.LocalPlayer, playerObject);
            }
        }
    }

    void AttachCamera(GameObject player)
    {
        var cameraInstance = Instantiate(_cam);
        cameraInstance.Follow = player.transform;
    }
}
