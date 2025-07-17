using System;
using System.ComponentModel.Design;
using JetBrains.Annotations;
using UnityEngine;

public class CurrencyManager : MonoBehaviour //Отобразить в UI процесс банкротсва
{
    public static CurrencyManager Instance;

    private bool bankruptcy = false;

    [SerializeField] private int currentMoney = 1000;
    private float bankruptcyProcess = 0f;
    private float negativeBalanceTimer = 5f;
    private bool isAtRiskOfBankruptcy = false;
    public int GetCurrentMoney() => currentMoney;


    private void Awake()
    {
        Initialize();
    }
    private void Initialize()
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
        currentMoney += amount;
        EventBusManager.Instance.MoneyChanged(GetCurrentMoney());
    }

    public bool TrySpendMoney(int amount)
    {
        if(currentMoney >= amount)
        {
            currentMoney -= amount;
            EventBusManager.Instance.MoneyChanged(GetCurrentMoney());
            return true;
        }
        return false;
    }

    public void SpendMoney(int amount)
    {
        currentMoney -= amount;
        EventBusManager.Instance.MoneyChanged(GetCurrentMoney());
    }

    private void Update()
    {
        if (bankruptcy) return;
        
        BankruptcyTimer();
    }
    private void BankruptcyTimer()
    {
        CheckEnterBankruptcyRisk();
        UpdateBankruptcyProcess();
        CheckTriggerBankruptcy();
    }
    private void CheckEnterBankruptcyRisk()
    {
        if (GetCurrentMoney() < 0 && !isAtRiskOfBankruptcy)
        {
            negativeBalanceTimer -= Time.deltaTime;

            if (negativeBalanceTimer <= 0)
            {
                StartBankruptcyProcess();
            }
        }
    }
    private void StartBankruptcyProcess()
    {
        negativeBalanceTimer = 5f;
        isAtRiskOfBankruptcy = true;
        bankruptcyProcess = 10f;
    }
    private void UpdateBankruptcyProcess()
    {
        if (!isAtRiskOfBankruptcy) return;

        if (GetCurrentMoney() <= 0)
        {
            bankruptcyProcess += Time.deltaTime;
        }
        else
        {
            bankruptcyProcess -= Time.deltaTime / 2;
        }
        ClampBankruptcyProcess();
    }
    private void ClampBankruptcyProcess()
    {
        bankruptcyProcess = Mathf.Clamp(bankruptcyProcess, 0, 30);
        EventBusManager.Instance.BankruptcyProcess(bankruptcyProcess);
    }
    private void CheckTriggerBankruptcy()
    {
        if (bankruptcyProcess >= 30) TriggerBankruptcy();
        else if (bankruptcyProcess <= 0) ResetBankruptcyProcess();
    }
    private void TriggerBankruptcy()
    {
        bankruptcy = true;
        EventBusManager.Instance.Bankruptcy();
    }
    private void ResetBankruptcyProcess()
    {
        bankruptcyProcess = 0f;
        isAtRiskOfBankruptcy = false;
    }
}
