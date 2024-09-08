using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Settings")]
    [SerializeField] List<GameObject> levelPrefabs;
    [SerializeField] Transform levelParent;
    [SerializeField] float startPositionSpacing = 5.0f;

    GameObject currentLevel;
    Transform startLine;
    Transform finishLine;

    void Awake()
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

    public void LoadRandomLevel()
    {
        if (currentLevel != null)
        {
            Destroy(currentLevel);
        }

        int randomIndex = Random.Range(0, levelPrefabs.Count);
        currentLevel = Instantiate(levelPrefabs[randomIndex], levelParent);
        Debug.Log($"Loaded Level: {currentLevel.name}");

        // Retrieve start and finish lines
        startLine = currentLevel.transform.Find("Start Line");
        finishLine = currentLevel.transform.Find("Finish Line");
    }

    public Transform GetStartLine()
    {
        return startLine;
    }

    public Transform GetFinishLine()
    {
        return finishLine;
    }

    public void PositionPlayers(List<Transform> players)
    {
        if (startLine == null)
        {
            Debug.LogError("Start Line not set! Make sure the level is loaded properly.");
            return;
        }

        Vector3 startPosition = startLine.position;
        for (int i = 0; i < players.Count; i++)
        {
            players[i].position = startPosition - new Vector3(i * startPositionSpacing, 0, 0);
        }
    }
}
