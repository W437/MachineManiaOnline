using UnityEngine;
using Fusion;
using TMPro;
using Assets.Scripts.TypewriterEffects;
using System;

public class LobbyHubManager
{
    private bool isMessageCooldown = false;
    public const float MESSAGE_DISPLAY_TIME = 2.5f;
    public const float MESSAGE_COOLDOWN_DURATION = 2f;


    public void ShowEmote(PlayerRef player, string emote)
    {
        if (isMessageCooldown) return;

        isMessageCooldown = true;

        switch (emote)
        {
            case "<sprite name=\"ManiaMoji_4\">":
                PlayCustomEmoteAnimation(player, emote);
                break;

            default:
                PlayDefaultEmoteAnimation(player, emote);
                break;
        }
    }

    public void ShowMessage(PlayerRef player, string message)
    {
        if (isMessageCooldown) return;

        isMessageCooldown = true;
        PlayMessageAnimation(player, message);
    }

    void PlayDefaultEmoteAnimation(PlayerRef player, string emote)
    {
        var emoteText = InitializeText(player, emote, 63, true);
        if (emoteText != null)
        {
            AnimateEmoteText(emoteText, () => ResetEmoteText(emoteText));
        }
    }

    void PlayCustomEmoteAnimation(PlayerRef player, string emote)
    {
        var emoteText = InitializeText(player, emote, 55, true);
        if (emoteText != null)
        {
            AnimateEmoteText(emoteText, () => ResetEmoteText(emoteText));
        }
    }

    void PlayMessageAnimation(PlayerRef player, string message)
    {
        var messageText = InitializeText(player, message, 28, false);
        if (messageText != null)
        {
            var typewriter = messageText.GetComponent<Typewriter>();
            typewriter.Animate(message);

            // Fade out after the display time
            LeanTween.delayedCall(MESSAGE_DISPLAY_TIME, () => FadeOutText(messageText, () => ResetMessageText(messageText)));
        }
    }

    TextMeshProUGUI InitializeText(PlayerRef player, string content, float fontSize, bool startWithScaleZero)
    {
        int playerIndex = PublicLobbyManager.Instance.FindPlayerPosition(player);
        if (playerIndex >= 0 && playerIndex < PublicLobbyManager.Instance.playerPosition.Length)
        {
            Transform position = LobbyUI.Instance.PlayerSlotsParent.GetChild(playerIndex);
            var textComponent = position.Find("messageTxt").GetComponent<TextMeshProUGUI>();

            if (textComponent != null)
            {
                LeanTween.cancel(textComponent.gameObject);
                textComponent.gameObject.SetActive(false);

                textComponent.fontSize = fontSize;
                textComponent.transform.localScale = startWithScaleZero ? Vector3.zero : Vector3.one;
                textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 1);

                textComponent.text = content;
                textComponent.gameObject.SetActive(true);

                if (startWithScaleZero)
                {
                    // Scale up immediately for emotes
                    LeanTween.scale(textComponent.gameObject, Vector3.one, 0.35f).setEase(LeanTweenType.easeOutBack);
                }

                return textComponent;
            }
        }
        return null;
    }

    void AnimateEmoteText(TextMeshProUGUI textComponent, Action onComplete)
    {
        // after
        LeanTween.delayedCall(MESSAGE_DISPLAY_TIME, () =>
        {
            LeanTween.scale(textComponent.gameObject, Vector3.zero, 0.5f)
                .setEase(LeanTweenType.easeInBack);

            LeanTween.alphaText(textComponent.rectTransform, 0f, 0.5f)
                .setEase(LeanTweenType.easeInBack)
                .setOnComplete(() => onComplete?.Invoke());
        });
    }

    void ResetEmoteText(TextMeshProUGUI textComponent)
    {
        textComponent.gameObject.SetActive(false);
        textComponent.transform.localScale = Vector3.one;
        isMessageCooldown = false;
    }

    void FadeOutText(TextMeshProUGUI textComponent, Action onComplete)
    {
        LeanTween.alphaText(textComponent.rectTransform, 0f, 0.5f)
            .setOnComplete(() => onComplete?.Invoke());
    }

    void ResetMessageText(TextMeshProUGUI textComponent)
    {
        textComponent.gameObject.SetActive(false);
        textComponent.transform.localScale = Vector3.one;
        isMessageCooldown = false;
    }
}
