using UnityEngine;
using TMPro;

public class TalkBoxCategory : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI categoryText;

    public void Initialize(string category)
    {
        categoryText.text = category;
    }
}
