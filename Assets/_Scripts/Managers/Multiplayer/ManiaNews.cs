using System.Collections.Generic;
using System;
using UnityEngine;

public class ManiaNews
{
    public event Action<int> OnNewsChanged;
    List<GameObject> newsPrefabs = new List<GameObject>();
    Transform newsParent;
    GameObject currentNewsPrefab;
    int lastIndex = -1;
    float displayTime = 3.5f;
    bool isTransitioning = false;

    const string FOLDER_PATH = "News";

    // konstructor
    public ManiaNews(Transform parent, float displayTime)
    {
        this.newsParent = parent;
        this.displayTime = displayTime;
        LoadNewsPrefabs();
    }

    public int GetNewsPrefabsCount()
    {
        return newsPrefabs != null ? newsPrefabs.Count : 0;
    }

    public bool IsTransitioning()
    {
        return isTransitioning;
    }

    public void ShowSpecificNews(int newsIndex)
    {
        if (newsPrefabs.Count == 0 || newsIndex < 0 || newsIndex >= newsPrefabs.Count)
        {
            Debug.LogError("Invalid news index.");
            return;
        }

        if (currentNewsPrefab != null)
        {
            currentNewsPrefab.SetActive(false);
        }

        currentNewsPrefab = GameObject.Instantiate(newsPrefabs[newsIndex], newsParent);
        currentNewsPrefab.SetActive(true);
        currentNewsPrefab.transform.localScale = Vector3.zero;

        LeanTween.scale(currentNewsPrefab, Vector3.one, 0.3f).setEase(LeanTweenType.easeOutBack);

        //OnNewsChanged?.Invoke(newsIndex);

        LeanTween.delayedCall(displayTime, () =>
        {
            HideCurrentNews(() =>
            {
                OnNewsChanged?.Invoke(-1);
            });
        });
    }

    void HideCurrentNews(Action onComplete)
    {
        if (currentNewsPrefab != null)
        {
            LeanTween.scale(currentNewsPrefab, Vector3.zero, 0.2f).setEase(LeanTweenType.easeInBack)
                .setOnComplete(() =>
                {
                    GameObject.Destroy(currentNewsPrefab);
                    currentNewsPrefab = null;
                    isTransitioning = false;
                    onComplete?.Invoke();
                });
        }
    }
    
    void LoadNewsPrefabs()
    {
        var loadedPrefabs = Resources.LoadAll<GameObject>(FOLDER_PATH);
        newsPrefabs = new List<GameObject>(loadedPrefabs);

        if (newsPrefabs == null || newsPrefabs.Count == 0)
        {
            Debug.LogError($"No news prefabs found in folder: {FOLDER_PATH}");
        }
    }
}
