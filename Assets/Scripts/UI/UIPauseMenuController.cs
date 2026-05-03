using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIPauseMenuController : MonoBehaviour, IUIWindow
{
    [SerializeField] private GameObject targetWindow;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitMainMenuButton;
    public void Initialize()
    {
        InitializeButtons();
        CloseWindow();
    }
    public void Uninitialize()
    {
        UninitializeButtons();
    }
    private void UninitializeButtons()
    {
        resumeButton.onClick.RemoveAllListeners();
        restartButton.onClick.RemoveAllListeners();
        exitMainMenuButton.onClick.RemoveAllListeners();
    }
    private void InitializeButtons()
    {
        resumeButton.onClick.AddListener(Resume);
        restartButton.onClick.AddListener(Restart);
        exitMainMenuButton.onClick.AddListener(ExitMainMenu);
    }

    private void ExitMainMenu()
    {
        WindowCloseRequested();
        EventBusManager.Instance.RequestLevelProgressSave();
        EventBusManager.Instance.SceneLoadRequest("Main_Menu");
    }

    private void WindowCloseRequested()
    {
        EventBusManager.Instance.WindowCloseRequested(this);
    }
    public void OpenWindow()
    {
        Time.timeScale = 0f;
        targetWindow.SetActive(true);
    }
    public void CloseWindow()
    {
        Time.timeScale = 1f;
        targetWindow.SetActive(false);
    }

    private void Resume()
    {
        EventBusManager.Instance.WindowCloseRequested(this);
    }

    private void Restart()
    {
        Time.timeScale = 1f; // Убираем паузу перед рестартом
        EventBusManager.Instance.LevelRestart();
    }
}
