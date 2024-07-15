using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSystem", menuName = "Game/LevelSystem")]
public class LevelSystem : ScriptableObject
{
    public List<PlayerProgressLevelData> levels;

    public int GetRequiredXPForLevel(int level)
    {
        foreach (var levelData in levels)
        {
            if (levelData.levelNumber == level)
            {
                return levelData.requiredXP;
            }
        }
        return 0;
    }

    public int GetMaxLevel()
    {
        if (levels.Count > 0)
        {
            return levels[levels.Count - 1].levelNumber;
        }
        return 1;
    }
}
