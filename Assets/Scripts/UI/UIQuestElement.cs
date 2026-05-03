using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestElement : MonoBehaviour //Теперь переходим к языкам, нужно создать основу, сами же языки позже.
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI description;
    [Space, Header("Icons for special quest type")]

    [SerializeField] private Sprite moneyIcon;
    [SerializeField] private Sprite[] periodIcons = new Sprite[4];

    public void SetData(QuestData data)
    {
        iconImage.sprite = GetIconForQuest(data);
        description.text = data.GetProgressText();
        UpdateVisual(data);
    }

    private Sprite GetIconForQuest(QuestData data)
    {
        switch (data.QuestType)
        {
            case QuestType.CollectMoney: return moneyIcon;
            case QuestType.GatherResource:var gatherQuest = data as GatherResourceQuestData; return gatherQuest.resourceData.Icon;
            case QuestType.ProduceResource: var produceQuest = data as ProduceResourceQuestData; return produceQuest.resourceData.Icon;
            case QuestType.BuildBuilding: var buildingQuest = data as BuildingQuestData; return buildingQuest.buildingData.icon;
            case QuestType.ReachPeriod: return GetReachPeriodIcon(data as PeriodQuestData);
            default: return null;
        }
    }
    private Sprite GetReachPeriodIcon(PeriodQuestData periodQuest)
    {
        switch (periodQuest.targetPeriod)
        {
            case Period.I: return periodIcons[0];
            case Period.II: return periodIcons[1];
            case Period.III: return periodIcons[2];
            case Period.IV: return periodIcons[3];
            default: return null;
        }
    }

    public void UpdateProgress(QuestData data)
    {
        UpdateVisual(data);
    }
    private void UpdateVisual(QuestData data)
    {
        string text = data.GetProgressText();

        if (data.IsCompleted)
        {
            description.text = $"<s>{text}</s>";
            description.color = Color.gray;
        }
        else
        {
            description.text = text;
            description.color = Color.white;
        }
    }
}
