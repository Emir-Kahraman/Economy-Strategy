using System;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;

    [SerializeField] private int currentLevel;// Открытый уровень, до считаются пройденными, после - закрытыми.
    [SerializeField] private LevelDatabase levelDatabase;
    public int CurrentLevel => currentLevel;

    private void Awake()
    {
        Initialize();
    }
    private void OnDestroy()
    {
        UninitializeEvents();
    }

    private void Initialize()
    {
        InitializeSingleton();
        InitializeEvents();
        LoadProgress();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        gameObject.name = "ProgressManager";
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnLevelCompleted += LevelCompleted;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnLevelCompleted -= LevelCompleted;
    }
    private void LoadProgress()
    {
        currentLevel = SaveManager.Instance.LoadGameProgress();
    }

    private void LevelCompleted(int levelIndex)
    {
        if (levelIndex >= currentLevel)
        {
            currentLevel = levelIndex + 1;
            ProgressData progressData = new()
            {
                CurrentLevel = currentLevel
            };
            EventBusManager.Instance.ProgressChanged(progressData);
        }
    }

    public bool IsLevelUnlocked(int levelIndex)
    {
        return levelIndex <= 1 || levelIndex <= currentLevel; // First level is always unlocked
    }
    public int GetCurrentLevel() => currentLevel;
}
