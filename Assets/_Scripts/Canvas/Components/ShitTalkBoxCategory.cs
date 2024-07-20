using UnityEngine;
using TMPro;

public class ShitTalkBoxCategory : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI categoryText;

    public void Initialize(string category)
    {
        categoryText.text = category;
    }
}
