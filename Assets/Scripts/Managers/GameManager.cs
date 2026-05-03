using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool Exists => Instance != null;
    public static GameManager Instance;

    [Header("Main Camera Prefab")]
    [SerializeField] private GameObject mainCameraPrefab;
    [Header("Persistent Managers")]
    [SerializeField] private List<GameObject> persistentManagers = new();
    [Header("Ephemeral Managers")]
    [SerializeField] private List<GameObject> ephemeralManagers = new();

    private MainCameraMovement mainCamera;

    private void Awake()
    {
        Initialize();
    }
    private void OnDestroy()
    {
        UninitalizeEvents();
    }
    private void Initialize()
    {
        InitializeSingleton();
        InitializePersistentManagers();
        InitializeEvents();
        InitializeMainCamera();
    }

    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameObject.name = "GameManager";
        DontDestroyOnLoad(gameObject);
    }
    private void InitializePersistentManagers()
    {
        for (int i = 0; i < persistentManagers.Count; i++)
        {
            if (persistentManagers[i] != null)
            {
                GameObject managerObj = Instantiate(persistentManagers[i]);
            }
        }
        Debug.Log($"Все глобальные менеджеры в количестве {persistentManagers.Count} были успешно созданы");
        EventBusManager.Instance.SceneLoaded();
    }
    private void InitializeEvents()
    {
        SceneManager.sceneLoaded += SceneLoaded;
        EventBusManager.Instance.OnSceneLoadRequest += LoadScene;
        EventBusManager.Instance.OnLevelRestart += RestartLevel;
    }
    private void UninitalizeEvents()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        EventBusManager.Instance.OnSceneLoadRequest -= LoadScene;
        EventBusManager.Instance.OnLevelRestart -= RestartLevel;
    }
    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex > 1)
        {
            DestroyEphemeralManagers();
            InitializeEphemeralManagers();
        }
        EventBusManager.Instance.SceneLoaded();
    }
    private void InitializeEphemeralManagers()
    {
        for (int i = 0; i < ephemeralManagers.Count ; i++)
        {
            if (ephemeralManagers[i] != null)
            {
                GameObject managerObj = Instantiate(ephemeralManagers[i]);
            }
        }
        Debug.Log($"Все локальные менеджеры в количестве {ephemeralManagers.Count} были успешно созданы");
        SetMainCameraParameters();
    }
    private void InitializeMainCamera()
    {
        GameObject mainCameraGO = Instantiate(mainCameraPrefab);
        DontDestroyOnLoad(mainCameraGO);
        mainCameraGO.name = "Main Camera";
        mainCamera = mainCameraGO.GetComponent<MainCameraMovement>();
    }

    private void SetMainCameraParameters()
    {
        Vector3 position = TilemapManager.Instance.GetMapCenter();
        mainCamera.SetPosition(position);
        TilemapManager.Instance.GetWorldBounds(out float mapMinX, out float mapMaxX, out float mapMinY, out float mapMaxY);
        mainCamera.SetMapBounds(mapMinX, mapMaxX, mapMinY, mapMaxY);
    }

    // ★ Новый метод: рестарт уровня
    private void RestartLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        // ★ Очищаем состояние банкротства
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.ResetBankruptcyState();
        }

        // ★ Очищаем UI менеджеры
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ResetCurrentWindow();
            var windows = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IUIWindow>();
            foreach (var window in windows)
            {
                window.Uninitialize();
            }
        }

        Time.timeScale = 1f;
        DestroyEphemeralManagers();
        SceneManager.LoadScene(currentScene.name);

        Debug.Log($"Уровень перезагружен: {currentScene.name}");
    }

    public void DestroyEphemeralManagers()
    {
        foreach (var managerPrefab in ephemeralManagers)
        {
            if (managerPrefab == null) continue;

            var activeInstance = FindObjectsByType(managerPrefab.GetComponent<MonoBehaviour>().GetType(), FindObjectsSortMode.None);
            foreach (var instance in activeInstance)
            {
                Destroy(instance);
            }
        }
    }
}
