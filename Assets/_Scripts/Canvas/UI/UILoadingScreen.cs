using Coffee.UIEffects;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILoadingScreen : MonoBehaviour
{
    public static UILoadingScreen Instance { get; private set; }

    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private GameObject loadingPanel;

    private UITransitionEffect _imageTransitionEffect;
    private Coroutine _loadingDotsCoroutine;
    private string _currentLoadingMessage;
    

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

        _imageTransitionEffect = loadingPanel.GetComponent<UITransitionEffect>();
    }

    public void ShowLoadingScreen(string message = "Connecting")
    {
        AudioManager.Instance.SetCutoffFrequency(22000, 1000, 0.1f);
        loadingScreen.SetActive(true);

        _currentLoadingMessage = message;
        loadingText.text = _currentLoadingMessage;
        loadingBar.value = 0f;

        _loadingDotsCoroutine ??= StartCoroutine(AnimateLoadingDots());
    }

    public void SetProgress(float progress)
    {
        loadingBar.value = progress;
    }

    public void SetLoadingMessage(string message)
    {
        _currentLoadingMessage = message;
        loadingText.text = _currentLoadingMessage;
    }

    public void HideLoadingScreen()
    {
        LeanTween.value(1, 0, 0.5f).setOnUpdate(value => _imageTransitionEffect.effectFactor = value)
            .setOnComplete(() => { loadingScreen.SetActive(false); });

        if (_loadingDotsCoroutine != null)
        {
            StopCoroutine(_loadingDotsCoroutine);
            _loadingDotsCoroutine = null;
        }

        AudioManager.Instance.SetCutoffFrequency(1000, 5000);
    }

    private IEnumerator AnimateLoadingDots()
    {
        int _dotCount = 0;

        while (true)
        {
            loadingText.text = _currentLoadingMessage + new string('.', _dotCount);
            _dotCount = (_dotCount + 1) % 4;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
