using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private List<GameObject> levels;
    private GameObject currentLevel;

    public Transform startLine;
    public Transform finishLine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadLevel(int index)
    {
        if (index < 0 || index >= levels.Count)
        {
            Debug.LogError("Invalid level index.");
            return;
        }

        if (currentLevel != null)
        {
            Destroy(currentLevel);
        }

        currentLevel = Instantiate(levels[index], Vector3.zero, Quaternion.identity);
        UpdateLevelReferences();
    }

    private void UpdateLevelReferences()
    {
        startLine = currentLevel.transform.Find("StartLine");
        finishLine = currentLevel.transform.Find("FinishLine");

        if (startLine == null || finishLine == null)
        {
            Debug.LogError("StartLine or FinishLine not found in the level prefab.");
        }
    }

    public void GetLevelStats()
    {
        //  get stats for the level, such as star ranking or points
    }
}
