using System;
using UnityEngine;
using TMPro;
using Assets.Scripts.TypewriterEffects;
using Assets.Scripts.TypewriterEffects.Notifiables;

public class TextEffects : MonoBehaviour, ITypingNotifiable
{
    private Action onTypingEndAction;

    void Awake()
    {
        var typewriter = GetComponent<Typewriter>();
        if (typewriter != null)
        {
            typewriter.typingNotifiables = new MonoBehaviour[] { this };
        }
    }

    public void SetOnTypingEnd(Action action)
    {
        onTypingEndAction = action;
    }

    public void OnTypingBegin() { }

    public void OnCaretMove() { }

    public void OnTypingEnd()
    {
        onTypingEndAction?.Invoke();
    }
}
