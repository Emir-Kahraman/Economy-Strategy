using System;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    private UIGameOverController gameOverController;

    private void Awake()
    {
        Initialize();
    }
    private void OnDestroy()
    {
        UninitiazeEvents();
    }
    private void Initialize()
    {
        InitializeSingleton();
        InitializeParameters();
        InitializeEvents();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "GameOverManager";
    }
    private void InitializeParameters()
    {
        gameOverController = FindAnyObjectByType<UIGameOverController>();
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnBankruptcy += Bankruptcy;
    }
    private void UninitiazeEvents()
    {
        EventBusManager.Instance.OnBankruptcy -= Bankruptcy;
    }

    private void Bankruptcy()
    {
        UIManager.Instance.OpenWindow(gameOverController);
        EventBusManager.Instance.GameOver("Bankruptcy");
    }
}
