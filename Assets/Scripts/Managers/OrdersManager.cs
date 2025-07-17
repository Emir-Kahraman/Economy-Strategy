using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class OrderSpawnRule
{
    public int ruleTime;
    public int spawnInterval;
    public int productionLevel;
    public int maxActiveOrders;
}
public class OrdersManager : MonoBehaviour
{
    public static OrdersManager Instance;

    private bool _bankruptcy = false;

    [SerializeField] private List<OrderSpawnRule> spawnRules = new();
    [SerializeField] private float baseExistenceTime;
    [SerializeField] private float baseCompletionTime;
    [SerializeField] private int baseResourceAmount;
    [SerializeField] private int baseRewardAmount;

    private int currentRuleIndex = 0;
    private float ruleTimer;
    private float spawnTimer;
    private int maxActiveOrders;
    private int activeOrdersCount;
    private int maxAcceptedOrders = 4;
    private int acceptedOrdersCount;
    private int currentProductionLevel;

    private float gameTime;
    private float timeModifier = 1f;

    private List<ResourceData> resourceDates = new();

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
        ApplyRule();
        InitializeEvents();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "OrdersManager";
    }
    private void ApplyRule()
    {
        if (spawnRules.Count == 0)
        {
            Debug.LogError("Количество spawnRules равно 0");
            return;
        }
        currentRuleIndex = Mathf.Clamp(currentRuleIndex, 0, spawnRules.Count - 1);

        OrderSpawnRule rule = spawnRules[currentRuleIndex];
        ruleTimer = rule.ruleTime;
        spawnTimer = rule.spawnInterval;
        currentProductionLevel = rule.productionLevel;
        if (currentProductionLevel == 0)
        {
            currentProductionLevel = 1;
            Debug.LogError("ProductionLevel = 0!");
        }
        maxActiveOrders = rule.maxActiveOrders;
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnBankruptcy += Bankruptcy;
        EventBusManager.Instance.OnResourceDataUpdated += SetResourceDates;
        EventBusManager.Instance.OnOrderAccepted += AcceptOrder;
        EventBusManager.Instance.OnOrderExpired += DeleteOrder;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnBankruptcy -= Bankruptcy;
        EventBusManager.Instance.OnResourceDataUpdated -= SetResourceDates;
        EventBusManager.Instance.OnOrderAccepted -= AcceptOrder;
        EventBusManager.Instance.OnOrderExpired -= DeleteOrder;
    }
    
    private void Bankruptcy()
    {
        _bankruptcy = true;
    }

    private void SetResourceDates(List<ResourceData> dates)
    {
        resourceDates = dates;
    }
    private void Update()
    {
        if (_bankruptcy) return;

        UpdateTimeModifier();
        UpdateRuleTimer();
        UpdateOrderSpawning();
    }
    private void UpdateTimeModifier()
    {
        if (timeModifier > 3) return;
        gameTime += Time.deltaTime;
        if (gameTime >= 10f)
        {
            gameTime = 0;
            timeModifier += 0.1f;
        }
    }
    private void UpdateRuleTimer()
    {
        ruleTimer -= Time.deltaTime;

        if (ruleTimer <= 0 && currentRuleIndex < spawnRules.Count - 1)
        {
            currentRuleIndex++;
            ApplyRule();
        }
    }

    private void UpdateOrderSpawning()
    {
        if (activeOrdersCount >= maxActiveOrders) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnOrder();
            ResetSpawnTimer();
        }
    }
    private void SpawnOrder()
    {
        OrderData newOrder = GenerateRandomOrder();
        activeOrdersCount++;
        EventBusManager.Instance?.OrderCreated(newOrder);
    }
    private OrderData GenerateRandomOrder()
    {
        string id = Guid.NewGuid().ToString();
        ResourceData resourceData = SelectResource();
        float existenceTime = GetExistenceTime();
        int resourceAmount = GetResourceAmount(resourceData.ProductionHard);
        float completionTime = GetCompletionTime(resourceAmount, resourceData.ProductionHard);
        int reward = GetRewardAmount(resourceAmount, resourceData.ProductionLevel, resourceData.ProductionHard);

        return new OrderData(id, existenceTime, completionTime, resourceData, resourceAmount, reward);
    }
    private ResourceData SelectResource()
    {
        var validResources = resourceDates.Where(r => r.ProductionLevel <= currentProductionLevel).ToList();

        List<float> weights = new List<float>();
        float totalWeight = 0;

        foreach (var resource in validResources)
        {
            int delta = currentProductionLevel - resource.ProductionLevel;
            float weight = 6f / Mathf.Pow(2, delta);

            weights.Add(weight);
            totalWeight += weight;
        }

        float randomPoint = UnityEngine.Random.Range(1, totalWeight);
        float currentWeight = 0;

        for (int i = 0; i < validResources.Count; i++)
        {
            currentWeight += weights[i];
            if (randomPoint <= currentWeight)
            {
                return validResources[i];
            }
        }
        return validResources.Last();
    }
    private float GetExistenceTime()
    {
        return CalculateRandomValue(baseExistenceTime, 20f);
    }
    private int GetResourceAmount(int hardLevel)
    {
        float value = baseResourceAmount / (hardLevel / 10f) * timeModifier;
        return Mathf.FloorToInt(CalculateRandomValue(value, 40f));
    }
    private float GetCompletionTime(int resourceAmount, float hardLevel)
    {
        float amountModifier = resourceAmount / baseResourceAmount / 5f;
        float hardLevelModifier = hardLevel / 10f;
        float value = baseCompletionTime + (baseCompletionTime * amountModifier) + (baseCompletionTime * hardLevelModifier);
        return CalculateRandomValue(value, 20f);
    }
    private int GetRewardAmount(int resourceAmount, int productionLevel, int hardLevel)
    {
        float amountModifier = resourceAmount / baseResourceAmount / 10f;
        float productionLevelModifier = productionLevel / 10f;
        float hardLevelModifier = hardLevel / 10f;
        float value = baseRewardAmount + (baseRewardAmount * amountModifier) + (baseRewardAmount * productionLevelModifier) + (baseRewardAmount * hardLevelModifier);
        return Mathf.FloorToInt(CalculateRandomValue(value, 40f));
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
        spawnTimer = spawnRules[currentRuleIndex].spawnInterval;
    }

    private void AcceptOrder()//Через событие скорее будет вызываться
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
}
