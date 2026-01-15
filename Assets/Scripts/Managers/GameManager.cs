using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool Exists => Instance != null;
    public static GameManager Instance;

    [Header("Persistent Managers")]
    [SerializeField] private List<GameObject> persistentManagers = new();
    [Header("Ephemeral Managers")]
    [SerializeField] private List<GameObject> ephemeralManagers = new();

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
    }

    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameObject.name = "GameManager";
        DontDestroyOnLoad(gameObject);
    }
    private void InitializeEvents()
    {
        SceneManager.sceneLoaded += InitializeEphemeralManagers;
        EventBusManager.Instance.OnSceneLoadRequest += LoadScene;
    }
    private void UninitalizeEvents()
    {
        SceneManager.sceneLoaded -= InitializeEphemeralManagers;
        EventBusManager.Instance.OnSceneLoadRequest -= LoadScene;
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        
    }
    private void InitializePersistentManagers()
    {
        for(int i = 0; i < persistentManagers.Count; i++)
        {
            if (persistentManagers[i] != null)
            {
                GameObject managerObj = Instantiate(persistentManagers[i]);
            }
        }
        Debug.Log($"Все глобальные менеджеры в количестве {persistentManagers.Count} были успешно созданы");
        EventBusManager.Instance.SceneLoaded();
    }
    private void InitializeEphemeralManagers(Scene currentScene, LoadSceneMode mode)
    {
        if (!currentScene.name.StartsWith("Level")) return;
        for (int i = 0; i < ephemeralManagers.Count ; i++)
        {
            if (ephemeralManagers[i] != null)
            {
                GameObject managerObj = Instantiate(ephemeralManagers[i]);
            }
        }
        Debug.Log($"Все локальные менеджеры в количестве {ephemeralManagers.Count} были успешно созданы");
        EventBusManager.Instance.SceneLoaded();
    }
}
