using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

/// <summary>
/// Last addition was to have the loading screen ( first launch ) wait for the user to inpit his name
/// then go to menu scene
/// test it 
/// see if everythings okay
/// 
/// </summary>
public class GameInitiator : MonoBehaviour
{
    [Header("Core Components")]
    [SerializeField] private LoadingUI _loadingUI;
    [SerializeField] private GameObject _audioManager;
    [SerializeField] private GameObject _notificationManager;
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private NetworkRunner _networkRunner;

    private bool _isPlayerNameSet = false;

    private async void Start()
    {
        DontDestroyOnLoad(gameObject);
        BindObjects();

        _playerData.ResetAllData();
        _loadingUI.SetProgress(0.123f);

        if (SaveSystem.IsFirstLaunch())
        {
            await HandleFirstLaunch();
        }

        _loadingUI.SetLoadingMessage("Initializing");
        _loadingUI.SetProgress(0.29f);
        _loadingUI.SetLoadingMessage("Connecting");
        _loadingUI.SetProgress(0.39f);

        await SetupNetworkConnection();
        _loadingUI.SetProgress(0.79f);

        await UniTask.Delay(500);
        _loadingUI.SetLoadingMessage("Connected");
        _loadingUI.SetProgress(1f);

        _networkManager.LoadScene(SceneType.MenuScene);
    }

    private async UniTask HandleFirstLaunch()
    {
        _loadingUI.StartFirstLaunchSequence();

        // Wait until the player sets their name
        await UniTask.WaitUntil(() => _isPlayerNameSet);

        Debug.Log($"Player name set to: {_playerData.PlayerName}");
    }

    // Called when the continue button is pressed
    public void OnPlayerNameSet(string playerName)
    {
        _playerData.PlayerName = playerName;
        _isPlayerNameSet = true;
    }

    private void BindObjects()
    {
        _networkManager = Instantiate(_networkManager);
        _playerData = Instantiate(_playerData);
        _audioManager = Instantiate(_audioManager);
        _notificationManager = Instantiate(_notificationManager);

        DontDestroyOnLoad(_playerData);
        DontDestroyOnLoad(_audioManager);
        DontDestroyOnLoad(_notificationManager);
        DontDestroyOnLoad(_networkManager);
    }

    private async UniTask SetupNetworkConnection()
    {
        _networkManager.InitiateConnection(_networkRunner);
        string sessionName = _networkManager.GenerateUniqueSession();
        await _networkManager.StartSession(SceneRef.FromIndex(1), sessionName);
    }
}

