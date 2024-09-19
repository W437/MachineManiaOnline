using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;

public class HomeChatManager : NetworkBehaviour
{
    public static HomeChatManager Instance;

    public bool chatEffectsEnabled = true;
    [SerializeField] GameObject chatMessagePrefab;
    List<GameObject> messages = new List<GameObject>();
    Dictionary<string, System.Action> specialMessages = new Dictionary<string, System.Action>();
    Vector2 chatOriginalPosition;
    Vector2 chatOffScreenPosition;
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
    }

    void Start()
    {
        HomeUI.Instance.ButtonHandler.AddButtonEventTrigger(HomeUI.Instance.SendMessageButton, OnSendButtonClicked, new ButtonConfig(yOffset: -4f, animationTime: 0.15f, returnTime: 0.15f, realTimeUpdate: true));
        HomeUI.Instance.BGExitButton.onClick.AddListener(ToggleChat);
        HomeUI.Instance.ChatExitButton.onClick.AddListener(HideChat);
        HomeUI.Instance.ChatPanel.SetActive(false);

        specialMessages["/wave"] = PlayWaveAnimation;
        specialMessages["/cheer"] = PlayCheerAnimation;

        RectTransform chatContainer = HomeUI.Instance.ChatContainer;
        chatOriginalPosition = chatContainer.anchoredPosition;
        chatOffScreenPosition = new Vector2(chatOriginalPosition.x, chatOriginalPosition.y + 900f);
    }

    public void ToggleChat()
    {
        if (HomeUI.Instance.ChatVisible)
        {
            HideChat();
        }
        else
        {
            ShowChat();
        }
    }

    public void ShowChat()
    {
        HomeUI.Instance.ChatVisible = true;
        HomeUI.Instance.ChatPanel.SetActive(true);

        CanvasGroup bgCanvasGroup = HomeUI.Instance.ChatBG.GetComponent<CanvasGroup>();
        CanvasGroup chatCanvasGroup = HomeUI.Instance.ChatContainer.GetComponent<CanvasGroup>();

        bgCanvasGroup.alpha = 0f;
        chatCanvasGroup.alpha = 0f;

        LeanTween.alphaCanvas(bgCanvasGroup, 1f, HomeUI.Instance.FadeDuration);
        LeanTween.alphaCanvas(chatCanvasGroup, 1f, HomeUI.Instance.FadeDuration);

        RectTransform chatContainer = HomeUI.Instance.ChatContainer;
        chatContainer.anchoredPosition = chatOffScreenPosition;

        LeanTween.move(chatContainer, chatOriginalPosition, HomeUI.Instance.SlideDuration)
            .setEase(LeanTweenType.easeOutQuart);
    }

    public void HideChat()
    {
        HomeUI.Instance.ChatVisible = false;

        CanvasGroup bgCanvasGroup = HomeUI.Instance.ChatBG.GetComponent<CanvasGroup>();
        CanvasGroup chatCanvasGroup = HomeUI.Instance.ChatContainer.GetComponent<CanvasGroup>();

        LeanTween.alphaCanvas(bgCanvasGroup, 0f, HomeUI.Instance.FadeDuration);
        LeanTween.alphaCanvas(chatCanvasGroup, 0f, HomeUI.Instance.FadeDuration);

        RectTransform chatContainer = HomeUI.Instance.ChatContainer;
        LeanTween.move(chatContainer, chatOffScreenPosition, HomeUI.Instance.SlideDuration)
            .setEase(LeanTweenType.easeInQuart)
            .setOnComplete(() =>
            {
                chatContainer.anchoredPosition = chatOriginalPosition;

                HomeUI.Instance.ChatPanel.SetActive(false);
            });
    }

    public void ToggleChatPanel()
    {
        HomeUI.Instance.ChatPanel.SetActive(!HomeUI.Instance.ChatPanel.activeSelf);
    }

    void OnSendButtonClicked(Button button)
    {
        Debug.Log(button);
        if (!string.IsNullOrEmpty(HomeUI.Instance.MessageInputField.text))
        {
            SendChatMessage(HomeUI.Instance.MessageInputField.text);
            HomeUI.Instance.MessageInputField.text = string.Empty;
        }
    }

    void SendChatMessage(string message)
    {
        RPC_SendChatMessage(message, Runner.LocalPlayer);
    }

    void AddMessageToChat(string owner, string message)
    {
        GameObject newMessage = Instantiate(chatMessagePrefab, HomeUI.Instance.MessageContent);
        var messageText = newMessage.transform.Find("message").GetComponent<TextMeshProUGUI>();
        var ownerText = newMessage.transform.Find("owner").GetComponent<TextMeshProUGUI>();

        messageText.text = message;
        ownerText.text = owner;

        messages.Add(newMessage);

        UpdateContentHeight(HomeUI.Instance.MessageContent, chatMessagePrefab, HomeUI.Instance.MessageScrollView);

        LayoutRebuilder.ForceRebuildLayoutImmediate(HomeUI.Instance.MessageContent.GetComponent<RectTransform>());

        newMessage.transform.localScale = new Vector3(1, 0, 1);
        LeanTween.scaleY(newMessage, 1, 0.3f).setEase(LeanTweenType.easeOutQuart);

        Canvas.ForceUpdateCanvases();
        HomeUI.Instance.MessageScrollView.verticalNormalizedPosition = 0f;
    }

    void UpdateContentHeight(Transform listParent, GameObject prefab, ScrollRect scrollRect)
    {
        RectTransform contentRectTransform = listParent.GetComponent<RectTransform>();
        float itemHeight = prefab.GetComponent<RectTransform>().rect.height;
        float spacing = listParent.GetComponent<VerticalLayoutGroup>().spacing;
        float totalHeight = (itemHeight + spacing) * listParent.childCount - spacing;

        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, totalHeight);

        Canvas.ForceUpdateCanvases();
    }

    void PlayWaveAnimation()
    {
        if (chatEffectsEnabled)
        {
            Debug.Log("Playing wave animation for all clients.");
        }
    }

    void PlayCheerAnimation()
    {
        if (chatEffectsEnabled)
        {
            Debug.Log("Playing cheer animation for all clients.");
        }
    }
    
    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RPC_SendChatMessage(string message, PlayerRef sender)
    {
        AddMessageToChat(sender.PlayerId.ToString(), message);

        if (specialMessages.ContainsKey(message))
        {
            specialMessages[message]?.Invoke();
        }
    }
}
