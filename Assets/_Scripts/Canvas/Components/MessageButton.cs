using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageButton : ButtonHandler
{
    public TextMeshProUGUI messageText;
    private float buttonHoldTime = 1.5f;
    private bool isHolding = false;
    private float holdTimer = 0f;


    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        AddEventTrigger(button, OnClick);
    }

    public void Initialize(string message)
    {
        messageText.text = message;
    }

    private void OnClick(Button button)
    {

    }
}
