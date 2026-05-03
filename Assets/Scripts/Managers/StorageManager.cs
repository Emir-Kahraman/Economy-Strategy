using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.Collections;

public enum Period { I = 1, II = 2, III = 3, IV = 4 }
public enum ResourceType {None, Test, Forest, Oak_Tree, Ebony_Tree, Stone, Iron_Vein, Copper_Vein, Silver_Vein, Gold_Vein, Fish_Shoal, Pearl_Reef, Wood, Cobblestone}

public class StorageManager : MonoBehaviour
{
    public static StorageManager Instance;
    [SerializeField] private CurrentLevelRuntimeData currentLevelRuntimeData;

    [SerializeField] private float baseStartCapacity = 100f;

    private Dictionary<ResourceType, int> inventory = new();
    private float currentVolume;    
    private float capacityForStorageBuildings;
    private float totalCapacity;
    private List<ResourceData> allResources = new();
    private Dictionary<ResourceType, ResourceData> resourceInfos = new();//По сути весь список находится в библиотеке ресурсов, но для удобства доступа к данным ресурсов при работе с инвентарем, я решил создать словарь, который будет хранить данные ресурсов по их типу.

    public int GetResourceCount(ResourceType type) => inventory.TryGetValue(type, out var count) ? count : 0;
    public float GetCurrentVolume() => currentVolume;
    public float GetTotalCapacity() => totalCapacity;

    public StorageManagerData GetStorageManagerData()
    {
        return new StorageManagerData
        {
            inventory = inventory.Select(kvp => new StorageManagerData.ResourceEntry(kvp.Key.ToString(), kvp.Value)).ToList(),
            capacityForStorageBuildings = capacityForStorageBuildings
        };

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
        InitializeInventory();
        InitializeResourceData();
        InitializeParameters();
        InitializeLevelParameters();
        InitializeEvents();
        IsLevelLoadFromSave();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameObject.name = "StorageManager";
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
        allResources = ResourceLibrary.GetAllResources();
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
    private void InitializeLevelParameters()
    {
        if (currentLevelRuntimeData != null && currentLevelRuntimeData.levelData != null)
        {
            foreach (var resource in currentLevelRuntimeData.levelData.startResources)
            {
                AddResource(resource.type, resource.amount);
            }
        }
        else
        {
            Debug.LogError("CurrentLevelRuntimeData or its levelData is not assigned in the inspector.");
        }
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnStorageBuilt += UpdateStorageCapacity;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnStorageBuilt -= UpdateStorageCapacity;
    }
    private void IsLevelLoadFromSave()
    {
        if (SaveManager.Instance.IsLoadLevelFromSave)
        {
            LoadStorageManagerData(SaveManager.Instance.LoadedLevelDates);
        }
    }
    private void LoadStorageManagerData(GameSessionData data)
    {
        StorageManagerData storageManagerData = data.storageManagerData;
        Dictionary<ResourceType, int> dataInventory = storageManagerData.inventory.ToDictionary(entry => Enum.Parse<ResourceType>(entry.resourceType), entry => entry.amount);
        LoadResourcesFromSave(dataInventory);
        totalCapacity = baseStartCapacity + capacityForStorageBuildings;
    }
    private void LoadResourcesFromSave(Dictionary<ResourceType, int> dataInventory)
    {
        foreach (var resource in dataInventory)
        {
            inventory[resource.Key] = resource.Value;
        }
    }

    private void Start()
    {
        InvokeStartEvents();
    }
    private void InvokeStartEvents()
    {
        EventBusManager.Instance.ResourceDataUpdated(allResources);
        foreach (var resource in inventory) EventBusManager.Instance.ResourceAmountUpdated(resource.Key, resource.Value);
        EventBusManager.Instance.StorageCapacityUpdated(totalCapacity);
    }

    private void Update()
    {
        CurrentVolumeUpdate();
    }
    private void CurrentVolumeUpdate()
    {
        float volumeOfResources = 0;
        foreach (var resource in inventory)
        {
            if (!resourceInfos.ContainsKey(resource.Key)) continue;
            volumeOfResources += inventory[resource.Key] * resourceInfos[resource.Key].VolumePerUnit;
        }
        currentVolume = volumeOfResources;
    }

    public bool AddResource(ResourceType type, int amount)
    {
        if (!resourceInfos.TryGetValue(type, out var info)) return false;

        float addedVolume = info.VolumePerUnit * amount;
        if (currentVolume + addedVolume > totalCapacity) return false;

        inventory[type] += amount;
        EventBusManager.Instance.ResourceAmountUpdated(type, inventory[type]);
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
            EventBusManager.Instance.ResourceAmountUpdated(type, inventory[type]);
        }
        
        return amountToConsume;
    }

    public bool HasResource(ResourceType type, int amount)
    {
        return inventory.TryGetValue(type, out var count) && count >= amount;
    }

    private void UpdateStorageCapacity(float changeCapacity)
    {
        capacityForStorageBuildings += changeCapacity;
        totalCapacity += baseStartCapacity + capacityForStorageBuildings;
        EventBusManager.Instance.StorageCapacityUpdated(totalCapacity);
    }
}