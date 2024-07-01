using UnityEngine;
using TMPro;

public class CategoryHeader : MonoBehaviour
{
    public TextMeshProUGUI categoryText;

    public void Initialize(string category)
    {
        categoryText.text = category;
    }
}
