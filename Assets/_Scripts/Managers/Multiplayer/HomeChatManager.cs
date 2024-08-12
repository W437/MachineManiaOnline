using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;

public class HomeChatManager : NetworkBehaviour
{
    public static HomeChatManager Instance;

    [SerializeField] private GameObject chatMessagePrefab;
    private List<GameObject> messages = new List<GameObject>();
    private Dictionary<string, System.Action> specialMessages = new Dictionary<string, System.Action>();

    private ButtonHandler buttonHandler;

    public bool chatEffectsEnabled = true;

    private void Awake()
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
        LobbyUI.Instance.ButtonHandler.AddButtonEventTrigger(LobbyUI.Instance.SendMessageButton, OnSendButtonClicked, new ButtonConfig(yOffset: -4f, animationTime: 0.15f, returnTime: 0.15f, realTimeUpdate: true));
        LobbyUI.Instance.BGExitButton.onClick.AddListener(ToggleChat);
        LobbyUI.Instance.ChatExitButton.onClick.AddListener(HideChat);
        LobbyUI.Instance.ChatPanel.SetActive(false);

        specialMessages["/wave"] = PlayWaveAnimation;
        specialMessages["/cheer"] = PlayCheerAnimation;
    }

    public void ToggleChat()
    {
        if (LobbyUI.Instance.ChatVisible)
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
        LobbyUI.Instance.ChatVisible = true;

        LobbyUI.Instance.ChatPanel.SetActive(true);

        CanvasGroup bgCanvasGroup = LobbyUI.Instance.ChatBG.GetComponent<CanvasGroup>();
        CanvasGroup chatCanvasGroup = LobbyUI.Instance.ChatContainer.GetComponent<CanvasGroup>();

        bgCanvasGroup.alpha = 0f;
        chatCanvasGroup.alpha = 0f;

        LeanTween.alphaCanvas(bgCanvasGroup, 1f, LobbyUI.Instance.FadeDuration);
        LeanTween.alphaCanvas(chatCanvasGroup, 1f, LobbyUI.Instance.FadeDuration);

        Vector2 originalPosition = LobbyUI.Instance.ChatContainer.position;

        LobbyUI.Instance.ChatContainer.anchoredPosition = new Vector3(0, 300, 0);

        LeanTween.move(LobbyUI.Instance.ChatContainer, originalPosition, LobbyUI.Instance.SlideDuration).setEase(LeanTweenType.easeOutQuart);
    }

    public void HideChat()
    {
        LobbyUI.Instance.ChatVisible = false;
         
        CanvasGroup bgCanvasGroup = LobbyUI.Instance.ChatBG.GetComponent<CanvasGroup>();
        CanvasGroup chatCanvasGroup = LobbyUI.Instance.ChatContainer.GetComponent<CanvasGroup>();

        LeanTween.alphaCanvas(bgCanvasGroup, 0f, LobbyUI.Instance.FadeDuration);
        LeanTween.alphaCanvas(chatCanvasGroup, 0f, LobbyUI.Instance.FadeDuration);

        Vector2 originalPosition = LobbyUI.Instance.ChatContainer.anchoredPosition;
        LeanTween.move(LobbyUI.Instance.ChatContainer, new Vector2(originalPosition.x, originalPosition.y + 900f), LobbyUI.Instance.SlideDuration).setEase(LeanTweenType.easeOutQuart)
            .setOnComplete(() =>
            {
                LobbyUI.Instance.ChatPanel.SetActive(false);
            });
    }

    public void ToggleChatPanel()
    {
        LobbyUI.Instance.ChatPanel.SetActive(!LobbyUI.Instance.ChatPanel.activeSelf);
    }

    private void OnSendButtonClicked(Button button)
    {
        if (!string.IsNullOrEmpty(LobbyUI.Instance.MessageInputField.text))
        {
            SendChatMessage(LobbyUI.Instance.MessageInputField.text);
            LobbyUI.Instance.MessageInputField.text = string.Empty;
        }
    }

    private void SendChatMessage(string message)
    {
        RPC_SendChatMessage(message, Runner.LocalPlayer);
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

    private void AddMessageToChat(string owner, string message)
    {
        GameObject newMessage = Instantiate(chatMessagePrefab, LobbyUI.Instance.MessageContent);
        var messageText = newMessage.transform.Find("message").GetComponent<TextMeshProUGUI>();
        var ownerText = newMessage.transform.Find("owner").GetComponent<TextMeshProUGUI>();

        messageText.text = message;
        ownerText.text = owner;

        messages.Add(newMessage);

        UpdateContentHeight(LobbyUI.Instance.MessageContent, chatMessagePrefab, LobbyUI.Instance.MessageScrollView);

        LayoutRebuilder.ForceRebuildLayoutImmediate(LobbyUI.Instance.MessageContent.GetComponent<RectTransform>());

        newMessage.transform.localScale = new Vector3(1, 0, 1);
        LeanTween.scaleY(newMessage, 1, 0.3f).setEase(LeanTweenType.easeOutQuart);

        Canvas.ForceUpdateCanvases();
        LobbyUI.Instance.MessageScrollView.verticalNormalizedPosition = 0f;
    }

    private void UpdateContentHeight(Transform listParent, GameObject prefab, ScrollRect scrollRect)
    {
        RectTransform contentRectTransform = listParent.GetComponent<RectTransform>();
        float itemHeight = prefab.GetComponent<RectTransform>().rect.height;
        float spacing = listParent.GetComponent<VerticalLayoutGroup>().spacing;
        float totalHeight = (itemHeight + spacing) * listParent.childCount - spacing;

        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, totalHeight);

        Canvas.ForceUpdateCanvases();
    }

    private void PlayWaveAnimation()
    {
        if (chatEffectsEnabled)
        {
            Debug.Log("Playing wave animation for all clients.");
        }
    }

    private void PlayCheerAnimation()
    {
        if (chatEffectsEnabled)
        {
            Debug.Log("Playing cheer animation for all clients.");
        }
    }
}
