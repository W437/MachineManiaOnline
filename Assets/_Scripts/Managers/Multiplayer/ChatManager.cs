using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;
using UnityEngine.UIElements;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance;

    public GameObject chatMessagePrefab;
    private List<GameObject> messages = new List<GameObject>();
    private Dictionary<string, System.Action> specialMessages = new Dictionary<string, System.Action>();

    public bool chatEffectsEnabled = true;

    private void Awake()
    {
        // Singleton pattern
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
        UILobby.Instance.sendButton.onClick.AddListener(OnSendButtonClicked);
        UILobby.Instance.BGExitButton.onClick.AddListener(ToggleChat);
        UILobby.Instance.chatPanel.SetActive(false);

        UILobby.Instance.exitButton.onClick.AddListener(HideChat);

        specialMessages["/wave"] = PlayWaveAnimation;
        specialMessages["/cheer"] = PlayCheerAnimation;
    }

    public void ToggleChat()
    {
        if (UILobby.Instance.isChatVisible)
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
        UILobby.Instance.isChatVisible = true;

        // Enable the chat panel
        UILobby.Instance.chatPanel.SetActive(true);

        // Fade in the background
        CanvasGroup bgCanvasGroup = UILobby.Instance.chatBG.GetComponent<CanvasGroup>();
        CanvasGroup chatCanvasGroup = UILobby.Instance.chatContainer.GetComponent<CanvasGroup>();

        bgCanvasGroup.alpha = 0f;
        chatCanvasGroup.alpha = 0f;

        LeanTween.alphaCanvas(bgCanvasGroup, 1f, UILobby.Instance.fadeDuration);
        LeanTween.alphaCanvas(chatCanvasGroup, 1f, UILobby.Instance.fadeDuration);

        // Slide in the ChatContainer from above
        Vector2 originalPosition = UILobby.Instance.chatContainer.position;

        UILobby.Instance.chatContainer.anchoredPosition = new Vector3(0, 300, 0);

        LeanTween.move(UILobby.Instance.chatContainer, originalPosition, UILobby.Instance.slideDuration).setEase(LeanTweenType.easeOutQuart);
    }

    public void HideChat()
    {
        UILobby.Instance.isChatVisible = false;
         
        // Fade out the background after the chat box is hidden
        CanvasGroup bgCanvasGroup = UILobby.Instance.chatBG.GetComponent<CanvasGroup>();
        CanvasGroup chatCanvasGroup = UILobby.Instance.chatContainer.GetComponent<CanvasGroup>();

        LeanTween.alphaCanvas(bgCanvasGroup, 0f, UILobby.Instance.fadeDuration);
        LeanTween.alphaCanvas(chatCanvasGroup, 0f, UILobby.Instance.fadeDuration);

        Vector2 originalPosition = UILobby.Instance.chatContainer.anchoredPosition;
        LeanTween.move(UILobby.Instance.chatContainer, new Vector2(originalPosition.x, originalPosition.y + 900f), UILobby.Instance.slideDuration).setEase(LeanTweenType.easeOutQuart)
            .setOnComplete(() =>
            {
                UILobby.Instance.chatPanel.SetActive(false);
            });
    }

    public void ToggleChatPanel()
    {
        UILobby.Instance.chatPanel.SetActive(!UILobby.Instance.chatPanel.activeSelf);
    }

    private void OnSendButtonClicked()
    {
        if (!string.IsNullOrEmpty(UILobby.Instance.messageInputField.text))
        {
            SendChatMessage(UILobby.Instance.messageInputField.text);
            UILobby.Instance.messageInputField.text = string.Empty;
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
        GameObject newMessage = Instantiate(chatMessagePrefab, UILobby.Instance.messageContent);
        var messageText = newMessage.transform.Find("message").GetComponent<TextMeshProUGUI>();
        var ownerText = newMessage.transform.Find("owner").GetComponent<TextMeshProUGUI>();

        messageText.text = message;
        ownerText.text = owner;

        messages.Add(newMessage);

        UpdateContentHeight(UILobby.Instance.messageContent, chatMessagePrefab, UILobby.Instance.messageScrollView);

        LayoutRebuilder.ForceRebuildLayoutImmediate(UILobby.Instance.messageContent.GetComponent<RectTransform>());

        newMessage.transform.localScale = new Vector3(1, 0, 1);
        LeanTween.scaleY(newMessage, 1, 0.3f).setEase(LeanTweenType.easeOutQuart);

        Canvas.ForceUpdateCanvases();
        UILobby.Instance.messageScrollView.verticalNormalizedPosition = 0f;
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
