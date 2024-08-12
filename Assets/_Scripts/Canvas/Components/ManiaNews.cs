using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManiaNews : MonoBehaviour
{
    public static ManiaNews Instance;

    [SerializeField] private GameObject maniaNewsParent;
    public float _displayDuration = 3f;
    public bool _randomOrder = true;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void InitializeNews()
    {
        if (maniaNewsParent != null)
        {
            maniaNewsParent.SetActive(true);

            foreach (Transform child in maniaNewsParent.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void ShowNews(int index)
    {
        foreach (Transform child in maniaNewsParent.transform)
        {
            child.gameObject.SetActive(false);
        }

        if (index >= 0 && index < maniaNewsParent.transform.childCount)
        {
            maniaNewsParent.transform.GetChild(index).gameObject.SetActive(true);
        }
    }

    public int GetNewsCount()
    {
        return maniaNewsParent.transform.childCount;
    }
}
