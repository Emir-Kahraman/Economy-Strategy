using System.Collections.Generic;
using UnityEngine;
using System;

public enum LevelDifficulty
{
    Easy,
    Medium,
    Hard,
    Expert,
    None
}
[Serializable]
public class LevelData
{
    public int index;
    public string sceneName;
    public LevelDifficulty difficulty;
    public GameObject levelPrefab;
    public bool isUnlocked = false;

    [Header("Level Parameters")]
    public int startingMoney;
    public List<StartResources> startResources;
    public PeriodParameters periodParameters;
    public List<MoneyQuestData> moneyQuests = new();
    public List<GatherResourceQuestData> gatherResourceQuests = new();
    public List<ProduceResourceQuestData> produceResourceQuests = new();
    public List<BuildingQuestData> buildingQuests = new();
    public List<PeriodQuestData> periodQuests = new();
    [Header("Random Level Indexes Prefabs")]
    public List<int> startMoneyPrefabs = new();
    public List<Period> startPeriodPrefabs = new();
    public List<QuestData> allQuests
    {
        get
        {
            List<QuestData> quests = new();
            quests.AddRange(moneyQuests);
            quests.AddRange(gatherResourceQuests);
            quests.AddRange(produceResourceQuests);
            quests.AddRange(buildingQuests);
            quests.AddRange(periodQuests);
            return quests;
        }
    }

    public void SetParameters(int startMoneyIndex, int startPeriodIndex)
    {
        startingMoney = startMoneyPrefabs[startMoneyIndex];
        periodParameters.startPeriod = startPeriodPrefabs[startPeriodIndex];
        periodParameters.endPeriod = Period.IV; //Для случайно генерируемых уровней, для остальных уровней эти значения будут перезаписаны в инспекторе
        periodParameters.timerToIIPeriod = 60; //Здесь должны быть базовые значения, для случайно генерируемых уровней, для остальных уровней эти значения будут перезаписаны в инспекторе
        periodParameters.timerToIIIPeriod = 120;
        periodParameters.timerToIVPeriod = 180;
    }
}
[Serializable]
public class StartResources
{
    public ResourceType type;
    public int amount;
}
[Serializable]
public class PeriodParameters
{
    public Period startPeriod;
    public Period endPeriod;
    public int timerToIIPeriod;
    public int timerToIIIPeriod;
    public int timerToIVPeriod;
}
