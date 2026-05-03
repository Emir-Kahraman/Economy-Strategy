using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;
    [SerializeField] private CurrentLevelRuntimeData currentLevelRuntimeData;

    private int currentMoney = 1000;
    private float bankruptcyProcess = 0f;
    private float negativeBalanceTimer = 5f;
    private bool isAtRiskOfBankruptcy = false;
    private bool bankruptcy = false;

    public int GetCurrentMoney() => currentMoney;
    public bool IsBankrupt => bankruptcy;

    public CurrencyManagerData GetCurrencyManagerData()
    {
        return new CurrencyManagerData
        {
            currentMoney = currentMoney,
            bankruptcyProcess = bankruptcyProcess,
            isAtRiskOfBankruptcy = isAtRiskOfBankruptcy
        };
    }

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        InitializeSingleton();
        InitializeLevelParameters();
        IsLoadLevelFromSave();
    }

    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameObject.name = "CurrencyManager";
    }

    private void InitializeLevelParameters()
    {
        if (currentLevelRuntimeData != null && currentLevelRuntimeData.levelData != null)
        {
            currentMoney = currentLevelRuntimeData.levelData.startingMoney;
        }
        else
        {
            Debug.LogError("CurrentLevelRuntimeData or LevelData is not assigned in CurrencyManager.");
        }
    }

    private void IsLoadLevelFromSave()
    {
        if (SaveManager.Instance.IsLoadLevelFromSave)
        {
            LoadCurrencyManagerData(SaveManager.Instance.LoadedLevelDates);
        }
    }

    private void LoadCurrencyManagerData(GameSessionData data)
    {
        CurrencyManagerData currencyManagerData = data.currencyManagerData;
        currentMoney = currencyManagerData.currentMoney;
        bankruptcyProcess = currencyManagerData.bankruptcyProcess;
        isAtRiskOfBankruptcy = currencyManagerData.isAtRiskOfBankruptcy;
    }

    private void Start()
    {
        InvokeStartEvents();
    }

    private void InvokeStartEvents()
    {
        EventBusManager.Instance.MoneyChanged(currentMoney);
    }

    public void AddMoney(int amount)
    {
        if (bankruptcy) return;  // ★ Блокируем при банкротстве
        
        currentMoney += amount;
        EventBusManager.Instance.MoneyChanged(GetCurrentMoney());
    }

    public bool TrySpendMoney(int amount)
    {
        if (bankruptcy) return false;  // ★ Блокируем при банкротстве
        
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            EventBusManager.Instance.MoneyChanged(GetCurrentMoney());
            return true;
        }
        return false;
    }

    public void SpendMoney(int amount)
    {
        if (bankruptcy) return;  // ★ Блокируем при банкротстве
        
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
        else if (GetCurrentMoney() >= 0)
        {
            // ★ Сброс таймера если деньги положительные
            negativeBalanceTimer = 5f;
        }
    }

    private void StartBankruptcyProcess()
    {
        negativeBalanceTimer = 5f;
        isAtRiskOfBankruptcy = true;
        bankruptcyProcess = 10f;
        Debug.Log("⚠️ Начинается процесс банкротства!");
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
        if (bankruptcyProcess >= 30)
            TriggerBankruptcy();
        else if (bankruptcyProcess <= 0)
            ResetBankruptcyProcess();
    }

    private void TriggerBankruptcy()
    {
        bankruptcy = true;
        Debug.Log("💀 БАНКРОТСТВО!");
        EventBusManager.Instance.Bankruptcy();
    }

    private void ResetBankruptcyProcess()
    {
        bankruptcyProcess = 0f;
        isAtRiskOfBankruptcy = false;
    }

    // ★ Новый метод: сброс при рестарте уровня
    public void ResetBankruptcyState()
    {
        bankruptcy = false;
        isAtRiskOfBankruptcy = false;
        bankruptcyProcess = 0f;
        negativeBalanceTimer = 5f;
    }
}
