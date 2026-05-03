using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameSessionData
{
    public DateTime saveDateTime;
    public string saveGameVersion;

    public TilemapManagerData tilemapManagerData;
    public BuildingManagerData buildingManagerData;
    public CurrencyManagerData currencyManagerData;
    public StorageManagerData storageManagerData;
    public QuestManagerData questManagerData;
    public OrdersManagerData ordersManagerData;
    public ProductionManagerData productionManagerData;
}
[Serializable]
public class GameSessionMeta
{
    public int index;
    public string sceneName;
}
[Serializable]
public class TilemapManagerData
{
    public List<TilemapLayerData> groundTiles;
    public List<TilemapLayerData> resourceTiles;
    public List<TilemapLayerData> buildingTiles;

    [Serializable]
    public class TilemapLayerData
    {
        public int x;
        public int y;
        public string tileName;
    }
}
[Serializable]
public class BuildingManagerData
{
    public List<BuildingRecordData> buildingRecordDataList;

    [Serializable]
    public class BuildingRecordData
    {
        public string buildingDataKey;
        public SerializableVector3Int startCell;
        public List<SerializableVector3Int> occupiedCells;//Возможно, стоит сохранять только начальную клетку и тип здания, а остальные клетки восстанавливать при загрузке, исходя из размера здания. Но пока так.
    }
}
[Serializable]
public class CurrencyManagerData
{
    public int currentMoney;
    public float bankruptcyProcess;
    public bool isAtRiskOfBankruptcy;
}
[Serializable]
public class StorageManagerData
{
    public List<ResourceEntry> inventory = new();
    public float capacityForStorageBuildings;
    [Serializable]
    public class ResourceEntry
    {
        public string resourceType;
        public int amount;

        public ResourceEntry(string resourceType, int amount)
        {
            this.resourceType = resourceType;
            this.amount = amount;
        }
        public ResourceType GetResourceType()
        {
            return Enum.TryParse(resourceType, out ResourceType result) ? result : ResourceType.None;
        }
    }
}
[Serializable]
public class QuestManagerData
{
    public List<QuestSaveData> quests;

    [Serializable]
    public class QuestSaveData
    {
        public string questType;
        public bool isCompleted;

        public int targetAmount;
        public string resourceName;
        public string buildingID;
        public int targetBuildingAmount;
        public string targetPeriod;

        public int currentAmount;
    }
}
[Serializable]
public class OrdersManagerData
{
    public string currentPeriod;
    public List<ResourceDemandEntry> resourceDemand = new();
    public List<OrderSaveData> acceptedOrders = new();
    public List<OrderSaveData> activeOrders = new();

    [Serializable]
    public class ResourceDemandEntry
    {
        public string resourceName;
        public float demand;
        public ResourceDemandEntry(string resourceName, float demand)
        {
            this.resourceName = resourceName;
            this.demand = demand;
        }
        public ResourceData GetResourceData()
        {
            return ResourceLibrary.GetResource(resourceName);
        }
    }
    [Serializable]
    public class OrderSaveData
    {
        public string id;
        public string resourceName;
        public float existenceTime;
        public float completionTime;
        public int resourceAmount;
        public int reward;
        public OrderSaveData(string id, string resourceName, float existenceTime, float completionTime, int resourceAmount, int reward)
        {
            this.id = id;
            this.resourceName = resourceName;
            this.existenceTime = existenceTime;
            this.completionTime = completionTime;
            this.resourceAmount = resourceAmount;
            this.reward = reward;
        }
        public OrderData ToOrderData()
        {
            return new OrderData
            (
                id,
                existenceTime,
                completionTime,
                ResourceLibrary.GetResource(resourceName),
                resourceAmount,
                reward
            );
        }
    }
    [Serializable]
    public class ProductionManagerData
    {
        public List<ProductionFactory> allFactories;
        public List<ProductionFactory> activeFactories;
    }
}
[Serializable]
public class ProductionManagerData
{
    public List<ProductionFactorySaveData> allFactories;

    [Serializable]
    public class ProductionFactorySaveData
    {
        public SerializableVector3Int originCell;
        public List<ResourceEntry> storedResources;
        public List<AllocatedCellEntry> allocatedCells;
        public List<SerializableVector3Int> blockedCells;
        public float currentProgress;
        public bool isPaused;
        public float serviceTime;
    }
    [Serializable]
    public class ResourceEntry
    {
        public string resourceName;
        public int amount;
    }
    [Serializable]
    public class AllocatedCellEntry
    {
        public string resourceType;
        public List<SerializableVector3Int> cells;
    }

}
[Serializable]
public struct SerializableVector3Int
{
    public int x;
    public int y;
    public int z;
    public SerializableVector3Int(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public static SerializableVector3Int ToSerializableVector3Int(Vector3Int vector)
    {
        return new SerializableVector3Int(vector.x, vector.y, vector.z);
    }
    public Vector3Int ToVector3Int()
    {
        return new Vector3Int(x, y, z);
    }
}