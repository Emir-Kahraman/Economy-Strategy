using System;
using UnityEngine;
using UnityEngine.UI;

public class UILoadLevelButton : MonoBehaviour
{
    [Header("Level Runtime Data")]
    [SerializeField] private CurrentLevelRuntimeData currentLevelData;
    [Header("Load Level Button")]
    [SerializeField] private Button loadLevelButton;
    
    private int levelIndex;
    private string sceneName;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        loadLevelButton.onClick.AddListener(LoadLevel);
    }

    private void OnEnable()
    {
        UpdateButtonStatus();
    }

    private void UpdateButtonStatus()
    {
        loadLevelButton.interactable = SaveManager.Instance.HasLevelProgressSave();
        if (loadLevelButton.interactable)
        {
            GameSessionMeta meta = SaveManager.Instance.GetGameSessionMetaDate();
            levelIndex = meta.index;
            sceneName = meta.sceneName;
        }
    }

    private void LoadLevel()
    {
        currentLevelData.levelData.index = levelIndex;
        currentLevelData.levelData.sceneName = sceneName;
        currentLevelData.isLoadLevelFromSave = true;
        EventBusManager.Instance.SceneLoadRequest(sceneName);
    }
}
