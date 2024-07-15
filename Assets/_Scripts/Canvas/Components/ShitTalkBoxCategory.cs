using UnityEngine;
using TMPro;

public class ShitTalkBoxCategory : MonoBehaviour
{
    public TextMeshProUGUI categoryText;

    public void Initialize(string category)
    {
        categoryText.text = category;
    }
}
