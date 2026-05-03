using System;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    private void Awake()
    {
        InitializeSingleton();
        InitializeEvents();
    }

    private void OnDestroy()
    {
        UninitializeEvents();
    }

    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameObject.name = "GameOverManager";
    }

    private void InitializeEvents()
    {
        EventBusManager.Instance.OnBankruptcy += HandleBankruptcy;
        EventBusManager.Instance.OnLevelRestart += ResetBankruptcyState;
    }

    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnBankruptcy -= HandleBankruptcy;
        EventBusManager.Instance.OnLevelRestart -= ResetBankruptcyState;
    }

    private void HandleBankruptcy()
    {
        Time.timeScale = 0f;
        UIManager.Instance.ShowGameOver();  // ★ Просто вызываем метод менеджера
        Debug.Log("💀 Банкротство! Игра на паузе.");
    }

    private void ResetBankruptcyState()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.ResetBankruptcyState();
        }
    }
}
