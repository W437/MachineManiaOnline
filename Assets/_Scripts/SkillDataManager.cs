using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SkillDataManager : MonoBehaviour
{
    private SkillData skillData;

    void Start()
    {
        LoadDataFromJson();
    }

    private void LoadDataFromJson()
    {
        string jsonPath = Path.Combine(Application.persistentDataPath, "SkillData.json");
        if (File.Exists(jsonPath))
        {
            string json = File.ReadAllText(jsonPath);
            SkillDataWrapper dataWrapper = JsonConvert.DeserializeObject<SkillDataWrapper>(json);
            skillData = dataWrapper.SkillData;
            Debug.Log("Data loaded from JSON file.");
        }
        else
        {
            Debug.LogError("JSON file not found.");
        }
    }

    public List<string> GetSkillNames()
    {
        return skillData?.SkillNames ?? new List<string>();
    }

    public List<int> GetDurations()
    {
        return skillData?.Durations ?? new List<int>();
    }

    public List<float> GetDropRates()
    {
        return skillData?.DropRates ?? new List<float>();
    }
}
