using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.UI;
using UnityEngine;

public class OrdersManager : MonoBehaviour
{
    public static OrdersManager Instance;

    private bool _bankruptcy = false;

    [SerializeField] private float baseExistenceTime;
    [SerializeField] private float baseCompletionTime;
    [SerializeField] private int baseResourceAmount;
    [SerializeField] private float baseOrderSpawnTimer;
    [SerializeField] private int maxActiveOrders;
    [SerializeField] private float increaseValue;
    [SerializeField] private Period currentPeriod;
    [Header("Level Parameters")]
    [SerializeField] private Period maxPeriod;
    [SerializeField] private int timerToIIPeriod;
    [SerializeField] private int timerToIIIPeriod;
    [SerializeField] private int timerToIVPeriod;
    
    private float orderSpawnTimer;
    
    private int activeOrdersCount;
    private int maxAcceptedOrders = 8;
    private int acceptedOrdersCount;
    private float demandBonusPerPeriod = 0.25f;
    private float completionTimeBonusPerPeriod = 0.15f;
    private float timeToNewPeriod;

    private Coroutine increaseDemand;

    private List<ResourceData> resourceDates = new();
    private List<ResourceData> availableResources = new();
    private Dictionary<ResourceData, float> resourceDemand = new();

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
        InitializeEvents();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "OrdersManager";
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnBankruptcy += Bankruptcy;
        EventBusManager.Instance.OnResourceDataUpdated += SetResourceDates;
        EventBusManager.Instance.OnOrderAccepted += AcceptOrder;
        EventBusManager.Instance.OnOrderExpired += DeleteOrder;
        EventBusManager.Instance.OnOrderCompleted += OrderCompleted;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnBankruptcy -= Bankruptcy;
        EventBusManager.Instance.OnResourceDataUpdated -= SetResourceDates;
        EventBusManager.Instance.OnOrderAccepted -= AcceptOrder;
        EventBusManager.Instance.OnOrderExpired -= DeleteOrder;
        EventBusManager.Instance.OnOrderCompleted -= OrderCompleted;
    }
    
    private void Bankruptcy()
    {
        _bankruptcy = true;
    }

    private void SetResourceDates(List<ResourceData> dates)
    {
        resourceDates = dates;
        UpdateAvailableResource();
    }

    private void UpdateAvailableResource()
    {
        availableResources = resourceDates.Where(resource => IsPeriodAllowed(resource.Period) && resource.CanBeOrdered).ToList();
        foreach (var resource in availableResources)
        {
            if (!resourceDemand.ContainsKey(resource))
            {
                resourceDemand[resource] = 1;
            }
        }
    }

    private void Start()
    {
        SetPeriodTimer();
        EventBusManager.Instance.CurrentPeriodUpdated(currentPeriod);

        if (increaseDemand == null)
        {
            increaseDemand = StartCoroutine(IncreaseDemand());
        }
        else StopCoroutine(increaseDemand);
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
            foreach (var resource in resourceDemand.Keys)
            {
                Debug.Log(resource.Type + "/" + resourceDemand[resource]);
            }
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
        return availableResources.Last();
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
