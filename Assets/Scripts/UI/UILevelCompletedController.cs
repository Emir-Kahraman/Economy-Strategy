using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UILevelCompletedController : MonoBehaviour, IUIWindow
{
    [SerializeField] private GameObject targetWindow;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private LevelDatabase levelDatabase;
    [SerializeField] private CurrentLevelRuntimeData currentLevelRuntimeData;

    private int completedLevelIndex;

    public void Initialize()
    {
        InitializeButtons();
        InitializeEvents();
        CloseWindow();
    }
    public void Uninitialize()
    {
        UninitializeButtons();
        UninitializeEvents();
    }
    private void InitializeButtons()
    {
        mainMenuButton.onClick.AddListener(BackToMainMenu);
        nextLevelButton.onClick.AddListener(ToNextLevel);
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnAllQuestsCompleted += LevelCompleted;
    }
    private void UninitializeButtons()
    {
        mainMenuButton.onClick.RemoveListener(BackToMainMenu);
        nextLevelButton.onClick.RemoveListener(ToNextLevel);
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnAllQuestsCompleted -= LevelCompleted;
    }

    private void LevelCompleted(int levelIndex)
    {
        Time.timeScale = 0f;
        completedLevelIndex = levelIndex;
        if (levelDatabase != null)
        {
            levelDatabase = Resources.Load<LevelDatabase>("LevelDatabase/LevelDatabase");
        }

        bool nextLevelExists = false;
        if (levelDatabase != null)
        {
            int nextIndex = completedLevelIndex + 1;
            nextLevelExists = levelDatabase.levels.Any(l => l.index == nextIndex);
        }
        nextLevelButton.gameObject.SetActive(nextLevelExists);

        OpenWindow();
        EventBusManager.Instance.LevelCompleted(levelIndex);
    }

    private void BackToMainMenu()
    {
        EventBusManager.Instance.SceneLoadRequest("Main_Menu");
    }
    private void ToNextLevel()
    {
        if (levelDatabase == null)
        {
            Debug.LogError("LevelDatabase is not assigned.");
            BackToMainMenu();
            return;
        }

        int nextIndex = completedLevelIndex + 1;
        LevelData nextLevel = levelDatabase.levels.FirstOrDefault(l => l.index == nextIndex);

        if (nextLevel != null)
        {
            currentLevelRuntimeData.PrepareForNewLevel(nextLevel);

            GameManager.Instance.DestroyEphemeralManagers();

            EventBusManager.Instance.SceneLoadRequest(nextLevel.sceneName);
        }
        else
        {
            Debug.LogWarning("No next level found. Returning to main menu.");
            BackToMainMenu();
        }
    }

    public void OpenWindow()
    {
        targetWindow.SetActive(true);
    }
    public void CloseWindow()
    {
        targetWindow.SetActive(false);
    }
}
