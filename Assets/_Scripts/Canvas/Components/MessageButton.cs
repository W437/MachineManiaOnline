using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageButton : ButtonHandler
{
    [SerializeField] TextMeshProUGUI _messageText;

    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Initialize(string message)
    {
        _messageText.text = message;
    }
}
