using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private GameSessionMeta gameSessionMeta;
    private GameSessionData gameSessionData;

    [SerializeField] private CurrentLevelRuntimeData levelRuntimeData;

    private string levelSaveMetaPath;
    private string levelSaveDataPath;
    private string gameProgressSavePath;
    public bool IsLoadLevelFromSave => levelRuntimeData.isLoadLevelFromSave;
    public GameSessionData LoadedLevelDates
    {
        get
        {
            if (gameSessionData == null && levelRuntimeData.isLoadLevelFromSave)
            {
                LoadLevelSaveData();
            }
            return gameSessionData;
        }
    }

    private void Awake()
    {
        Initialize();
    }
    private void OnDestroy()
    {
        UninitializeEvents();
    }

    public bool HasLevelProgressSave()
    {
        return File.Exists(levelSaveDataPath);
    }
    public GameSessionMeta GetGameSessionMetaDate()
    {
        string json = File.ReadAllText(levelSaveMetaPath);
        GameSessionMeta meta = JsonUtility.FromJson<GameSessionMeta>(json);
        return meta;
    }

    private void Initialize()
    {
        InitializeSingleton();
        InitializeSavePaths();
        InitializeEvents();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        gameObject.name = "SaveManager";
    }
    private void InitializeSavePaths()
    {
        levelSaveMetaPath = Path.Combine(Application.persistentDataPath, "levelMeta.meta");
        levelSaveDataPath = Path.Combine(Application.persistentDataPath, "levelProgress.sav");
        gameProgressSavePath = Path.Combine(Application.persistentDataPath, "gameProgress.sav");
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnRequestLevelProgressSave += LevelProgressSave;
        SceneManager.sceneLoaded += UpdateLevelRuntime;
        EventBusManager.Instance.OnGameProgressChanged += SaveGameProgress;
        EventBusManager.Instance.OnResetGameProgress += ResetGameProgress;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnRequestLevelProgressSave -= LevelProgressSave;
        SceneManager.sceneLoaded -= UpdateLevelRuntime;
        EventBusManager.Instance.OnGameProgressChanged -= SaveGameProgress;
        EventBusManager.Instance.OnResetGameProgress -= ResetGameProgress;
    }

    private void LevelProgressSave()
    {
        gameSessionMeta = GameSessionMetaDate();
        LevelMetaDateSave();
        gameSessionData = AllGameSessionDates();
        LevelGameSessionSave();
    }
    private void LevelMetaDateSave()
    {
        string json = JsonUtility.ToJson(gameSessionMeta);
        string savePath = levelSaveMetaPath;
        File.WriteAllText(savePath, json);
    }
    private void LevelGameSessionSave()
    {
        string json = JsonUtility.ToJson(gameSessionData);
        string savePath = levelSaveDataPath;
        File.WriteAllText(savePath, json);
    }
    private GameSessionMeta GameSessionMetaDate()
    {
        return levelRuntimeData.GetGameSessionMeta();
    }
    private GameSessionData AllGameSessionDates()
    {
        GameSessionData dates = new GameSessionData();
        dates.saveDateTime = DateTime.Now;
        dates.saveGameVersion = Application.version;

        dates.tilemapManagerData = TilemapManager.Instance.GetTilemapManagerData();
        dates.buildingManagerData = BuildingManager.Instance.GetBuildingManagerData();
        dates.currencyManagerData = CurrencyManager.Instance.GetCurrencyManagerData();
        dates.storageManagerData = StorageManager.Instance.GetStorageManagerData();
        dates.questManagerData = QuestManager.Instance.GetQuestManagerData();
        dates.ordersManagerData = OrdersManager.Instance.GetOrdersManagerData();
        dates.productionManagerData = ProductionManager.Instance.GetProductionManagerData();

        return dates;
    }

    private void LoadLevelSaveData()
    {
        if (File.Exists(levelSaveDataPath))
        {
            string jsonDates = File.ReadAllText(levelSaveDataPath);
            gameSessionData = JsonUtility.FromJson<GameSessionData>(jsonDates);
        }
        else
        {
            Debug.LogWarning("No level progress save found.");
            gameSessionData = null;
        }
    }

    private void UpdateLevelRuntime(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex <= 1)
        {
            levelRuntimeData.Reset();
            ClearLoadedData();
        }
    }
    private void ClearLoadedData()
    {
        gameSessionData = null;
    }

    private void SaveGameProgress(ProgressData progressData)
    {
        progressData.lastSaveTime = DateTime.Now;
        progressData.gameVersion = Application.version;

        string json = JsonUtility.ToJson(progressData);
        File.WriteAllText(gameProgressSavePath, json);
    }
    public int LoadGameProgress()
    {
        if (!File.Exists(gameProgressSavePath))
        {
            Debug.LogWarning("No saved game progress found.");
            ProgressData newSave = CreateNewGameProgressSave();
            return newSave.CurrentLevel;
        }

        try
        {
            string json = File.ReadAllText(gameProgressSavePath);
            ProgressData progressSave = JsonUtility.FromJson<ProgressData>(json);

            if (progressSave == null)
            {
                Debug.LogWarning("Failed to deserialize game progress data. Creating new save.");
                ProgressData newSave = CreateNewGameProgressSave();
                return newSave.CurrentLevel;
            }

            return progressSave.CurrentLevel;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading game progress: {ex.Message}");
            ProgressData newSave = CreateNewGameProgressSave();
            return newSave.CurrentLevel;
        }
    }
    private ProgressData CreateNewGameProgressSave()
    {
        ProgressData newProgressData = new ProgressData
        {
            CurrentLevel = 1,
            lastSaveTime = DateTime.Now,
            gameVersion = Application.version
        };
        SaveGameProgress(newProgressData);
        return newProgressData;
    }
    private void ResetGameProgress()
    {
        CreateNewGameProgressSave();
    }
}
