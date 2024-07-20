using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;
using UnityEngine.UIElements;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance;

    [SerializeField] private GameObject chatMessagePrefab;
    private List<GameObject> messages = new List<GameObject>();
    private Dictionary<string, System.Action> specialMessages = new Dictionary<string, System.Action>();

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
        UILobby.Instance.SendMessageButton.onClick.AddListener(OnSendButtonClicked);
        UILobby.Instance.BGExitButton.onClick.AddListener(ToggleChat);
        UILobby.Instance.ChatExitButton.onClick.AddListener(HideChat);
        UILobby.Instance.ChatPanel.SetActive(false);

        specialMessages["/wave"] = PlayWaveAnimation;
        specialMessages["/cheer"] = PlayCheerAnimation;
    }

    public void ToggleChat()
    {
        if (UILobby.Instance.ChatVisible)
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
        UILobby.Instance.ChatVisible = true;

        // Enable the chat panel
        UILobby.Instance.ChatPanel.SetActive(true);

        // Fade in the background
        CanvasGroup bgCanvasGroup = UILobby.Instance.ChatBG.GetComponent<CanvasGroup>();
        CanvasGroup chatCanvasGroup = UILobby.Instance.ChatContainer.GetComponent<CanvasGroup>();

        bgCanvasGroup.alpha = 0f;
        chatCanvasGroup.alpha = 0f;

        LeanTween.alphaCanvas(bgCanvasGroup, 1f, UILobby.Instance.FadeDuration);
        LeanTween.alphaCanvas(chatCanvasGroup, 1f, UILobby.Instance.FadeDuration);

        // Slide in the ChatContainer from above
        Vector2 originalPosition = UILobby.Instance.ChatContainer.position;

        UILobby.Instance.ChatContainer.anchoredPosition = new Vector3(0, 300, 0);

        LeanTween.move(UILobby.Instance.ChatContainer, originalPosition, UILobby.Instance.SlideDuration).setEase(LeanTweenType.easeOutQuart);
    }

    public void HideChat()
    {
        UILobby.Instance.ChatVisible = false;
         
        // Fade out the background after the chat box is hidden
        CanvasGroup bgCanvasGroup = UILobby.Instance.ChatBG.GetComponent<CanvasGroup>();
        CanvasGroup chatCanvasGroup = UILobby.Instance.ChatContainer.GetComponent<CanvasGroup>();

        LeanTween.alphaCanvas(bgCanvasGroup, 0f, UILobby.Instance.FadeDuration);
        LeanTween.alphaCanvas(chatCanvasGroup, 0f, UILobby.Instance.FadeDuration);

        Vector2 originalPosition = UILobby.Instance.ChatContainer.anchoredPosition;
        LeanTween.move(UILobby.Instance.ChatContainer, new Vector2(originalPosition.x, originalPosition.y + 900f), UILobby.Instance.SlideDuration).setEase(LeanTweenType.easeOutQuart)
            .setOnComplete(() =>
            {
                UILobby.Instance.ChatPanel.SetActive(false);
            });
    }

    public void ToggleChatPanel()
    {
        UILobby.Instance.ChatPanel.SetActive(!UILobby.Instance.ChatPanel.activeSelf);
    }

    private void OnSendButtonClicked()
    {
        if (!string.IsNullOrEmpty(UILobby.Instance.MessageInputField.text))
        {
            SendChatMessage(UILobby.Instance.MessageInputField.text);
            UILobby.Instance.MessageInputField.text = string.Empty;
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
        GameObject newMessage = Instantiate(chatMessagePrefab, UILobby.Instance.MessageContent);
        var messageText = newMessage.transform.Find("message").GetComponent<TextMeshProUGUI>();
        var ownerText = newMessage.transform.Find("owner").GetComponent<TextMeshProUGUI>();

        messageText.text = message;
        ownerText.text = owner;

        messages.Add(newMessage);

        UpdateContentHeight(UILobby.Instance.MessageContent, chatMessagePrefab, UILobby.Instance.MessageScrollView);

        LayoutRebuilder.ForceRebuildLayoutImmediate(UILobby.Instance.MessageContent.GetComponent<RectTransform>());

        newMessage.transform.localScale = new Vector3(1, 0, 1);
        LeanTween.scaleY(newMessage, 1, 0.3f).setEase(LeanTweenType.easeOutQuart);

        Canvas.ForceUpdateCanvases();
        UILobby.Instance.MessageScrollView.verticalNormalizedPosition = 0f;
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
