using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CurrentLevelRuntimeData", menuName = "Game/CurrentLevelRuntimeData", order = 1)]
public class CurrentLevelRuntimeData : ScriptableObject
{
    public bool isLoadLevelFromSave = false;
    public LevelData levelData;
    public List<QuestData> currentQuests = new();
    public WorldGenerateParameters worldGenerateParameters;
    public int worldSeed = 0;

    public GameSessionMeta GetGameSessionMeta()
    {
        return new GameSessionMeta
        {
            index = levelData.index,
            sceneName = levelData.sceneName
        };
    }
    public void Reset()
    {
        currentQuests.Clear();
    }

    public void PrepareForNewLevel(LevelData nextLevelData)
    {
        levelData = nextLevelData;
        currentQuests.Clear();
        currentQuests.AddRange(levelData.allQuests);
        isLoadLevelFromSave = false;
    }
}
