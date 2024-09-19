using Coffee.UIExtensions;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance;

    [Header("Settings Panel")]
    [SerializeField] Button panelExitBtn;
    [SerializeField] Button panelBackBtn;
    [SerializeField] Button switchButton1;
    [SerializeField] Button switchButton2;
    [SerializeField] Button switchButton3;
    [SerializeField] Button changeNameBtn;
    [SerializeField] Button resetDataBtn;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    [Header("Change Name Panel")]
    [SerializeField] Button changeNameConfirmBtn;
    [SerializeField] Button changeNamePanelExitBtn;
    [SerializeField] GameObject changeNamePanel;
    [SerializeField] TMP_Text changeNameCostText;
    [SerializeField] GameObject panelOverlayBG;
    [SerializeField] TMP_InputField nameInput;

    [Header("Reset Data Panel")]
    [SerializeField] GameObject resetPanel;
    [SerializeField] Button resetDataConfirmBtn;
    [SerializeField] Button closeResetBtn;
    [SerializeField] Button closeResetBtnWindow;

    ButtonHandler buttonHandler;
    CanvasGroup panelOverlayGroup;
    float originalAlpha = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        buttonHandler = gameObject.GetComponent<ButtonHandler>();
        panelOverlayGroup = panelOverlayBG.GetComponent<CanvasGroup>();
    }

    void Start()
    {
        buttonHandler.AddButtonEventTrigger(panelExitBtn, _ => HomeUI.Instance.CloseSettingsPanel(),
            new ButtonConfig(yOffset: 0, callbackDelay: 0.1f, rotationLock: true));
        buttonHandler.AddButtonEventTrigger(panelBackBtn, _ => HomeUI.Instance.CloseSettingsPanel(),
            new ButtonConfig(yOffset: 0, callbackDelay: 0.1f, rotationLock: true));

        // tab 1 general
        buttonHandler.AddSwitch(switchButton1, new SwitchConfig(animationTime: 0.2f));
        buttonHandler.AddSwitch(switchButton2, new SwitchConfig(animationTime: 0.2f));
        buttonHandler.AddSwitch(switchButton3, new SwitchConfig(animationTime: 0.2f));
        buttonHandler.AddSliderEventTrigger(musicSlider, 1.2f, 0.1f);
        buttonHandler.AddSliderEventTrigger(sfxSlider, 1.2f, 0.1f);

        // Name Panel
        buttonHandler.AddButtonEventTrigger(changeNameBtn, _ => ToggleChangeNamePanel(true),
            new ButtonConfig(yOffset: -5f, callbackDelay: 0.1f, rotationLock: true));

        buttonHandler.AddButtonEventTrigger(changeNameConfirmBtn, _ => OnChangeNameClicked(),
            new ButtonConfig(yOffset: -5f, callbackDelay: 0.1f, rotationLock: true));

        buttonHandler.AddButtonEventTrigger(changeNamePanelExitBtn, _ => ToggleChangeNamePanel(false),
            new ButtonConfig(yOffset: -5, callbackDelay: 0.1f, rotationLock: true));

        // Reset Panel
        buttonHandler.AddButtonEventTrigger(resetDataBtn, _ => ToggleResetPanel(true),
            new ButtonConfig(yOffset: -5f, callbackDelay: 0.1f, rotationLock: true));

        buttonHandler.AddButtonEventTrigger(resetDataConfirmBtn, _ => OnResetData(),
            new ButtonConfig(yOffset: -5f, callbackDelay: 0.1f, rotationLock: true));

        buttonHandler.AddButtonEventTrigger(closeResetBtn, _ => ToggleResetPanel(false),
            new ButtonConfig(yOffset: 0, shrinkScale: 0.95f, callbackDelay: 0.1f, rotationLock: true));

        buttonHandler.AddButtonEventTrigger(closeResetBtnWindow, _ => ToggleResetPanel(false),
            new ButtonConfig(yOffset: 0, shrinkScale: 0.95f, callbackDelay: 0.1f, rotationLock: true));

        buttonHandler.AddButtonEventTrigger(panelOverlayBG.GetComponent<Button>(), _ =>
        {
            if (changeNamePanel.activeSelf)
                ToggleChangeNamePanel(false);
            if (resetPanel.activeSelf)
                ToggleResetPanel(false);
        }, new ButtonConfig(yOffset: 0, callbackDelay: 0.1f, rotationLock: true));
    }

    public void ToggleChangeNamePanel(bool isOpening)
    {
        changeNamePanel.SetActive(true);
        if (isOpening)
        {
            panelOverlayBG.SetActive(true);
            UpdateBGAlpha(0);
            changeNamePanel.transform.localScale = Vector3.zero;
            LeanTween.scale(changeNamePanel, Vector3.one, 0.15f).setEase(LeanTweenType.easeOutQuad);
            LeanTween.value(panelOverlayGroup.gameObject, UpdateBGAlpha, 0, originalAlpha, 0.25f).setEase(LeanTweenType.easeOutQuad);
        }
        else
        {
            LeanTween.scale(changeNamePanel, Vector3.zero, 0.15f).setEase(LeanTweenType.easeInQuad).setOnComplete(() =>
            {
                changeNamePanel.SetActive(false);
                panelOverlayBG.SetActive(false);
            });
            LeanTween.value(panelOverlayGroup.gameObject, UpdateBGAlpha, originalAlpha, 0, 0.25f).setEase(LeanTweenType.easeInQuad);
        }
    }

    public void ToggleResetPanel(bool isOpening)
    {
        resetPanel.SetActive(true);
        if (isOpening)
        {
            panelOverlayBG.SetActive(true);
            UpdateBGAlpha(0);
            resetPanel.transform.localScale = Vector3.zero;
            LeanTween.scale(resetPanel, Vector3.one, 0.15f).setEase(LeanTweenType.easeOutQuad);
            LeanTween.value(panelOverlayGroup.gameObject, UpdateBGAlpha, 0, originalAlpha, 0.25f).setEase(LeanTweenType.easeOutQuad);
        }
        else
        {
            LeanTween.scale(resetPanel, Vector3.zero, 0.15f).setEase(LeanTweenType.easeInQuad).setOnComplete(() =>
            {
                resetPanel.SetActive(false);
                panelOverlayBG.SetActive(false);
            });
            LeanTween.value(panelOverlayGroup.gameObject, UpdateBGAlpha, originalAlpha, 0, 0.25f).setEase(LeanTweenType.easeInQuad);
        }
    }

    void UpdateBGAlpha(float alpha)
    {
        panelOverlayGroup.alpha = alpha;
    }

    void OnChangeNameClicked()
    {
        string newName = nameInput.text;

        if (string.IsNullOrEmpty(newName) || newName.Length >= 10 || newName.Contains(" "))
        {
            NotificationManager.Instance.ShowNotification(NotificationManager.NotificationType.Warning,
                "Name must be shorter than 10 characters and contain no spaces.");
            return;
        }

        ToggleChangeNamePanel(false);
        NotificationManager.Instance.ShowNotification(NotificationManager.NotificationType.Success, "Name successfully changed.");
    }

    void OnResetData()
    {
        NotificationManager.Instance.ShowNotification(NotificationManager.NotificationType.Success, "Data has been reset.");
    }
}
