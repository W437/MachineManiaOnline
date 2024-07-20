using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageButton : ButtonHandler
{
    [SerializeField] private TextMeshProUGUI _messageText;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Initialize(string message)
    {
        _messageText.text = message;
    }
}
