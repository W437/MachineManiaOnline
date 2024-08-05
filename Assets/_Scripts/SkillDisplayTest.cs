using UnityEngine;

public class SkillDisplayTest : MonoBehaviour
{
    public SkillDataManager skillDataManager;

    void Start()
    {
        // Ensure skillDataManager is assigned in the Inspector or found in the scene
        if (skillDataManager == null)
        {
            skillDataManager = FindObjectOfType<SkillDataManager>();
        }

        if (skillDataManager != null)
        {
            try
            {
                // Example of retrieving specific values from lists
                int skillIndex = 1; // Index starts from 0
                string skillName = skillDataManager.GetSkillName(skillIndex);
                int duration = skillDataManager.GetDuration(skillIndex);
                float dropRate = skillDataManager.GetDropRate(skillIndex);

                Debug.Log($"Skill Name: {skillName}, Duration: {duration}, Drop Rate: {dropRate}");
            }
            catch (System.IndexOutOfRangeException ex)
            {
                Debug.LogError(ex.Message);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Unexpected error: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("SkillDataManager not found.");
        }
    }
}
