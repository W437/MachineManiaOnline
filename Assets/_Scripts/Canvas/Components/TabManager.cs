using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    [Header("Tabs Configuration")]
    public Button generalButton;
    public Button otherButton;
    public Button infoButton;
    public GameObject[] contentContainers;

    public Color activeTabColor = new Color(0.255f, 0.545f, 0.875f); // Hex 418BDF
    public Color inactiveTabColor = new Color(0.082f, 0.365f, 0.682f); // Hex 155DAE

    public Color activeTextColor = Color.white; // Hex FFFFFF
    public Color inactiveTextColor = new Color(0.071f, 0.212f, 0.369f); // Hex 12365E

    int currentTabIndex = 0;

    ButtonHandler buttonHandler;

    void Start()
    {
        buttonHandler = gameObject.GetComponent<ButtonHandler>();
        InitializeTabs();
    }

    void InitializeTabs()
    {
        var config = new ButtonConfig(
            rotationLock: true,
            customAnimation: true,
            returnTime: 0f,
            yOffset: 0,
            rotation: new Vector3(40, 0, 0)
        );

        buttonHandler.AddButtonEventTrigger(generalButton, (button) => OnTabSelected(0), config);

        buttonHandler.AddButtonEventTrigger(otherButton, (button) => OnTabSelected(1), config);

        buttonHandler.AddButtonEventTrigger(infoButton, (button) => OnTabSelected(2), config);

        SetTabColor(0, currentTabIndex == 0);
        SetTabColor(1, currentTabIndex == 1);
        SetTabColor(2, currentTabIndex == 2);

        UpdateContentVisibility();
    }

    void OnTabSelected(int index)
    {
        if (index == currentTabIndex) return;

        SetTabColor(currentTabIndex, false);
        currentTabIndex = index;
        SetTabColor(currentTabIndex, true);

        UpdateContentVisibility();
    }

    void UpdateContentVisibility()
    {
        for (int i = 0; i < contentContainers.Length; i++)
        {
            contentContainers[i].SetActive(i == currentTabIndex);
        }
    }

    void SetTabColor(int index, bool isActive)
    {
        Button button = null;
        switch (index)
        {
            case 0:
                button = generalButton;
                break;
            case 1:
                button = otherButton;
                break;
            case 2:
                button = infoButton;
                break;
        }

        if (button != null)
        {
            button.GetComponent<Image>().color = isActive ? activeTabColor : inactiveTabColor;

            TextMeshProUGUI textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.color = isActive ? activeTextColor : inactiveTextColor;
            }
        }
    }
}
