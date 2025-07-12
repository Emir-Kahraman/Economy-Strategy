using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum ResourceType {None, Test, Forest, Stone_Ore, Wood, Stone}

public class StorageManager : MonoBehaviour
{
    public static StorageManager Instance;
    [SerializeField] private List<ResourceData> allResources;
    [SerializeField] private Dictionary<ResourceType, int> inventory = new();
    [SerializeField] private float totalCapacity;
    [SerializeField] private float currentVolume;
    [Serializable]
    private class StartResources
    {
        public ResourceType type;
        public int amount;
    }
    [SerializeField] private List<StartResources> startResources;

    private float baseStartCapacity = 100f;

    private Dictionary<ResourceType, ResourceData> resourceInfos = new();//Не все ресурсы могут быть иниализированы, нужно добавить для этого обработку
    public int GetResourceCount(ResourceType type) => inventory.TryGetValue(type, out var count) ? count : 0;
    public float GetCurrentVolume() => currentVolume;
    public float GetTotalCapacity() => totalCapacity;

    private void Awake()
    {
        InitializeSingleton();
        Initialize();
    }
    private void OnDestroy()
    {
        UninitializeEvents();
    }
    private void InitializeSingleton()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "StorageManager";
    }
    private void Initialize()
    {
        InitializeInventory();
        InitializeResourceData();
        InitializeParameters();
        InitializeEvents();
    }
    private void InitializeInventory()
    {
        foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
        {
            inventory[resource] = 0;
        }   
    }
    private void InitializeResourceData()
    {
        foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
        {
            for (int i = 0; i < allResources.Count; i++)
            {
                if (resource == allResources[i].Type) resourceInfos[resource] = allResources[i];
            }
        }
    }
    private void InitializeParameters()
    {
        totalCapacity = baseStartCapacity;
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnStorageBuilt += UpdateStorageCapacity;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnStorageBuilt -= UpdateStorageCapacity;
    }

    private void Start()
    {
        InvokeStartEvents();
        AddStartResources();
    }
    private void AddStartResources()
    {
        for (int i = 0; i < startResources.Count; i++)
        {
            var type = startResources[i].type;
            var amount = startResources[i].amount;
            AddResource(type, amount);
        }
    }
    private void InvokeStartEvents()
    {
        EventBusManager.Instance.ResourceDataUpdated(allResources);
        EventBusManager.Instance.StorageCapacityUpdated(totalCapacity);
    }

    public bool AddResource(ResourceType type, int amount)
    {
        if (!resourceInfos.TryGetValue(type, out var info)) return false;

        float addedVolume = info.VolumePerUnit * amount;
        if (currentVolume + addedVolume > totalCapacity) return false;

        currentVolume += addedVolume;
        inventory[type] += amount;
        EventBusManager.Instance.ResourceUpdated(type, inventory[type]);
        return true;
    }

    public int ConsumeResource(ResourceType type, int amount)
    {
        int availableAmount = inventory[type];
        int amountToConsume = Mathf.Min(availableAmount, amount);

        if (amountToConsume > 0)
        {
            currentVolume -= resourceInfos[type].VolumePerUnit * amountToConsume;
            inventory[type] -= amountToConsume;
            EventBusManager.Instance.ResourceUpdated(type, inventory[type]);
        }
        
        return amountToConsume;
    }

    public bool HasResource(ResourceType type, int amount)
    {
        return inventory.TryGetValue(type, out var count) && count >= amount;
    }

    private void UpdateStorageCapacity(float changeCapacity)
    {
        totalCapacity += changeCapacity;
        EventBusManager.Instance.StorageCapacityUpdated(totalCapacity);
    }
    public bool CanDeleteStorageBuilding()//
    {
        return true;
    }
}