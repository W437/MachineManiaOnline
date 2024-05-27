using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button resetDataButton, haxGameButton;


    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Settings Panel")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Button settingsBackButton;

    [Header("Credits Panel")]
    [SerializeField] private Button creditsBackButton;


    private void Start()
    {
        // Saved values
        InitializeSliders();

        // Setup buttons and effects
        SetupButtonListeners();
        SetupButtonEffects();

        // Other panels
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    void OnDestroy()
    {

    }

    private void InitializeSliders()
    {

    }

    private void SetupButtonListeners()
    {

    }

    private void UpdateResourceDisplay()
    {

    }




    private void TweenResourceValue(TextMeshProUGUI resourceText, int targetValue)
    {
        int currentValue = int.TryParse(resourceText.text, out currentValue) ? currentValue : 0;

        // This will animate the value change over 1.5 seconds.
        LeanTween.value(gameObject, currentValue, targetValue, 1.5f)
            .setOnUpdate((float value) => {
                resourceText.text = Mathf.FloorToInt(value).ToString();
            })
            .setEase(LeanTweenType.easeInOutQuad);
    }



    private void SetupButtonEffects()
    {

    }

    public void StartGame()
    {

    }

    private void AddButtonEffects(Button button, Color hoverColor)
    {

    }

    private void ChangeButtonAlpha(Button button, float alpha)
    {
        Color color = button.image.color;
        color.a = alpha;
        button.image.color = color;
    }

    public void OpenSettings()
    {
        TogglePanel(settingsPanel, true);
    }

    public void OpenCredits()
    {
        TogglePanel(creditsPanel, true);
    }

    private void TogglePanel(GameObject panel, bool isActive)
    {
        // Duration settings
        float closeDuration = 0.3f; // Faster close
        float openDuration = 0.5f; // Slower open

        // Cancel any ongoing animations (main and target panel)
        LeanTween.cancel(mainPanel);
        LeanTween.cancel(panel);

        if (isActive)
        {
            // First, shrink the main panel
            LeanTween.scale(mainPanel, Vector3.zero, closeDuration)
                .setEaseInBack()
                .setOnComplete(() =>
                {
                    mainPanel.SetActive(false);

                    // Then, open the subpanel after the main panel is hidden
                    panel.SetActive(true);
                    panel.transform.localScale = Vector3.zero; // Start scaled down
                    LeanTween.scale(panel, Vector3.one, openDuration)
                        .setEaseOutBack(); // Animate to full size
                });
        }
        else
        {
            // shrink the subpanel
            LeanTween.scale(panel, Vector3.zero, closeDuration)
                .setEaseInBack()
                .setOnComplete(() =>
                {
                    panel.SetActive(false);

                    // Then, expand..
                    mainPanel.SetActive(true);
                    mainPanel.transform.localScale = Vector3.zero; // Start scaled down
                    LeanTween.scale(mainPanel, Vector3.one, openDuration)
                        .setEaseOutBack(); // Animate to full size
                });
        }

        // Reset colors
        //ResetButtonColors();
    }

    private void ResetButtonColors()
    {
        playButton.image.color = Color.black;
        settingsButton.image.color = Color.black;
        creditsButton.image.color = Color.black;
        ChangeButtonAlpha(creditsBackButton, 0.5f);
        ChangeButtonAlpha(settingsBackButton, 0.5f);
    }

    private void OnHoverEnter(Button button, Color color)
    {
        if (button == settingsBackButton)
        {
            Color _settingsBackBtnColor;
            ColorUtility.TryParseHtmlString("#900811DB", out _settingsBackBtnColor);
            button.image.color = _settingsBackBtnColor;
        }
        else
        {
            button.image.color = color;
        }

        LeanTween.scale(button.gameObject, new Vector3(1.05f, 1.05f, 1.05f), 0.15f).setEase(LeanTweenType.easeOutQuad);
    }

    private void OnHoverExit(Button button)
    {
        // default button colors 
        if (button == creditsBackButton)
        {
            Color _creditsBackBtnColor;
            ColorUtility.TryParseHtmlString("#20202080", out _creditsBackBtnColor);
            button.image.color = _creditsBackBtnColor;
        }
        if(button == settingsBackButton)
        {
            Color _settingsBackBtnColor;
            ColorUtility.TryParseHtmlString("#90081180", out _settingsBackBtnColor);
            button.image.color = _settingsBackBtnColor;
        }
        else
        {
            button.image.color = Color.black;
            ChangeButtonAlpha(button, 0.5f);
        }

        LeanTween.scale(button.gameObject, Vector3.one, 0.15f).setEase(LeanTweenType.easeInQuad);
        if (button.gameObject.CompareTag("Back Button")) return; // Ignore back button hover sfx. 
    }
}


