using UnityEngine;
using TMPro;

public class TalkBoxCategory : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI categoryText;

    public void Initialize(string category)
    {
        categoryText.text = category;
    }
}
