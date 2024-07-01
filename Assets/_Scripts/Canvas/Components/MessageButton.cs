using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageButton : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    private float buttonHoldTime = 1.5f;
    private bool isHolding = false;
    private float holdTimer = 0f;


    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    public void Initialize(string message)
    {
        messageText.text = message;
    }

    private void OnButtonClick()
    {

    }
}
