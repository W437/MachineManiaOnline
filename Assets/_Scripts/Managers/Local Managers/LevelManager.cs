using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Settings")]
    [SerializeField] private List<GameObject> levelPrefabs; // List of level prefabs
    [SerializeField] private Transform levelParent; // Parent transform for instantiated levels
    [SerializeField] private float playerSpacing = 5.0f; // Distance between players at the start line

    private GameObject currentLevel;
    private Transform startLine;
    private Transform finishLine;

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
            players[i].position = startPosition - new Vector3(i * playerSpacing, 0, 0);
        }
    }
}
