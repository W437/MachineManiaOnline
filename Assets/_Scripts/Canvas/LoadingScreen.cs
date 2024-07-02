using Coffee.UIEffects;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private GameObject loadingPanel;
    private UITransitionEffect imageTransitionEffect;

    private Coroutine loadingDotsCoroutine;
    private string currentLoadingMessage;
    

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

        imageTransitionEffect = loadingPanel.GetComponent<UITransitionEffect>();
    }

    public void ShowLoadingScreen(string message = "Connecting")
    {
        ServiceLocator.GetAudioManager().SetCutoffFrequency(22000, 1000, 0.1f);
        loadingScreen.SetActive(true);
        currentLoadingMessage = message;
        loadingText.text = currentLoadingMessage;
        loadingBar.value = 0f;

        if (loadingDotsCoroutine == null)
        {
            loadingDotsCoroutine = StartCoroutine(AnimateLoadingDots());
        }
    }

    public void SetProgress(float progress)
    {
        loadingBar.value = progress;
    }

    public void SetLoadingMessage(string message)
    {
        currentLoadingMessage = message;
        loadingText.text = currentLoadingMessage;
    }

    public void HideLoadingScreen()
    {
        LeanTween.value(1, 0, 0.5f).setOnUpdate(value => imageTransitionEffect.effectFactor = value)
            .setOnComplete(() => { loadingScreen.SetActive(false); });

        if (loadingDotsCoroutine != null)
        {
            StopCoroutine(loadingDotsCoroutine);
            loadingDotsCoroutine = null;
        }

        ServiceLocator.GetAudioManager().SetCutoffFrequency(1000, 5000);
    }

    private IEnumerator AnimateLoadingDots()
    {
        int dotCount = 0;

        while (true)
        {
            loadingText.text = currentLoadingMessage + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
