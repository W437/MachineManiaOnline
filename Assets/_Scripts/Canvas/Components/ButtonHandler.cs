using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    // button-related
    private Dictionary<Button, float> originalYPositions = new Dictionary<Button, float>();
    private Dictionary<Button, ButtonConfig> buttonConfigs = new Dictionary<Button, ButtonConfig>();
    private Dictionary<Button, Action<Button>> buttonCallbacks = new Dictionary<Button, Action<Button>>();
    private Dictionary<Button, bool> buttonToggledStates = new Dictionary<Button, bool>();
    private Dictionary<Button, Vector2> initialPressPositions = new Dictionary<Button, Vector2>();
    private Dictionary<Button, Vector3> parentOriginalPositions = new Dictionary<Button, Vector3>();
    private Dictionary<Button, bool> withinThreshold = new Dictionary<Button, bool>();
    private Dictionary<Button, bool> buttonCooldowns = new Dictionary<Button, bool>();

    // Switch-related
    private Dictionary<Button, bool> switchStates = new Dictionary<Button, bool>();
    private Dictionary<Button, SwitchConfig> switchConfigs = new Dictionary<Button, SwitchConfig>();
    private Dictionary<Button, RectTransform> toggleRects = new Dictionary<Button, RectTransform>();
    private Dictionary<Button, bool> switchCooldowns = new Dictionary<Button, bool>();

    private Dictionary<Slider, Vector3> originalHandleScales = new Dictionary<Slider, Vector3>();


    private Vector3 _originalScale;
    private Vector3 _originalRotation;

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

    private void OnButtonPressed(Button button, Transform buttonTransform, PointerEventData eventData)
    {
        if (buttonCooldowns[button]) return;

        AudioManager.Instance.PlayMenuSFX(AudioManager.MenuSFX.Click);

        LeanTween.cancel(button.gameObject);

        var config = buttonConfigs[button];
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
    }


    private void OnButtonDragged(Button button, Transform buttonTransform, PointerEventData eventData)
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

    private void ApplyCustomAnimation(Button button, Transform parentTransform, Vector2 normalizedPoint, float animationTime)
    {
        float rotationDirection = normalizedPoint.x >= 0 ? 1 : -1;
        float randomRotation = rotationDirection * UnityEngine.Random.Range(2f, 5f);

        LeanTween.rotateZ(parentTransform.gameObject, randomRotation, animationTime).setEase(LeanTweenType.easeInExpo);
    }

    private void OnButtonReleased(Button button, Transform buttonTransform, PointerEventData eventData)
    {
        if (buttonCooldowns[button]) return;
        var config = buttonConfigs[button];

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
            else
            {
                // Delay the callback to ensure animation completion
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

        StartCoroutine(ButtonCooldown(button));
    }


    private IEnumerator InvokeCallbackAfterDelay(Button button, float delay)
    {
        yield return new WaitForSeconds(delay);
        buttonCallbacks[button]?.Invoke(button);
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

    private IEnumerator ButtonCooldown(Button button)
    {
        buttonCooldowns[button] = true;
        yield return new WaitForSeconds(.533f);
        buttonCooldowns[button] = false;
    }

    // for handling switch buttons
    private void OnSwitchClicked(Button switchButton)
    {
        if (switchCooldowns[switchButton]) return;

        StartCoroutine(SwitchCooldown(switchButton));

        bool currentState = switchStates[switchButton];
        switchStates[switchButton] = !currentState; 
        SetTogglePosition(switchButton, switchStates[switchButton]);
    }

    private IEnumerator SwitchCooldown(Button switchButton)
    {
        switchCooldowns[switchButton] = true;
        yield return new WaitForSeconds(0.5f);
        switchCooldowns[switchButton] = false;
    }

    private void SetTogglePosition(Button switchButton, bool isOn)
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

    private void OnSliderHandlePressed(Slider slider, float growFactor, float animationTime)
    {
        LeanTween.scale(slider.handleRect.gameObject, originalHandleScales[slider] * growFactor, animationTime).setEase(LeanTweenType.easeOutExpo);
    }

    private void OnSliderHandleDragged(Slider slider, PointerEventData eventData)
    {
        // Allow the slider to be dragged normally
        slider.OnDrag(eventData);
    }

    private void OnSliderHandleReleased(Slider slider, float animationTime)
    {
        LeanTween.scale(slider.handleRect.gameObject, originalHandleScales[slider], animationTime).setEase(LeanTweenType.easeOutExpo);
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

    public ButtonConfig(float yOffset = -7f, float shrinkScale = 1f, float animationTime = 0.1f, float returnTime = 0.133f, bool toggle = false, float thresholdDistance = 1100f, float callbackDelay = 0f, bool customAnimation = false, bool realTimeUpdate = false, bool rotationLock = false, float pinchMoveDistance = 2f, Vector3 rotation = default(Vector3))
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