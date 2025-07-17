using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGameOverController : MonoBehaviour, IUIWindow
{
    [SerializeField] private GameObject targetWindow;
    [SerializeField] private TextMeshProUGUI causeForGameOver;
    [SerializeField] private Button closeButton;

    public void Initialize()
    {
        InitializeParameters();
        InitializeEvents();
        CloseWindow();
    }
    private void OnDestroy()
    {
        UninitializeEvents();
    }

    private void InitializeParameters()
    {
        closeButton.onClick.AddListener(CloseWindowRequested);
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnGameOver += GameOverCause;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnGameOver -= GameOverCause;
    }

    private void GameOverCause(string cause)
    {
        causeForGameOver.text = cause;
    }

    private void CloseWindowRequested()
    {
        EventBusManager.Instance.WindowCloseRequested(this);
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
