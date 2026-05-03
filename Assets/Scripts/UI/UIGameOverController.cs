using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGameOverController : MonoBehaviour, IUIWindow
{
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    public void Initialize()
    {
        CloseWindow();
        restartButton?.onClick.AddListener(OnRestart);
        mainMenuButton?.onClick.AddListener(OnMainMenu);
    }

    public void Uninitialize()
    {
        restartButton?.onClick.RemoveListener(OnRestart);
        mainMenuButton?.onClick.RemoveListener(OnMainMenu);
    }

    public void OpenWindow()
    {
        gameObject.SetActive(true);
        // Пауза уже установлена в GameOverManager
    }

    public void CloseWindow()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;  // ★ Возобновляем игру
    }

    private void OnRestart()
    {
        Time.timeScale = 1f;  // ★ Убираем паузу перед рестартом
        EventBusManager.Instance.LevelRestart();
    }

    private void OnMainMenu()
    {
        Time.timeScale = 1f;  // ★ Убираем паузу перед выходом
        EventBusManager.Instance.SceneLoadRequest("Main_Menu");
    }
}
