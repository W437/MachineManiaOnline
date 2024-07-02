using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    private Dictionary<Button, float> originalYPositions = new Dictionary<Button, float>();
    private Dictionary<Button, ButtonConfig> buttonConfigs = new Dictionary<Button, ButtonConfig>();
    private Dictionary<Button, Action<Button>> buttonCallbacks = new Dictionary<Button, Action<Button>>();
    private Dictionary<Button, bool> buttonToggledStates = new Dictionary<Button, bool>();
    private Dictionary<Button, Vector2> initialPressPositions = new Dictionary<Button, Vector2>();
    private Dictionary<Button, Vector3> parentOriginalPositions = new Dictionary<Button, Vector3>();
    private Dictionary<Button, bool> withinThreshold = new Dictionary<Button, bool>();
    private Dictionary<Button, bool> buttonCooldowns = new Dictionary<Button, bool>();

    private Vector3 originalScale;
    private Vector3 originalRotation;

    private Action<Button> onButtonReleasedCallback;

    // Main method used to add triggers to button events w/ custom callbacks n' config
    public void AddEventTrigger(Button button, Action<Button> onButtonReleased, ButtonConfig config = null)
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

    private void OnButtonPressed(Button button, Transform buttonTransform, PointerEventData eventData)
    {
        if (buttonCooldowns[button]) return;

        ServiceLocator.GetAudioManager().PlayMenuSFX(AudioManager.MenuSFX.Click);

        LeanTween.cancel(button.gameObject);

        var config = buttonConfigs[button];
        originalScale = buttonTransform.localScale;

        initialPressPositions[button] = eventData.position;

        Transform parentTransform = button.transform.parent;
        originalRotation = parentTransform.localRotation.eulerAngles;

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
                LeanTween.scale(button.gameObject, originalScale * config.ShrinkScale, config.AnimationTime).setEase(LeanTweenType.easeInExpo);
            }

            if (config.CustomAnimation)
            {
                ApplyCustomAnimation(button, parentTransform, normalizedPoint, config.AnimationTime);
            }
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
            LeanTween.scale(button.gameObject, originalScale, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);

            if (config.CustomAnimation)
            {
                LeanTween.scale(parentTransform.gameObject, Vector3.one, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                LeanTween.rotate(parentTransform.gameObject, originalRotation, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
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
                    LeanTween.scale(button.gameObject, originalScale, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);

                    if (config.CustomAnimation)
                    {
                        LeanTween.scale(parentTransform.gameObject, Vector3.one, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                        LeanTween.rotate(parentTransform.gameObject, originalRotation, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                    }
                }

                buttonToggledStates[button] = !buttonToggledStates[button];
            }
            else
            {
                LeanTween.moveLocal(parentTransform.gameObject, parentOriginalPositions[button], config.ReturnTime).setEase(LeanTweenType.easeOutBounce);
                LeanTween.moveLocalY(button.gameObject, originalYPositions[button], config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                LeanTween.scale(button.gameObject, originalScale, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);

                if (config.CustomAnimation)
                {
                    LeanTween.scale(parentTransform.gameObject, Vector3.one, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                    LeanTween.rotate(parentTransform.gameObject, originalRotation, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                }
            }

            if (config.CallbackDelay > 0)
            {
                StartCoroutine(InvokeCallbackAfterDelay(button, config.CallbackDelay));
            }
            else
            {
                buttonCallbacks[button]?.Invoke(button);
            }
        }
        else
        {
            LeanTween.moveLocal(parentTransform.gameObject, parentOriginalPositions[button], config.ReturnTime).setEase(LeanTweenType.easeOutBounce);
            LeanTween.moveLocalY(button.gameObject, originalYPositions[button], config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
            LeanTween.scale(button.gameObject, originalScale, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);

            if (config.CustomAnimation)
            {
                LeanTween.scale(parentTransform.gameObject, Vector3.one, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                LeanTween.rotate(parentTransform.gameObject, originalRotation, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
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
            LeanTween.scale(button.gameObject, originalScale, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);

            if (config.CustomAnimation)
            {
                LeanTween.scale(button.transform.parent.gameObject, Vector3.one, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
                LeanTween.rotate(button.transform.parent.gameObject, originalRotation, config.ReturnTime).setEase(LeanTweenType.easeOutExpo);
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
}

public class ButtonConfig
{
    public float YOffset { get; set; }
    public float ShrinkScale { get; set; }
    public float AnimationTime { get; set; }
    public float ReturnTime { get; set; }
    public bool Toggle { get; set; }
    public float ThresholdDistance { get; set; }
    public float CallbackDelay { get; set; }
    public bool CustomAnimation { get; set; }
    public bool RealTimeUpdate { get; set; }
    public bool RotationLock { get; set; }
    public float PinchMoveDistance { get; set; }

    public ButtonConfig(float yOffset = -7f, float shrinkScale = 1f, float animationTime = 0.1f, float returnTime = 0.333f, bool toggle = false, float thresholdDistance = 1100f, float callbackDelay = 0f, bool customAnimation = false, bool realTimeUpdate = false, bool rotationLock = false, float pinchMoveDistance = 2f)
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
    }
}
