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
    [SerializeField] Button switchButton;
    [SerializeField] Slider musicSlider;
    ButtonHandler buttonHandler;

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
    }

    void Start()
    {
        buttonHandler.AddSwitch(switchButton, new SwitchConfig(animationTime: 0.2f));
        buttonHandler.AddSliderEventTrigger(musicSlider, 1.2f, 0.1f);
    }

}
