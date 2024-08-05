using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

public class SkillDataManager : MonoBehaviour
{
    private SkillData skillData;

    void Start()
    {
        LoadDataFromJson();
        LogLoadedData();
    }

    private void LoadDataFromJson()
    {
        string jsonPath = @"C:\Users\angry\Desktop\Studies\Unity\New Unity Projects\MachineManiaOnline\SkillData.json";

        if (File.Exists(jsonPath))
        {
            try
            {
                string json = File.ReadAllText(jsonPath);
                SkillDataWrapper dataWrapper = JsonConvert.DeserializeObject<SkillDataWrapper>(json);
                skillData = dataWrapper.SkillData;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load data from JSON file: {ex.Message}");
                skillData = null;
            }
        }
        else
        {
            Debug.LogError($"JSON file not found at: {jsonPath}");
            skillData = null;
        }

        if (skillData == null)
        {
            skillData = new SkillData
            {
                SkillNames = new List<string>(),
                Durations = new List<int>(),
                DropRates = new List<float>()
            };
        }
    }

    private void LogLoadedData()
    {
        if (skillData == null)
        {
            Debug.LogError("Skill data is null.");
            return;
        }

        if (skillData.SkillNames != null)
        {
            foreach (var name in skillData.SkillNames)
            {
                Debug.Log(name);
            }
        }
        else
        {
            Debug.LogError("SkillNames list is null.");
        }

        if (skillData.Durations != null)
        {
            foreach (var duration in skillData.Durations)
            {
                Debug.Log(duration);
            }
        }
        else
        {
            Debug.LogError("Durations list is null.");
        }

        if (skillData.DropRates != null)
        {
            foreach (var dropRate in skillData.DropRates)
            {
                Debug.Log(dropRate);
            }
        }
        else
        {
            Debug.LogError("DropRates list is null.");
        }
    }

    public string GetSkillName(int index)
    {
        if (skillData == null || skillData.SkillNames == null)
        {
            Debug.LogError("Skill data or SkillNames list is null.");
            return null;
        }

        if (index >= 0 && index < skillData.SkillNames.Count)
            return skillData.SkillNames[index];
        else
            throw new System.IndexOutOfRangeException("Index out of range for SkillNames list.");
    }

    public int GetDuration(int index)
    {
        if (skillData == null || skillData.Durations == null)
        {
            Debug.LogError("Skill data or Durations list is null.");
            return 0;
        }

        if (index >= 0 && index < skillData.Durations.Count)
            return skillData.Durations[index];
        else
            throw new System.IndexOutOfRangeException("Index out of range for Durations list.");
    }

    public float GetDropRate(int index)
    {
        if (skillData == null || skillData.DropRates == null)
        {
            Debug.LogError("Skill data or DropRates list is null.");
            return 0f;
        }

        if (index >= 0 && index < skillData.DropRates.Count)
            return skillData.DropRates[index];
        else
            throw new System.IndexOutOfRangeException("Index out of range for DropRates list.");
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
