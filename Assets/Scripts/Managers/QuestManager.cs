using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

[Serializable]
public class QuestProgressState
{
    public bool isCompleted;
    public int currentAmount;
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    [SerializeField] private CurrentLevelRuntimeData currentLevelRuntimeData;
    private List<QuestData> quests;
    private bool isRandomGenerateLevel;

    private bool allQuestsCompleted = false;

    public List<QuestData> Quests { get { return quests; } }

    public QuestManagerData GetQuestManagerData()
    {
        var data = new QuestManagerData();
        data.quests = new();

        foreach (var quest in quests)
        {
            data.quests.Add(quest.GetSaveData());
        }
        return data;
    }
    private void Awake()
    {
        Initialize();
    }
    private void OnDestroy()
    {
        Uninitialize();
    }

    private void Initialize()
    {
        InitializeSingleton();
        InitializeQuests();
        InitializeEvents();
        IsLevelLoadFromSave();
    }
    private void Uninitialize()
    {
        UninitializeQuests();
        UninitializeEvents();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameObject.name = "QuestManager";
    }
    private void InitializeQuests()
    {
        if (SceneManager.GetActiveScene().name == "Random_Level") isRandomGenerateLevel = true;
        else isRandomGenerateLevel = false;

        if (currentLevelRuntimeData == null || currentLevelRuntimeData.levelData == null)
        {
            Debug.LogError("CurrentLevelRuntimeData or LevelData is null.");
            return;
        }
        else
        {
            quests = new List<QuestData>(currentLevelRuntimeData.levelData.allQuests);
        }

        foreach (var quest in quests)
        {
            quest.ResetProgress();
        }
    }
    private void UninitializeQuests()
    {
        quests.Clear();
        currentLevelRuntimeData.Reset();
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnResourceAmountUpdated += CheckProduceResourceQuest;
        EventBusManager.Instance.OnBuildingBuilt += CheckBuildingQuest;
        EventBusManager.Instance.OnResourceBuilt += CheckBuildingQuest;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnResourceAmountUpdated -= CheckProduceResourceQuest;
        EventBusManager.Instance.OnBuildingBuilt -= CheckBuildingQuest;
        EventBusManager.Instance.OnResourceBuilt -= CheckBuildingQuest;
    }
    private void IsLevelLoadFromSave()
    {
        if (SaveManager.Instance.IsLoadLevelFromSave)
        {
            LoadQuestManagerData(SaveManager.Instance.LoadedLevelDates);
        }
    }
    private void LoadQuestManagerData(GameSessionData gameSessionData)
    {
        QuestManagerData data = gameSessionData.questManagerData;
        quests.Clear();
        foreach (var saveData in data.quests)
        {
            QuestData quest = LoadQuestFromSaveData(saveData);
            if (quest != null)
            {
                quests.Add(quest);
            }
        }
    }
    private QuestData LoadQuestFromSaveData(QuestManagerData.QuestSaveData saveData)
    {
        switch (saveData.questType)
        {
            case "Money": var m = new MoneyQuestData(); m.LoadFromSaveData(saveData); return m;
            case "Gather": var g = new GatherResourceQuestData(); g.LoadFromSaveData(saveData); return g;
            case "Produce": var p = new ProduceResourceQuestData(); p.LoadFromSaveData(saveData); return p;
            case "Period": var per = new PeriodQuestData(); per.LoadFromSaveData(saveData); return per;
            case "Build": var b = new BuildingQuestData(); b.LoadFromSaveData(saveData); return b;
            default: Debug.LogError($"Unknown quest type in save data: {saveData.questType}"); return null;
        }
    }

    private void Update()
    {
        if (isRandomGenerateLevel || allQuestsCompleted) return;
        CheckQuestsProgress();
        CheckQuestsCompleted();
    }

    private void CheckQuestsProgress()
    {
        foreach (var quest in quests)
        {
            if (quest.IsCompleted)
                continue;
            
            switch (quest.QuestType)
            {
                case QuestType.CollectMoney: quest.CheckCompletion(CurrencyManager.Instance.GetCurrentMoney()); EventBusManager.Instance.QuestProgressChanged(quest); break;
                case QuestType.GatherResource: var rqd = quest as GatherResourceQuestData; quest.CheckCompletion(StorageManager.Instance.GetResourceCount(rqd.resourceData.Type)); EventBusManager.Instance.QuestProgressChanged(quest); break;
                case QuestType.ReachPeriod: quest.CheckCompletion(OrdersManager.Instance.GetPeriod); EventBusManager.Instance.QuestProgressChanged(quest); break;
                default: break;
            }
        }
    }
    private void CheckProduceResourceQuest(ResourceType resource, int amount)
    {
        foreach (var quest in quests.Where(q => q.QuestType == QuestType.ProduceResource))
        {
            if (quest.IsCompleted)
                continue;

            var prqd = quest as ProduceResourceQuestData;
            if (prqd.resourceData.Type == resource)
            {
                quest.CheckCompletion(amount);
                EventBusManager.Instance.QuestProgressChanged(quest);
            }
        }
    }
    private void CheckBuildingQuest(Vector3Int position, BuildingData buildingData)//position не используется, костыль
    {
        foreach (var quest in quests.Where(q => q.QuestType == QuestType.BuildBuilding))
        {
            if (quest.IsCompleted)
                continue;

            quest.CheckCompletion(buildingData);
            EventBusManager.Instance.QuestProgressChanged(quest);
        }
    }

    private void CheckQuestsCompleted()
    {
        foreach (var quest in quests)
        {
            if (!quest.IsCompleted)
                return;
        }
        allQuestsCompleted = true;
        EventBusManager.Instance.AllQuestsCompleted(currentLevelRuntimeData.levelData.index);//UI
    }
}
