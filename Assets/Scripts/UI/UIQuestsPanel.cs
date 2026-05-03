using System.Collections.Generic;
using UnityEngine;

public class UIQuestsPanel : MonoBehaviour, IUIPanel
{
    [SerializeField] private GameObject questElementPrefab;
    [SerializeField] private Transform questsContent;

    private Dictionary<QuestData, UIQuestElement> quests = new();
    public void Initialize()
    {
        RefreshQuests();
        EventBusManager.Instance.OnQuestProgressChanged += QuestProgressChanged;
    }
    public void Uninitialize()
    {
        EventBusManager.Instance.OnQuestProgressChanged -= QuestProgressChanged;
    }

    private void RefreshQuests()
    {
        foreach (var quest in quests.Values)
        {
            if (quest != null) Destroy(quest.gameObject);
        }
        quests.Clear();

        var questDate = QuestManager.Instance.Quests;
        foreach (var quest in questDate)
        {
            var go = Instantiate(questElementPrefab, questsContent);
            var element = go.GetComponent<UIQuestElement>();
            element.SetData(quest);
            quests[quest] = element;
        }
    }
    
    private void QuestProgressChanged(QuestData quest)
    {
        if (quests.TryGetValue(quest, out var element))
        {
            element.UpdateProgress(quest);
        }
    }
}
