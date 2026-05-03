using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Transactions;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UILevelSelectionController : MonoBehaviour
{
    [Header("Level Runtime Data")]
    [SerializeField] private CurrentLevelRuntimeData currentLevelData;
    [Header("Level Selection UI Elements"), Space]
    [SerializeField] private LevelDatabase levelDatabase;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform easyPanel;
    [SerializeField] private Transform mediumPanel;
    [SerializeField] private Transform hardPanel;
    [SerializeField] private Transform expertPanel;
    [SerializeField] private Color lockedColor;
    [SerializeField] private Color openedColor;
    [SerializeField] private Color completedColor;

    private Dictionary<LevelDifficulty, Transform> panelMap = new();
    private List<UILevelButton> levelButtons = new();

    private void OnEnable()
    {
        UpdateLevelsStatus();
    }

    private void UpdateLevelsStatus()
    {
        foreach (var levelButton in levelButtons)
        {            
            levelButton.InteractiveStatus(ProgressManager.Instance.IsLevelUnlocked(levelButton.Data.index), GetLevelStatusColor(levelButton.Data.index));
        }
    }

    private void Start()
    {
        CreatePanelMaps();
        CreateLevelButtons();
    }

    private void CreatePanelMaps()
    {
        panelMap = new Dictionary<LevelDifficulty, Transform>
        {
            { LevelDifficulty.Easy, easyPanel },
            { LevelDifficulty.Medium, mediumPanel },
            { LevelDifficulty.Hard, hardPanel },
            { LevelDifficulty.Expert, expertPanel }
        };
    }
    private void CreateLevelButtons()
    {
        foreach (var levelData in levelDatabase.levels)
        {
            Transform parentPanel = panelMap[levelData.difficulty];
            GameObject buttonGO = Instantiate(buttonPrefab, parentPanel);

            UILevelButton levelButton = buttonGO.GetComponent<UILevelButton>();
            if (levelButton != null)
            {
                levelButton.Initialize(levelData, this);
                levelButton.InteractiveStatus(ProgressManager.Instance.IsLevelUnlocked(levelData.index), GetLevelStatusColor(levelButton.Data.index));
                levelButtons.Add(levelButton);
            }
        }
    }
    public void LoadLevelWithData(LevelData levelData)
    {
        currentLevelData.levelData = levelData;
        currentLevelData.currentQuests.AddRange(levelData.allQuests);
        currentLevelData.isLoadLevelFromSave = false;
        EventBusManager.Instance.SceneLoadRequest(levelData.sceneName);
    }
    
    private Color GetLevelStatusColor(int levelIndex)
    {
        Color statusColor;
        int currentLevel = ProgressManager.Instance.GetCurrentLevel();
        
        if (currentLevel < levelIndex)
        {
            statusColor = lockedColor;
        }
        else if (currentLevel == levelIndex)
        {
            statusColor = openedColor;
        }
        else
        {
            statusColor = completedColor;
        }
        return statusColor;
    }
}