using System;
using JetBrains.Annotations;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [SerializeField] private int _currentMoney = 1000;
    public int GetCurrentMoney() => _currentMoney;    

    private void Awake()
    {
        InitializeSingleton();
    }    
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "CurrencyManager";
    }

    private void Start()
    {
        InvokeStartEvents();
    }
    private void InvokeStartEvents()
    {
        EventBusManager.Instance.MoneyChanged(GetCurrentMoney());
    }

    public void AddMoney(int amount)
    {
        _currentMoney += amount;
        EventBusManager.Instance.MoneyChanged(GetCurrentMoney());
    }

    public bool TrySpendMoney(int amount)
    {
        if(_currentMoney >= amount)
        {
            _currentMoney -= amount;
            EventBusManager.Instance.MoneyChanged(GetCurrentMoney());
            return true;
        }
        return false;
    }
}
