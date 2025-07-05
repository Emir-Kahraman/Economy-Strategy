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
        public ResourceType outputResource;
        public float baseProductionTime;
        public int baseOutputAmount;
        public int priority;

        [Space]
        public List<ProductionCondition> conditions = new();
    }
    [System.Serializable]
    public class ProductionCondition
    {
        public enum ConditionType { StorageResource, EnvironmentTile }
        
        public ResourceType requiredResource;
        public int requiredAmount;
        public ConditionType conditionType;
        [Space, Header("Storage Type")]
        public int requestedAmount;
        [Space, Header("Environment Type")]
        public int requiredTileRadius;
    }

    [SerializeField] private ProductionData productionData;

    private Dictionary<ResourceType, int> storedResources = new();
    private Dictionary<ResourceType, int> resourcesInProduction = new();
    private Dictionary<ResourceType, List<Vector3Int>> allocatedCells = new();
    private HashSet<Vector3Int> blockedCells = new();
    
    private float currentProgress;
    private float currentEfficiency = 1f;

    private bool isPaused = false;
    private bool isActive;

    public string FactoryName => productionData.factoryName;
    public int Priority => productionData.priority;
    public float CurrentProgress => currentProgress;
    public List<ProductionCondition> ProductionConditions => productionData.conditions;
    public bool IsPaused => isPaused;
    public bool IsActive => isActive;
    
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
                storedResources[resource.requiredResource] = 0;
                resourcesInProduction[resource.requiredResource] = 0;
            }
            else allocatedCells[resource.requiredResource] = new List<Vector3Int>();
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
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnResourceTilemapUpdated += BlockedCell;
    }
    private void UnitializeEvents()
    {
        EventBusManager.Instance.OnResourceTilemapUpdated -= BlockedCell;
    }
    
    private void CheckForDuplicateConditions()
    {
        HashSet<ResourceType> conditionTypes = new();
        foreach (var condition in productionData.conditions)
        {
            if (conditionTypes.Contains(condition.requiredResource)) Debug.LogError($"В {productionData.factoryName} имеется дубликат условия!");
            else conditionTypes.Add(condition.requiredResource);
        }
    }

    public void UpdateProduction(float deltaTime)
    {
        if (IsPaused) return;
        PullRequiredResources();
        CalculateProductionEfficiency();
        UpdateActiveStatus();

        if (!isActive) return;
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
        int neededAmount = Mathf.Max(0, condition.requestedAmount - resourcesInProduction[condition.requiredResource]);

        if (neededAmount > 0)
        {
            int amountTaken = StorageManager.Instance.ConsumeResource(condition.requiredResource, neededAmount);

            if (amountTaken > 0)
            {
                storedResources[condition.requiredResource] += amountTaken;
                resourcesInProduction[condition.requiredResource] = storedResources[condition.requiredResource];
            }
        }
    }
    private void AllocateEnvironmentTiles(ProductionCondition condition)//срабатывает только в начале по сути, требуется полное изменение логики.
    {
        ResourceType resourceType = condition.requiredResource;
        
        int needed = Mathf.Max(0, condition.requiredAmount - allocatedCells[resourceType].Count);

        if (needed > 0)
        {
            List<Vector3Int> resourceCellsInRadius = TilemapManager.Instance.GetCellsInRadius(transform.position, resourceType, condition.requiredTileRadius);
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
                    amount = resourcesInProduction[condition.requiredResource];
                    break;
                case ProductionCondition.ConditionType.EnvironmentTile:
                    amount = allocatedCells[condition.requiredResource].Count;
                    break;
            }

            float efficiency = Mathf.Clamp01(amount / condition.requiredAmount);
            
            if (efficiency < currentMinEfficiency) currentMinEfficiency = efficiency;
        }
        currentEfficiency = currentMinEfficiency;
    }
    private void UpdateActiveStatus()
    {
        isActive = true;
        if(currentEfficiency <= 0.01f)
        {
            isActive = false;
        }
    }
    private void CompleteProductionCycle(float deltaTime)
    {
        float productionTime = productionData.baseProductionTime / currentEfficiency;
        currentProgress += deltaTime / productionTime;
        if (currentProgress < 1f) return;

        if(StorageManager.Instance.AddResource(productionData.outputResource, productionData.baseOutputAmount))
        currentProgress = 0f;
        DeductionConsumedResources();
    }
    private void DeductionConsumedResources()
    {
        foreach (var resource in productionData.conditions)
        {
            if (resource.conditionType == ProductionCondition.ConditionType.StorageResource)
            {
                storedResources[resource.requiredResource] -= resourcesInProduction[resource.requiredResource];
                resourcesInProduction[resource.requiredResource] = 0;
            }
        }
    }

    public void ChangeRequestedAmount(int amount, ResourceType resource)
    {
        foreach (var condition in productionData.conditions)
        {
            if (condition.requiredResource == resource) condition.requestedAmount = amount;
        }
    }

    private void BlockedCell(Vector3Int cell)
    {
        if (!blockedCells.Contains(cell)) blockedCells.Add(cell);
    }

    public void HandleCellOccupy(Vector3Int cell, ProductionCondition condition)
    {
        blockedCells.Remove(cell);
        allocatedCells[condition.requiredResource].Add(cell);
        TilemapManager.Instance.OccupyCell(cell, this);
    }
    public void HandleCellRelease(Vector3Int cell, ProductionCondition condition)
    {
        blockedCells.Add(cell);
        allocatedCells[condition.requiredResource].Remove(cell);
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

    public void SetPaused(bool pause)
    {
        isPaused = pause;
        ProductionManager.Instance.ManagerFactory(this);
    }

    private void OnValidate()
    {
        CheckForDuplicateConditions();
    }
}
