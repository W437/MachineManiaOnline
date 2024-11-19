using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoadingUIReferenceHolder : MonoBehaviour
{
    [Header("Loading UI Elements")]
    public Slider loadingBar;
    public TextMeshProUGUI loadingText;
    public GameObject firstLaunchContainer;
    public CanvasGroup loadingCanvasGroup;
    public TextMeshProUGUI welcomeText;
    public TMP_InputField usernameInput;
    public Button continueButton;

    // You can add other UI elements here if needed for other systems
}
