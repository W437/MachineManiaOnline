
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    // button-related
    Dictionary<Button, float> originalYPositions = new Dictionary<Button, float>();
    Dictionary<Button, ButtonConfig> buttonConfigs = new Dictionary<Button, ButtonConfig>();
    Dictionary<Button, Action<Button>> buttonCallbacks = new Dictionary<Button, Action<Button>>();
    Dictionary<Button, bool> buttonToggledStates = new Dictionary<Button, bool>();
    Dictionary<Button, Vector2> initialPressPositions = new Dictionary<Button, Vector2>();
    Dictionary<Button, Vector3> parentOriginalPositions = new Dictionary<Button, Vector3>();
    Dictionary<Button, bool> withinThreshold = new Dictionary<Button, bool>();
    Dictionary<Button, bool> buttonCooldowns = new Dictionary<Button, bool>();

    // Switch-related
    Dictionary<Button, bool> switchStates = new Dictionary<Button, bool>();
    Dictionary<Button, SwitchConfig> switchConfigs = new Dictionary<Button, SwitchConfig>();
    Dictionary<Button, RectTransform> toggleRects = new Dictionary<Button, RectTransform>();
    Dictionary<Button, bool> switchCooldowns = new Dictionary<Button, bool>();
    Dictionary<Slider, Vector3> originalHandleScales = new Dictionary<Slider, Vector3>();
    Vector3 _originalScale;
    Vector3 _originalRotation;

    public void AddButtonEventTrigger(Button button, Action<Button> onButtonReleased, ButtonConfig config = null)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDownEntry.callback.AddListener((eventData) => { OnButtonPressed(button, button.transform, eventData as PointerEventData); });
        trigger.triggers.Add(pointerDownEntry);

        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUpEntry.callback.AddListener((eventData) => { OnButtonReleased(button, button.transform, eventData as PointerEventData); });
        trigger.triggers.Add(pointerUpEntry);

        EventTrigger.Entry dragEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        dragEntry.callback.AddListener((eventData) => { OnButtonDragged(button, button.transform, eventData as PointerEventData); });
        trigger.triggers.Add(dragEntry);

        originalYPositions[button] = button.transform.localPosition.y;
        buttonConfigs[button] = config ?? new ButtonConfig();
        buttonCallbacks[button] = onButtonReleased;
        buttonToggledStates[button] = false;
        parentOriginalPositions[button] = button.transform.parent.localPosition;
        withinThreshold[button] = true;
        buttonCooldowns[button] = false;
    }

    public void AddSwitch(Button switchButton, SwitchConfig config = null)
    {
        EventTrigger trigger = switchButton.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerClickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        pointerClickEntry.callback.AddListener((eventData) => { OnSwitchClicked(switchButton); });
        trigger.triggers.Add(pointerClickEntry);

        switchStates[switchButton] = false;
        switchConfigs[switchButton] = config ?? new SwitchConfig();
        switchCooldowns[switchButton] = false;

        RectTransform toggleRect = switchButton.transform.Find("Container/Toggle").GetComponent<RectTransform>();
        toggleRects[switchButton] = toggleRect;

        SetTogglePosition(switchButton, false);
    }

    public void AddSliderEventTrigger(Slider slider, float growFactor = 1.5f, float animationTime = 0.1f)
    {
        EventTrigger trigger = slider.handleRect.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = slider.handleRect.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDownEntry.callback.AddListener((eventData) => { OnSliderHandlePressed(slider, growFactor, animationTime); });
        trigger.triggers.Add(pointerDownEntry);

        EventTrigger.Entry dragEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        dragEntry.callback.AddListener((eventData) => { OnSliderHandleDragged(slider, eventData as PointerEventData); });
        trigger.triggers.Add(dragEntry);

        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUpEntry.callback.AddListener((eventData) => { OnSliderHandleReleased(slider, animationTime); });
        trigger.triggers.Add(pointerUpEntry);

        originalHandleScales[slider] = slider.handleRect.localScale;
    }
    public void ResetButtonToggleState(Button button)
    {
        if (buttonToggledStates.ContainsKey(button) && buttonToggledStates[button])
        {
            var config = buttonConfigs[button];

            LeanTween.moveLocal(button.transform.parent.gameObject, parentOriginalPositions[button], config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveLocalY(button.gameObject, originalYPositions[button], config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
            LeanTween.scale(button.gameObject, _originalScale, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);

            if (config.CustomAnimation)
            {
                LeanTween.scale(button.transform.parent.gameObject, Vector3.one, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                LeanTween.rotate(button.transform.parent.gameObject, _originalRotation, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
            }

            buttonToggledStates[button] = false;
        }
    }

    void OnButtonPressed(Button button, Transform buttonTransform, PointerEventData eventData)
    {
        var config = buttonConfigs[button];

        if (config.CooldownEnabled && buttonCooldowns[button]) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuSFX(AudioManager.MenuSFX.Click);

        LeanTween.cancel(button.gameObject); // Cancel any ongoing animations
        LeanTween.cancel(button.transform.parent.gameObject); // Cancel parent animations too
        buttonTransform.localScale = Vector3.one;
        button.transform.parent.transform.localScale = Vector3.one;

        _originalScale = buttonTransform.localScale;
        initialPressPositions[button] = eventData.position;
        Transform parentTransform = button.transform.parent;
        _originalRotation = parentTransform.localRotation.eulerAngles;

        Vector2 _localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(buttonTransform as RectTransform, eventData.position, eventData.pressEventCamera, out _localPoint);
        RectTransform buttonRectTransform = buttonTransform as RectTransform;
        Vector2 normalizedPoint = (_localPoint - (Vector2)buttonRectTransform.rect.center) / (buttonRectTransform.rect.size / 2);

        if (!config.RotationLock)
        {
            Vector3 targetPosition = parentOriginalPositions[button] + (Vector3)(normalizedPoint * config.PinchMoveDistance);
            LeanTween.moveLocal(parentTransform.gameObject, targetPosition, config.AnimationTime).setEase(LeanTweenType.easeInExpo);
        }

        if (!config.Toggle || !buttonToggledStates[button])
        {
            LeanTween.moveLocalY(button.gameObject, originalYPositions[button] + config.YOffset, config.AnimationTime).setEase(LeanTweenType.easeInExpo);

            if (config.ShrinkScale != 1)
            {
                LeanTween.scale(button.gameObject, _originalScale * config.ShrinkScale, config.AnimationTime).setEase(LeanTweenType.easeInExpo);
            }

            if (config.CustomAnimation)
            {
                ApplyCustomAnimation(button, parentTransform, normalizedPoint, config.AnimationTime);
            }

            LeanTween.rotate(parentTransform.gameObject, config.Rotation, config.AnimationTime).setEase(LeanTweenType.easeInExpo);
        }

        if (config.ActivateOnPress)
        {
            AnimateButtonPress(button);
            buttonCallbacks[button]?.Invoke(button);
        }

        if (config.Toggle)
        {
            buttonCallbacks[button]?.Invoke(button);
        }
    }

    void AnimateButtonPress(Button button)
    {
        LeanTween.cancel(button.gameObject); // Ensure no overlapping animations
        LeanTween.scale(button.gameObject, _originalScale * 1.1f, 0.1f).setEase(LeanTweenType.easeOutExpo).setOnComplete(() =>
        {
            LeanTween.scale(button.gameObject, _originalScale, 0.05f).setEase(LeanTweenType.easeInExpo);
        });
    }

    void OnButtonDragged(Button button, Transform buttonTransform, PointerEventData eventData)
    {
        if (buttonCooldowns[button]) return;
        var config = buttonConfigs[button];
        Transform parentTransform = button.transform.parent;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(buttonTransform as RectTransform, eventData.position, eventData.pressEventCamera, out localPoint);

        RectTransform buttonRectTransform = buttonTransform as RectTransform;
        Vector2 normalizedPoint = (localPoint - (Vector2)buttonRectTransform.rect.center) / (buttonRectTransform.rect.size / 2);

        Vector3 targetPosition = parentOriginalPositions[button] + (Vector3)(normalizedPoint * config.PinchMoveDistance);

        if (config.RealTimeUpdate && Vector2.Distance(eventData.position, initialPressPositions[button]) <= config.ThresholdDistance)
        {
            withinThreshold[button] = true;

            LeanTween.value(parentTransform.gameObject, parentTransform.localPosition, targetPosition, config.AnimationTime)
                .setEase(LeanTweenType.linear)
                .setOnUpdate((Vector3 pos) => {
                    parentTransform.localPosition = pos;
                });

            if (config.CustomAnimation)
            {
                float rotationDirection = normalizedPoint.x >= 0 ? 1 : -1;
                float maxRotation = 1f;
                float targetRotation = normalizedPoint.x * maxRotation;

                LeanTween.rotateZ(parentTransform.gameObject, targetRotation, config.AnimationTime).setEase(LeanTweenType.linear);
            }
        }
        else if (withinThreshold[button])
        {
            LeanTween.moveLocal(parentTransform.gameObject, parentOriginalPositions[button], config.ReturnTime).setEase(LeanTweenType.easeOutSine);
            LeanTween.moveLocalY(button.gameObject, originalYPositions[button], config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
            LeanTween.scale(button.gameObject, _originalScale, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);

            if (config.CustomAnimation)
            {
                LeanTween.scale(parentTransform.gameObject, Vector3.one, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                LeanTween.rotate(parentTransform.gameObject, _originalRotation, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
            }

            withinThreshold[button] = false;
        }
    }

    void ApplyCustomAnimation(Button button, Transform parentTransform, Vector2 normalizedPoint, float animationTime)
    {
        float rotationDirection = normalizedPoint.x >= 0 ? 1 : -1;
        float randomRotation = rotationDirection * UnityEngine.Random.Range(2f, 5f);

        LeanTween.rotateZ(parentTransform.gameObject, randomRotation, animationTime).setEase(LeanTweenType.easeInExpo);
    }

    void OnButtonReleased(Button button, Transform buttonTransform, PointerEventData eventData)
    {
        var config = buttonConfigs[button];

        if (config.CooldownEnabled && buttonCooldowns[button]) return;

        Transform parentTransform = button.transform.parent;

        if (Vector2.Distance(eventData.position, initialPressPositions[button]) <= config.ThresholdDistance)
        {
            if (config.Toggle)
            {
                if (buttonToggledStates[button])
                {
                    LeanTween.moveLocal(parentTransform.gameObject, parentOriginalPositions[button], config.ReturnTime).setEase(LeanTweenType.easeOutBounce);
                    LeanTween.moveLocalY(button.gameObject, originalYPositions[button], config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                    LeanTween.scale(button.gameObject, _originalScale, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);

                    if (config.CustomAnimation)
                    {
                        LeanTween.scale(parentTransform.gameObject, Vector3.one, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                        LeanTween.rotate(parentTransform.gameObject, _originalRotation, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                    }
                }

                buttonToggledStates[button] = !buttonToggledStates[button];
            }
            else if (!config.ActivateOnPress)
            {
                LeanTween.moveLocal(parentTransform.gameObject, parentOriginalPositions[button], config.ReturnTime).setEase(LeanTweenType.easeOutBounce).setOnComplete(() =>
                {
                    LeanTween.moveLocalY(button.gameObject, originalYPositions[button], config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                    LeanTween.scale(button.gameObject, _originalScale, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);

                    if (config.CustomAnimation)
                    {
                        LeanTween.scale(parentTransform.gameObject, Vector3.one, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                        LeanTween.rotate(parentTransform.gameObject, _originalRotation, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                    }

                    if (config.CallbackDelay > 0)
                    {
                        StartCoroutine(InvokeCallbackAfterDelay(button, config.CallbackDelay));
                    }
                    else
                    {
                        buttonCallbacks[button]?.Invoke(button);
                    }
                });
            }
        }
        else
        {
            LeanTween.moveLocal(parentTransform.gameObject, parentOriginalPositions[button], config.ReturnTime).setEase(LeanTweenType.easeOutBounce);
            LeanTween.moveLocalY(button.gameObject, originalYPositions[button], config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
            LeanTween.scale(button.gameObject, _originalScale, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);

            if (config.CustomAnimation)
            {
                LeanTween.scale(parentTransform.gameObject, Vector3.one, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                LeanTween.rotate(parentTransform.gameObject, _originalRotation, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
            }
        }

        if (config.CooldownEnabled)
        {
            StartCoroutine(ButtonCooldown(button));
        }
    }

    // for handling switch buttons
    void OnSwitchClicked(Button switchButton)
    {
        if (switchCooldowns[switchButton]) return;

        StartCoroutine(SwitchCooldown(switchButton));

        bool currentState = switchStates[switchButton];
        switchStates[switchButton] = !currentState;
        SetTogglePosition(switchButton, switchStates[switchButton]);
    }

    public void SetSwitchState(Button switchButton, bool state)
    {
        switchStates[switchButton] = state;
        SetTogglePosition(switchButton, state);
    }

    void SetTogglePosition(Button switchButton, bool isOn)
    {
        RectTransform toggleRect = toggleRects[switchButton];
        RectTransform containerRect = toggleRect.parent.GetComponent<RectTransform>();
        SwitchConfig config = switchConfigs[switchButton];

        float halfContainerWidth = containerRect.rect.width / 2;
        float halfToggleWidth = toggleRect.rect.width / 2;
        float margin = 10f;

        float leftPosition = -halfContainerWidth + halfToggleWidth + margin;
        float rightPosition = halfContainerWidth - halfToggleWidth - margin;

        float targetX = isOn ? rightPosition : leftPosition;

        LeanTween.moveLocalX(toggleRect.gameObject, targetX, config.AnimationTime)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(() =>
            {
                if (isOn)
                {
                    // bounce and color flash when turned on
                    LeanTween.scale(toggleRect.gameObject, Vector3.one * 1.1f, 0.1f)
                        .setEase(LeanTweenType.easeOutBounce)
                        .setOnComplete(() =>
                        {
                            LeanTween.scale(toggleRect.gameObject, Vector3.one, 0.1f).setEase(LeanTweenType.easeOutBounce);
                        });

                    Image toggleImage = toggleRect.GetComponent<Image>();
                    Color originalColor = toggleImage.color;

                    LeanTween.value(toggleRect.gameObject, originalColor, Color.yellow, 0.1f)
                        .setOnUpdate((Color color) => toggleImage.color = color)
                        .setOnComplete(() =>
                        {
                            LeanTween.value(toggleRect.gameObject, Color.yellow, originalColor, 0.1f)
                                .setOnUpdate((Color color) => toggleImage.color = color);
                        });
                }

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayMenuSFX(AudioManager.MenuSFX.Click);
            });

        Image toggleImageState = toggleRect.GetComponent<Image>();
        toggleImageState.color = isOn ? Color.white : config.OffColor;

        Transform textOn = containerRect.Find("TextOn");
        Transform textOff = containerRect.Find("TextOff");

        if (textOn != null && textOff != null)
        {
            textOn.gameObject.SetActive(isOn);
            textOff.gameObject.SetActive(!isOn);
        }
    }

    void OnSliderHandlePressed(Slider slider, float growFactor, float animationTime)
    {
        LeanTween.scale(slider.handleRect.gameObject, originalHandleScales[slider] * growFactor, animationTime).setEase(LeanTweenType.easeOutExpo);
    }

    void OnSliderHandleDragged(Slider slider, PointerEventData eventData)
    {
        // Allow the slider to be dragged normally
        slider.OnDrag(eventData);
    }

    void OnSliderHandleReleased(Slider slider, float animationTime)
    {
        LeanTween.scale(slider.handleRect.gameObject, originalHandleScales[slider], animationTime).setEase(LeanTweenType.easeOutExpo);
    }

    IEnumerator SwitchCooldown(Button switchButton)
    {
        switchCooldowns[switchButton] = true;
        yield return new WaitForSeconds(0.5f);
        switchCooldowns[switchButton] = false;
    }

    IEnumerator InvokeCallbackAfterDelay(Button button, float delay)
    {
        yield return new WaitForSeconds(delay);
        buttonCallbacks[button]?.Invoke(button);
    }

    IEnumerator ButtonCooldown(Button button)
    {
        buttonCooldowns[button] = true;
        yield return new WaitForSeconds(.533f);
        buttonCooldowns[button] = false;
    }
}

public class ButtonConfig
{
    public float YOffset { get; set; } // Vertical offset on press
    public float ShrinkScale { get; set; } // Scale factor on press
    public float AnimationTime { get; set; } // Duration of the animation when pressed
    public float ReturnTime { get; set; } // Duration of animation released and returns to its original state
    public bool Toggle { get; set; } // If true, toggle button
    public float ThresholdDistance { get; set; } // Max distance for a drag to still register as a click
    public float CallbackDelay { get; set; } // Delay before the callback is invoked after release
    public bool CustomAnimation { get; set; } // Enables or disables custom animations on button interactions
    public bool RealTimeUpdate { get; set; } // If true, updates button state in real-time during drag
    public bool RotationLock { get; set; } // Prevents rotation (of parent) during interaction
    public float PinchMoveDistance { get; set; } // Distance the parent can move when pinched
    public Vector3 Rotation { get; set; }
    public bool ActivateOnPress { get; set; } // Determines if the action should occur on press
    public bool CooldownEnabled { get; set; } // Enable or disable cooldown

    // kONSTRUCTOR
    public ButtonConfig(float yOffset = -7f, float shrinkScale = 1f, float animationTime = 0.1f, float returnTime = 0f, bool toggle = false, float thresholdDistance = 1100f, float callbackDelay = 0f, bool customAnimation = false, bool realTimeUpdate = false, bool rotationLock = false, float pinchMoveDistance = 2f, Vector3 rotation = default(Vector3), bool activateOnPress = false, bool cooldownEnabled = true)
    {
        YOffset = yOffset;
        ShrinkScale = shrinkScale;
        AnimationTime = animationTime;
        ReturnTime = returnTime;
        Toggle = toggle;
        ThresholdDistance = thresholdDistance;
        CallbackDelay = callbackDelay;
        CustomAnimation = customAnimation;
        RealTimeUpdate = realTimeUpdate;
        RotationLock = rotationLock;
        PinchMoveDistance = pinchMoveDistance;
        Rotation = rotation;
        ActivateOnPress = activateOnPress;
        CooldownEnabled = cooldownEnabled;
    }
}

public class SwitchConfig
{
    public float AnimationTime { get; set; }
    public Color OffColor { get; set; }
    public SwitchConfig(float animationTime = 0.2f)
    {
        AnimationTime = animationTime;
        OffColor = new Color(0.255f, 0.545f, 0.875f);
    }
}