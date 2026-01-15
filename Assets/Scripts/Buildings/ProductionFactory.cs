using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32.SafeHandles;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class ProductionFactory : MonoBehaviour
{
    [Serializable]
    public class ProductionData
    {
        public string factoryName;
        public ResourceData outputResource;
        public int baseServiceCost;
        public float baseProductionTime;
        public int baseOutputAmount;

        [Space]
        public List<ProductionCondition> conditions = new();
    }
    [Serializable]
    public class ProductionCondition
    {
        public enum ConditionType { StorageResource, EnvironmentTile }
        
        public ResourceData requiredResource;
        public int requiredAmount;
        public ConditionType conditionType;
        [Space, Header("Storage Type")]
        public int requestedAmount;
        [Space, Header("Environment Type")]
        public int requiredTileRadius;
    }

    [SerializeField] private BuildingData buildingData;
    [SerializeField] private ProductionData productionData;
    [SerializeField] private float baseServiceTime;

    private Dictionary<ResourceType, int> storedResources = new();
    private Dictionary<ResourceType, int> resourcesInProduction = new();
    private Dictionary<ResourceType, List<Vector3Int>> allocatedCells = new();
    private HashSet<Vector3Int> blockedCells = new();
    
    private float currentProgress;
    private float currentEfficiency;

    private bool isPaused;
    private bool isOperational;
    private bool isInEditMode;
    private float serviceTime;

    public BuildingData FactoryBuildingData => buildingData;
    public ProductionData FactoryProductionData => productionData;
    public List<ProductionCondition> ProductionConditions => productionData.conditions;
    public float CurrentProgress => currentProgress;
    public bool IsPaused => isPaused;
    public bool IsOperational => isOperational;
    public int ServiceCost => productionData.baseServiceCost;
    
    private void Awake()
    {
        Initialize();
    }
    private void OnDestroy()
    {
        UninitializeFactory();
        UnitializeEvents();
    }
    private void Initialize()
    {
        InitializeFactory();
        InitializeResourceLists();
        InitializeConditions();
        InitializeParameters();
        InitializeEvents();
    }
    private void InitializeFactory()
    {
        ProductionManager.Instance.ManagerFactory(this);
    }
    private void UninitializeFactory()
    {
        ProductionManager.Instance.UnregisterFactory(this);
        TilemapManager.Instance.ReleaseCells(this);
    }
    private void InitializeResourceLists()
    {
        foreach (var resource in productionData.conditions)
        {
            if(resource.conditionType == ProductionCondition.ConditionType.StorageResource)
            {
                storedResources[resource.requiredResource.Type] = 0;
                resourcesInProduction[resource.requiredResource.Type] = 0;
            }
            else allocatedCells[resource.requiredResource.Type] = new List<Vector3Int>();
        }
    }
    private void InitializeConditions()
    {
        foreach (var condition in productionData.conditions)
        {
            condition.requestedAmount = condition.requiredAmount;
        }
        CheckForDuplicateConditions();
    }
    private void InitializeParameters()
    {
        currentEfficiency = 1f;
        isPaused = false;
        serviceTime = baseServiceTime;
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnResourceDeleted += ResourceDeleted; 
    }
    private void UnitializeEvents()
    {
        EventBusManager.Instance.OnResourceDeleted -= ResourceDeleted;
    }
    
    private void CheckForDuplicateConditions()
    {
        HashSet<ResourceType> conditionTypes = new();
        foreach (var condition in productionData.conditions)
        {
            if (conditionTypes.Contains(condition.requiredResource.Type)) Debug.LogError($"В {productionData.factoryName} имеется дубликат условия!");
            else conditionTypes.Add(condition.requiredResource.Type);
        }
    }

    public void UpdateProduction(float deltaTime)
    {
        if (isInEditMode) return;
        if (IsPaused) return;
        PullRequiredResources();
        CalculateProductionEfficiency();
        UpdateActiveStatus();

        if (!isOperational) return;
        CompleteProductionCycle(deltaTime);
    }    
    private void PullRequiredResources()
    {
        foreach (var condition in productionData.conditions)
        {
            switch (condition.conditionType)
            {
                case ProductionCondition.ConditionType.StorageResource:
                    HandleStorageResourceRequirement(condition);
                    break;
                case ProductionCondition.ConditionType.EnvironmentTile:
                    AllocateEnvironmentTiles(condition);
                    break;
            }
        }
    }
    private void HandleStorageResourceRequirement(ProductionCondition condition)
    {
        if (currentProgress > 0.85f) return;
        int neededAmount = Mathf.Max(0, condition.requestedAmount - resourcesInProduction[condition.requiredResource.Type]);

        if (neededAmount > 0)
        {
            int amountTaken = StorageManager.Instance.ConsumeResource(condition.requiredResource.Type, neededAmount);

            if (amountTaken > 0)
            {
                storedResources[condition.requiredResource.Type] += amountTaken;
                resourcesInProduction[condition.requiredResource.Type] = storedResources[condition.requiredResource.Type];
            }
        }
    }
    private void AllocateEnvironmentTiles(ProductionCondition condition)
    {
        ResourceType resourceType = condition.requiredResource.Type;
        
        int needed = Mathf.Max(0, condition.requiredAmount - allocatedCells[resourceType].Count);

        if (needed > 0)
        {
            List<Vector3Int> resourceCellsInRadius = TilemapManager.Instance.GetResourceCellsInRadius(transform.position, resourceType, condition.requiredTileRadius);
            List<Vector3Int> availableCells = new List<Vector3Int>();
            foreach (var tile in resourceCellsInRadius)
            {
                if (TilemapManager.Instance.IsCellOccupied(tile)) continue;
                if (blockedCells.Contains(tile)) continue;
                availableCells.Add(tile);
            }
            
            int toAllocate = Mathf.Min(needed, availableCells.Count);
            
            if (toAllocate > 0)
            {
                List<Vector3Int> newTiles = availableCells.GetRange(0, toAllocate);
                
                allocatedCells[resourceType].AddRange(newTiles);
                TilemapManager.Instance.OccupyCells(newTiles, this);

                availableCells.RemoveRange(0, toAllocate);
                blockedCells.AddRange(availableCells);
            }
        }
    }
    private void CalculateProductionEfficiency()
    {
        float currentMinEfficiency = 1f;
        foreach(var condition in productionData.conditions)
        {
            float amount = 0f;
            switch (condition.conditionType)
            {
                case ProductionCondition.ConditionType.StorageResource:
                    amount = resourcesInProduction[condition.requiredResource.Type];
                    break;
                case ProductionCondition.ConditionType.EnvironmentTile:
                    amount = allocatedCells[condition.requiredResource.Type].Count;
                    break;
            }

            float efficiency = Mathf.Clamp01(amount / condition.requiredAmount);
            
            if (efficiency < currentMinEfficiency) currentMinEfficiency = efficiency;
        }
        currentEfficiency = currentMinEfficiency;
    }
    private void UpdateActiveStatus()
    {
        isOperational = true;
        if(currentEfficiency <= 0.01f)
        {
            isOperational = false;
        }
    }
    private void CompleteProductionCycle(float deltaTime)
    {
        float productionTime = productionData.baseProductionTime / currentEfficiency;
        currentProgress += deltaTime / productionTime;
        if (currentProgress < 1f) return;

        if(StorageManager.Instance.AddResource(productionData.outputResource.Type, productionData.baseOutputAmount))
        currentProgress = 0f;
        DeductionConsumedResources();
    }
    private void DeductionConsumedResources()
    {
        foreach (var resource in productionData.conditions)
        {
            if (resource.conditionType == ProductionCondition.ConditionType.StorageResource)
            {
                storedResources[resource.requiredResource.Type] -= resourcesInProduction[resource.requiredResource.Type];
                resourcesInProduction[resource.requiredResource.Type] = 0;
            }
        }
    }

    public void ChangeRequestedAmount(int amount, ResourceType resource)
    {
        foreach (var condition in productionData.conditions)
        {
            if (condition.requiredResource.Type == resource) condition.requestedAmount = amount;
        }
    }

    private void ResourceDeleted(Vector3Int cell)
    {
        foreach (var kvp in allocatedCells)
        {
            if (kvp.Value.Contains(cell))
            {
                allocatedCells[kvp.Key].Remove(cell);
                TilemapManager.Instance.ReleaseCell(cell);
            }
        }
        
    }

    public void HandleCellOccupy(Vector3Int cell, ProductionCondition condition)
    {
        blockedCells.Remove(cell);
        allocatedCells[condition.requiredResource.Type].Add(cell);
        TilemapManager.Instance.OccupyCell(cell, this);
    }
    public void HandleCellRelease(Vector3Int cell, ProductionCondition condition)
    {
        blockedCells.Add(cell);
        allocatedCells[condition.requiredResource.Type].Remove(cell);
        TilemapManager.Instance.ReleaseCell(cell);
    }

    public void HandleCellRemoved(Vector3Int cell)
    {
        foreach (var resourceType in allocatedCells.Keys.ToList())
        {
            if (allocatedCells[resourceType].Contains(cell)) allocatedCells[resourceType].Remove(cell);
        }
    }

    public int GetAmountResourceInProduction(ResourceType resourceType)
    {
        int amount = 0;
        return amount = allocatedCells.ContainsKey(resourceType) ? allocatedCells[resourceType].Count : resourcesInProduction[resourceType];
    }

    public void ServiceUpdate(float deltaTime)
    {
        serviceTime -= deltaTime;
        if (serviceTime <= 0)
        {
            serviceTime = baseServiceTime;
            CurrencyManager.Instance.SpendMoney(productionData.baseServiceCost);
        }
    }

    public void SetPaused(bool pause)
    {
        isPaused = pause;
        ProductionManager.Instance.ManagerFactory(this);
    }
    public void SetEditPaused(bool pause)
    {
        isInEditMode = pause;
    }

    private void OnValidate()
    {
        CheckForDuplicateConditions();
    }
}
