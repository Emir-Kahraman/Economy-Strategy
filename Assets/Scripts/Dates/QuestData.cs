using System;
using UnityEngine;


public enum QuestType
{
    CollectMoney,
    GatherResource,
    ProduceResource,
    BuildBuilding,
    ReachPeriod
}

public abstract class QuestData
{
    [Header("Localization")]
    private const string category = "quests";
    protected const string keyOfMoneyQuest = "Quest_Money";
    protected const string keyOfGatherQuest = "Quest_GatherResource";
    protected const string keyOfProduceQuest = "Quest_ProduceResource";
    protected const string keyOfBuildingQuest = "Quest_Building";
    protected const string keyOfPeriodQuest = "Quest_Period";

    public abstract QuestManagerData.QuestSaveData GetSaveData();
    public abstract void LoadFromSaveData(QuestManagerData.QuestSaveData saveData);

    public abstract QuestType QuestType { get; }
    public abstract bool IsCompleted { get; }
    public abstract void CheckCompletion(object progressData);
    public abstract string GetProgressText();
    public abstract void ResetProgress();
    protected string GetLocalizedText(string key, params object[] args)
    {
        string text = LocalizationManager.Instance.GetText(category, key);
        if (args.Length > 0)
            return string.Format(text, args);
        return text;
    }
}

[Serializable]
public class MoneyQuestData : QuestData
{
    public override QuestManagerData.QuestSaveData GetSaveData()
    {
        return new QuestManagerData.QuestSaveData
        {
            questType = "Money",
            isCompleted = isCompleted,
            targetAmount = targetAmount
        };
    }
    public override void LoadFromSaveData(QuestManagerData.QuestSaveData saveData)
    {
        isCompleted = saveData.isCompleted;
        targetAmount = saveData.targetAmount;
    }

    public readonly QuestType questType = QuestType.CollectMoney;
    public bool isCompleted;
    public int targetAmount;
    public override QuestType QuestType => questType;
    public override bool IsCompleted => isCompleted;

    public override void CheckCompletion(object progressData)
    {
        if (progressData is int currentAmount)
            isCompleted = currentAmount >= targetAmount;
    }    
    public override string GetProgressText()
    {
        int currentMoney = CurrencyManager.Instance.GetCurrentMoney();
        return GetLocalizedText(keyOfMoneyQuest, currentMoney, targetAmount);
    }
    public override void ResetProgress()
    {
       isCompleted = false;
    }
}

[Serializable]
public class GatherResourceQuestData : QuestData
{
    public override QuestManagerData.QuestSaveData GetSaveData()
    {
        return new QuestManagerData.QuestSaveData
        {
            questType = "Gather",
            isCompleted = isCompleted,
            resourceName = resourceData.Key,  // ★ Key, не Name!
            targetAmount = targetAmount
        };
    }
    public override void LoadFromSaveData(QuestManagerData.QuestSaveData saveData)
    {
        isCompleted = saveData.isCompleted;
        targetAmount = saveData.targetAmount;
        resourceData = ResourceLibrary.GetResource(saveData.resourceName);  // ← ResourceLibrary ищет по Key
    }

    public readonly QuestType questType = QuestType.GatherResource;
    public bool isCompleted;
    public ResourceData resourceData;
    public int targetAmount;
    public override QuestType QuestType => questType;
    public override bool IsCompleted => isCompleted;

    public override void CheckCompletion(object progressData)
    {
        if (progressData is int currentAmount)
        {
            isCompleted = currentAmount >= targetAmount;
        }
    }
    
    public override string GetProgressText()
    {
        int currentAmount = StorageManager.Instance.GetResourceCount(resourceData.Type);
        return GetLocalizedText(keyOfGatherQuest, currentAmount, targetAmount, resourceData.GetLocalizedName());
    }
    public override void ResetProgress()
    {
        isCompleted = false;
    }
}

[Serializable]
public class ProduceResourceQuestData : QuestData
{
    public override QuestManagerData.QuestSaveData GetSaveData()
    {
        return new QuestManagerData.QuestSaveData
        {
            questType = "Produce",
            isCompleted = isCompleted,
            resourceName = resourceData.Key,
            targetAmount = targetAmount,
            currentAmount = producedAmount
        };
    }
    public override void LoadFromSaveData(QuestManagerData.QuestSaveData saveData)
    {
        isCompleted = saveData.isCompleted;
        targetAmount = saveData.targetAmount;
        resourceData = ResourceLibrary.GetResource(saveData.resourceName);
        producedAmount = saveData.currentAmount;
    }

    public readonly QuestType questType = QuestType.ProduceResource;
    public bool isCompleted;
    public ResourceData resourceData;
    public int targetAmount;
    private int producedAmount;
    public override QuestType QuestType => questType;
    public override bool IsCompleted => isCompleted;

    public override void CheckCompletion(object progressData)
    {
        if (progressData is int amount)
        {
            producedAmount += amount;
            isCompleted = producedAmount >= targetAmount;
        }
    }
    
    public override string GetProgressText()
    {
        return GetLocalizedText(keyOfProduceQuest, producedAmount, targetAmount, resourceData.GetLocalizedName());
    }
    public override void ResetProgress()
    {
        isCompleted = false;
        producedAmount = 0;
    }
}

[Serializable]
public class BuildingQuestData : QuestData
{
    public override QuestManagerData.QuestSaveData GetSaveData()
    {
        return new QuestManagerData.QuestSaveData
        {
            questType = "Build",
            isCompleted = isCompleted,
            buildingID = buildingData.BuildingKey,
            targetAmount = targetAmount,
            currentAmount = builtAmount
        };
    }
    public override void LoadFromSaveData(QuestManagerData.QuestSaveData saveData)
    {
        isCompleted = saveData.isCompleted;
        targetAmount = saveData.targetAmount;
        buildingData = BuildingLibrary.GetBuilding(saveData.buildingID);
        builtAmount = saveData.currentAmount;
    }

    public readonly QuestType questType = QuestType.BuildBuilding;
    public bool isCompleted;
    public BuildingData buildingData;
    public int targetAmount;
    private int builtAmount;
    public override QuestType QuestType => questType;
    public override bool IsCompleted => isCompleted;

    public override void CheckCompletion(object progressData)
    {
        if (progressData is BuildingData building)
        {
            if (building == buildingData)
            {
                builtAmount += 1;
                isCompleted = builtAmount >= targetAmount;
            }
        }
    }

    public override string GetProgressText()
    {
        return GetLocalizedText(keyOfBuildingQuest, builtAmount, targetAmount, buildingData.GetLocalizedName());
    }
    public override void ResetProgress()
    {
        isCompleted = false;
        builtAmount = 0;
    }
}

[Serializable]
public class PeriodQuestData : QuestData
{
    public override QuestManagerData.QuestSaveData GetSaveData()
    {
        return new QuestManagerData.QuestSaveData
        {
            questType = "Period",
            isCompleted = isCompleted,
            targetPeriod = targetPeriod.ToString()
        };
    }
    public override void LoadFromSaveData(QuestManagerData.QuestSaveData saveData)
    {
        isCompleted = saveData.isCompleted;
        targetPeriod = (Period)Enum.Parse(typeof(Period), saveData.targetPeriod);
    }

    public readonly QuestType questType = QuestType.ReachPeriod;
    public bool isCompleted;
    public Period targetPeriod;
    public override QuestType QuestType => questType;
    public override bool IsCompleted => isCompleted;

    public override void CheckCompletion(object progressData)
    {
        if (progressData is Period period)
            isCompleted = period >= targetPeriod;
    }

    public override string GetProgressText()
    {
        Period period = OrdersManager.Instance.GetPeriod;
        return GetLocalizedText(keyOfPeriodQuest, period, targetPeriod);
    }
    public override void ResetProgress()
    {
        isCompleted = false;
    }
}
