using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Game/Level Database")]
public class LevelDatabase : ScriptableObject
{
    public List<LevelData> levels = new();

    public List<LevelData> GetLevelsByDifficulty(LevelDifficulty difficulty)
    {
        return levels.FindAll(level => level.difficulty == difficulty);
    }
}
