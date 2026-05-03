using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class OrdersManager : MonoBehaviour
{
    public static OrdersManager Instance;
    [SerializeField] private CurrentLevelRuntimeData currentLevelRuntimeData;

    private bool _bankruptcy = false;

    [Header("Base Orders Parameters")]
    [SerializeField] private float baseExistenceTime;
    [SerializeField] private float baseCompletionTime;
    [SerializeField] private int baseResourceAmount;
    [SerializeField] private float baseOrderSpawnTimer;
    [SerializeField] private int maxActiveOrders;
    [SerializeField] private float increaseValue;

    private Period currentPeriod;
    private Period maxPeriod;
    private int timerToIIPeriod;
    private int timerToIIIPeriod;
    private int timerToIVPeriod;
    private float orderSpawnTimer;
    private int activeOrdersCount;
    private int maxAcceptedOrders = 8;
    private int acceptedOrdersCount;
    private float demandBonusPerPeriod = 0.25f;
    private float completionTimeBonusPerPeriod = 0.15f;
    private float timeToNewPeriod;

    private Coroutine increaseDemand;

    private List<ResourceData> availableResources = new();
    private Dictionary<ResourceData, float> resourceDemand = new();

    public Period GetPeriod => currentPeriod;

    UIOrdersMenuController ordersMenuController;

    public OrdersManagerData GetOrdersManagerData()
    {
        var data = new OrdersManagerData();
        data.currentPeriod = currentPeriod.ToString();

        foreach (var kvp in resourceDemand)
        {
            var entry = new OrdersManagerData.ResourceDemandEntry(kvp.Key.Key, kvp.Value);
            data.resourceDemand.Add(entry);
        }

        ordersMenuController = FindAnyObjectByType<UIOrdersMenuController>();
        var acceptedOrders = ordersMenuController.GetAcceptedOrders();
        foreach (var kvp in acceptedOrders)
        {
            OrderData orderData = kvp.GetOrderData();
            OrdersManagerData.OrderSaveData saveData = new OrdersManagerData.OrderSaveData
            (
                orderData.id,
                orderData.resourceData.Key,
                orderData.existenceTime,
                orderData.completionTime,
                orderData.resourceAmount,
                orderData.reward
            );
            data.acceptedOrders.Add(saveData);
        }

        var activeOrders = ordersMenuController.GetActiveOrders();
        foreach (var kvp in activeOrders)
        {
            OrderData orderData = kvp.GetOrderData();
            OrdersManagerData.OrderSaveData saveData = new OrdersManagerData.OrderSaveData
            (
                orderData.id,
                orderData.resourceData.Key,
                orderData.existenceTime,
                orderData.completionTime,
                orderData.resourceAmount,
                orderData.reward
            );
            data.activeOrders.Add(saveData);
        }

        return data;
    }

    private void Awake()
    {
        Initialize();
    }
    private void OnDestroy()
    {
        UninitializeEvents();
    }
    private void Initialize()
    {
        InitializeSingleton();
        InitializeLevelParameters();
        InitializeEvents();
        IsLevelLoadFromSave();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameObject.name = "OrdersManager";
    }
    private void InitializeLevelParameters()
    {
        if (currentLevelRuntimeData != null && currentLevelRuntimeData.levelData != null)
        {
            currentPeriod = currentLevelRuntimeData.levelData.periodParameters.startPeriod;
            maxPeriod = currentLevelRuntimeData.levelData.periodParameters.endPeriod;
            timerToIIPeriod = currentLevelRuntimeData.levelData.periodParameters.timerToIIPeriod;
            timerToIIIPeriod = currentLevelRuntimeData.levelData.periodParameters.timerToIIIPeriod;
            timerToIVPeriod = currentLevelRuntimeData.levelData.periodParameters.timerToIVPeriod;
        }
        else
        {
            Debug.LogError("CurrentLevelRuntimeData or LevelData is not assigned in OrdersManager.");
        }
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnBankruptcy += Bankruptcy;
        EventBusManager.Instance.OnOrderAccepted += AcceptOrder;
        EventBusManager.Instance.OnOrderExpired += DeleteOrder;
        EventBusManager.Instance.OnOrderCompleted += OrderCompleted;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnBankruptcy -= Bankruptcy;
        EventBusManager.Instance.OnOrderAccepted -= AcceptOrder;
        EventBusManager.Instance.OnOrderExpired -= DeleteOrder;
        EventBusManager.Instance.OnOrderCompleted -= OrderCompleted;
    }
    private void IsLevelLoadFromSave()
    {
        if (SaveManager.Instance.IsLoadLevelFromSave)
        {
            LoadOrdersManagerData(SaveManager.Instance.LoadedLevelDates);
        }
    }
    private void LoadOrdersManagerData(GameSessionData sessionData)
    {
        OrdersManagerData data = sessionData.ordersManagerData;
        currentPeriod = (Period)Enum.Parse(typeof(Period), data.currentPeriod);

        resourceDemand.Clear();
        foreach (var entry in data.resourceDemand)
        {
            ResourceData resourceData = entry.GetResourceData();
            if (resourceData != null)
                resourceDemand[resourceData] = entry.demand;
            else
                Debug.LogWarning($"Resource '{entry.resourceName}' not found. Skipping demand entry.");
        }

        List<OrderData> acceptedOrders = new List<OrderData>();
        foreach (var orderSaveData in data.acceptedOrders)
        {
            OrderData orderData = orderSaveData.ToOrderData();
            acceptedOrders.Add(orderData);
        }
        EventBusManager.Instance.OrdersLoadedFromSave(acceptedOrders, true);

        List<OrderData> activeOrders = new List<OrderData>();
        foreach (var orderSaveData in data.activeOrders)
        {
            OrderData orderData = orderSaveData.ToOrderData();
            activeOrders.Add(orderData);
        }
        EventBusManager.Instance.OrdersLoadedFromSave(activeOrders, false);
    }

    private void Bankruptcy()
    {
        _bankruptcy = true;
    }

    private void Start()
    {
        UpdateAvailableResource();
        SetPeriodTimer();
        EventBusManager.Instance.CurrentPeriodUpdated(currentPeriod);

        if (increaseDemand == null)
        {
            increaseDemand = StartCoroutine(IncreaseDemand());
        }
        else StopCoroutine(increaseDemand);
    }
    private void UpdateAvailableResource()
    {
        availableResources = ResourceLibrary.GetAllResources().Where(resource => IsPeriodAllowed(resource.Period) && resource.CanBeOrdered).ToList();
        foreach (var resource in availableResources)
        {
            if (!resourceDemand.ContainsKey(resource))
            {
                resourceDemand[resource] = 1;
            }
        }
    }

    private void SetPeriodTimer()
    {
        switch (currentPeriod)
        {
            case Period.I: timeToNewPeriod = timerToIIPeriod; break;
            case Period.II: timeToNewPeriod = timerToIIIPeriod; break;
            case Period.III: timeToNewPeriod = timerToIVPeriod; break;
            default:timeToNewPeriod = 0; break;
        }
    }

    
    private void Update()
    {
        if (_bankruptcy) return;
        PeriodTimer();
        UpdateOrderSpawning();
    }

    private IEnumerator IncreaseDemand()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            increaseValue *= Time.deltaTime;
            List<ResourceData> keys = new List<ResourceData>(resourceDemand.Keys);
            foreach (var resource in keys)
            {
                if (resourceDemand[resource] < 1)
                {
                    resourceDemand[resource] += increaseValue;
                }
                Mathf.Min(1f, resourceDemand[resource]);
            }
        }
    }

    private void PeriodTimer()
    {
        if ((int)currentPeriod >= (int)maxPeriod) return;
        timeToNewPeriod -= Time.deltaTime;
        switch (currentPeriod)
        {
            case Period.I: EventBusManager.Instance.PeriodTimerUpdated(timeToNewPeriod, timerToIIPeriod); break;
            case Period.II: EventBusManager.Instance.PeriodTimerUpdated(timeToNewPeriod, timerToIIIPeriod); break;
            case Period.III: EventBusManager.Instance.PeriodTimerUpdated(timeToNewPeriod, timerToIVPeriod); break;
        }
        if (timeToNewPeriod <= 0)
        {
            UpdatePeriod();
        }
    }
    private void UpdatePeriod()
    {
        switch (currentPeriod)
        {
            case Period.I: currentPeriod = Period.II; break;
            case Period.II: currentPeriod = Period.III; break;
            case Period.III: currentPeriod = Period.IV; break;
            default: currentPeriod = Period.IV; Debug.LogError("Лимит в 4 Периода!"); break;
        }
        SetPeriodTimer();
        UpdateAvailableResource();
        EventBusManager.Instance.CurrentPeriodUpdated(currentPeriod);
    }

    private void UpdateOrderSpawning()
    {
        if (activeOrdersCount >= maxActiveOrders) return;
        
        orderSpawnTimer -= Time.deltaTime;
        if (orderSpawnTimer <= 0)
        {
            SpawnOrder();
            ResetSpawnTimer();
        }
    }
    private void SpawnOrder()
    {
        OrderData newOrder = GenerateOrder();
        activeOrdersCount++;
        EventBusManager.Instance.OrderCreated(newOrder);
    }
    private OrderData GenerateOrder()
    {
        string id = Guid.NewGuid().ToString();//Есть вероятность повторения id, стоит создать словарь имеющихся заказов
        ResourceData resourceData = SelectResource();
        float existenceTime = GetExistenceTime();
        int resourceAmount = GetOrderResourceAmount(resourceData);
        float completionTime = GetCompletionTime(resourceData);
        int reward = GetRewardAmount(resourceAmount, resourceData);

        return new OrderData(id, existenceTime, completionTime, resourceData, resourceAmount, reward);
    }
    private ResourceData SelectResource()
    {
        float totalWeight = 0;
        foreach (var demand in resourceDemand.Values) 
        {
            totalWeight += demand;
        }


        float randomValue = UnityEngine.Random.Range(0, totalWeight);
        float currentSum = 0f;

        foreach (var kvp in resourceDemand)
        {
            currentSum += kvp.Value;
            if (randomValue <= currentSum)
            {
                return kvp.Key;
            }
        }
        if (availableResources.Count == 0)
        {
            UpdateAvailableResource();
        }
        try 
        {
            return availableResources.Last();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error selecting resource: " + ex.Message);
            return null;
        }
    }
    private bool IsPeriodAllowed(Period resourcePeriod)
    {
        return (int)resourcePeriod <= (int)currentPeriod;
    }
    private float GetExistenceTime()
    {
        return CalculateRandomValue(baseExistenceTime, 20f);
    }
    private int GetOrderResourceAmount(ResourceData resource)
    {
        return (int)(baseResourceAmount * (1 + demandBonusPerPeriod * (currentPeriod - resource.Period)));
    }
    private float GetCompletionTime(ResourceData resource)
    {
        return baseCompletionTime * (1 + completionTimeBonusPerPeriod * ((int)resource.Period - 1));
    }
    private int GetRewardAmount(int resourceAmount, ResourceData resource)
    {
        return (int)CalculateRandomValue((resourceAmount * resource.Price), 10f);
    }

    private float CalculateRandomValue(float amount, float percent)
    {
        float delta = amount * (percent / 100f);
        float minValue = amount - delta;
        float maxValue = amount + delta;
        return UnityEngine.Random.Range(minValue, maxValue);
    }

    private void ResetSpawnTimer()
    {
        orderSpawnTimer = baseOrderSpawnTimer;
    }

    private void AcceptOrder()
    {
        if (acceptedOrdersCount < maxAcceptedOrders)
        {
            activeOrdersCount--;
            acceptedOrdersCount++;
        }
        
    }
    public bool CanAcceptOrder()
    {
        return acceptedOrdersCount < maxAcceptedOrders;
    }
    private void DeleteOrder(bool isAcceptedOrder)
    {
        if (isAcceptedOrder) acceptedOrdersCount--;
        else activeOrdersCount--;
    }
    private void OrderCompleted(ResourceData resource, int reward)
    {
        DecreasingDemand(resource);
        CurrencyManager.Instance.AddMoney(reward);
    }
    private void DecreasingDemand(ResourceData resource)
    {
        resourceDemand[resource] -= 0.2f;
        if (resourceDemand[resource] < 0) resourceDemand[resource] = 0;
    }
}
