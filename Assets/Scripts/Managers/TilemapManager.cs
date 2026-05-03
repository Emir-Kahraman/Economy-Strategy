using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static WorldGenerator;

public enum TilemapType
{
    Ground,
    Resources,
    Buildings
}

public class TilemapManager : MonoBehaviour
{
    public static TilemapManager Instance { get; private set; }
    [SerializeField] private CurrentLevelRuntimeData runtimeData;

    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap resourcesTilemap;
    [SerializeField] private Tilemap buildingTilemap;
    [SerializeField] private GameObject worldGeneratorPrefab;
    [Header("Ground Tiles")]
    [SerializeField] private TileBase waterTile;
    [SerializeField] private TileBase sandTile;
    [SerializeField] private TileBase mountainTile;
    [SerializeField] private TileBase earthTile;
    [SerializeField] private TileBase riverTile;
    [Header("River Mouth Tiles")]
    [SerializeField] private TileBase riverMouthUpTile;
    [SerializeField] private TileBase riverMouthDownTile;
    [SerializeField] private TileBase riverMouthLeftTile;
    [SerializeField] private TileBase riverMouthRightTile;
    [Header("Resource Tiles")]
    [SerializeField] private TileBase forestTile;
    [SerializeField] private TileBase oakTreeTile;
    [SerializeField] private TileBase ebonyTreeTile;
    [SerializeField] private TileBase stoneTile;
    [SerializeField] private TileBase iron_VeinTile;
    [SerializeField] private TileBase copper_VeinTile;
    [SerializeField] private TileBase silver_VeinTile;
    [SerializeField] private TileBase gold_VeinTile;
    [SerializeField] private TileBase fish_ShoalTile;
    [SerializeField] private TileBase pearlReefTile;

    private string groundTilemapTag = "Ground Tilemap";
    private string resourcesTilemapTag = "Resources Tilemap";
    private string buildingTilemapTag = "Buildings Tilemap";

    private Dictionary<Vector3Int, ResourceType> resourceCellsCache = new();
    private Dictionary<Vector3Int, ProductionFactory> occupiedCells = new();

    public TilemapManagerData GetTilemapManagerData()
    {
        TilemapManagerData data = new TilemapManagerData
        {
            groundTiles = new List<TilemapManagerData.TilemapLayerData>(),
            resourceTiles = new List<TilemapManagerData.TilemapLayerData>(),
            buildingTiles = new List<TilemapManagerData.TilemapLayerData>()
        };
        SaveTilemapLayer(groundTilemap, data.groundTiles);
        SaveTilemapLayer(resourcesTilemap, data.resourceTiles);
        SaveTilemapLayer(buildingTilemap, data.buildingTiles);
        return data;
    }
    private void SaveTilemapLayer(Tilemap tilemap, List<TilemapManagerData.TilemapLayerData> layerData)
    {
        BoundsInt bounds = tilemap.cellBounds;

        foreach (var position in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(position);
            if (tile != null)
            {
                var tileInfo = new TilemapManagerData.TilemapLayerData
                {
                    x = position.x,
                    y = position.y,
                    tileName = tile.name
                };
                layerData.Add(tileInfo);
            }
        }
    }

    private void Awake()
    {
        Initialize();        
    }
    private void OnDestroy()
    {
        UninitializeEvents();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameObject.name = "TilemapManager";
    }
    private void Initialize()
    {
        InitializeSingleton();
        InitializeEvents();
        InitializeTilemaps();
        InitializeWorldGenerator();
        InitializeResourceCache();
        IsLoadLevelFromSave();
    }
    private void InitializeTilemaps()
    {
        groundTilemap = GameObject.FindWithTag(groundTilemapTag).GetComponent<Tilemap>();
        resourcesTilemap = GameObject.FindWithTag(resourcesTilemapTag).GetComponent<Tilemap>();
        buildingTilemap = GameObject.FindWithTag(buildingTilemapTag).GetComponent<Tilemap>();
    }
    private void InitializeWorldGenerator()
    {
        if (SceneManager.GetActiveScene().buildIndex != 2) return;
        if (worldGeneratorPrefab == null)
        {
            Debug.LogError("World Generator Prefab is not assigned in the inspector!");
            return;
        }

        GameObject worldGeneratorGO = Instantiate(worldGeneratorPrefab);
        worldGeneratorGO.GetComponent<WorldGenerator>().Initialize(runtimeData);
    }
    private void InitializeResourceCache()
    {
        BoundsInt bounds = resourcesTilemap.cellBounds;
        
        foreach (var cell in bounds.allPositionsWithin)
        {
            if (TryGetCellResourceType(cell, out ResourceType type))
            {
                resourceCellsCache[cell] = type;
            }
        }
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnResourceBuilt += BuiltResource;
        EventBusManager.Instance.OnBuildingBuilt += BuiltBuilding;
        EventBusManager.Instance.OnResourceDeleted += DeletedResource;
        EventBusManager.Instance.OnBuildingDeleted += DeletedBuilding;
        EventBusManager.Instance.OnCreateGroundTile += CreateGroundTile;
        EventBusManager.Instance.OnCreateResourceTile += CreateResourceTile;
        EventBusManager.Instance.OnCreateRiverMouthTile += CreateRiverMouthTile;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnResourceBuilt -= BuiltResource;
        EventBusManager.Instance.OnBuildingBuilt -= BuiltBuilding;
        EventBusManager.Instance.OnResourceDeleted -= DeletedResource;
        EventBusManager.Instance.OnBuildingDeleted -= DeletedBuilding;
        EventBusManager.Instance.OnCreateGroundTile -= CreateGroundTile;
        EventBusManager.Instance.OnCreateResourceTile -= CreateResourceTile;
        EventBusManager.Instance.OnCreateRiverMouthTile -= CreateRiverMouthTile;
    }
    private void IsLoadLevelFromSave()
    {
        if (SaveManager.Instance.IsLoadLevelFromSave)
        {
            LoadTilemapManagerData(SaveManager.Instance.LoadedLevelDates);
        }
    }

    private void LoadTilemapManagerData(GameSessionData gameSessionData)
    {
        TilemapManagerData data = gameSessionData.tilemapManagerData;
        groundTilemap = LoadTilemapLayer(groundTilemap, data.groundTiles);
        resourcesTilemap = LoadTilemapLayer(resourcesTilemap, data.resourceTiles);
        buildingTilemap = LoadTilemapLayer(buildingTilemap, data.buildingTiles);
    }
    private Tilemap LoadTilemapLayer(Tilemap tilemap, List<TilemapManagerData.TilemapLayerData> tiles)
    {
        tilemap.ClearAllTiles();

        foreach (var tileData in tiles)
        {
            TileBase tile = GetTileByName(tileData.tileName);
            if (tile != null)
            {
                tilemap.SetTile(new Vector3Int(tileData.x, tileData.y, 0), tile);
            }
            else
            {
                Debug.LogError($"Tile with name {tileData.tileName} not found!");
            }
        }
        return tilemap;
    }
    private TileBase GetTileByName(string tileName)
    {
        return Resources.Load<TileBase>($"Pallets/Tiles/{tileName}");
    }


    private bool TryGetCellResourceType(Vector3Int cell, out ResourceType tileResourceType)
    {
        tileResourceType = ResourceType.None;
        
        if (!resourcesTilemap.HasTile(cell)) return false;

        GameObject tileGO = resourcesTilemap.GetInstantiatedObject(cell);
        if (tileGO == null) return false;

        TileData data = tileGO.GetComponent<TileData>();
        if (data == null) return false;

        tileResourceType = data.resourceType;
        return true;
    }

    public Vector3 GetMapCenter()
    {
        BoundsInt bounds = groundTilemap.cellBounds;
        Vector3Int min = new Vector3Int(int.MaxValue, int.MaxValue, 0);
        Vector3Int max = new Vector3Int(int.MinValue, int.MinValue, 0);
        bool hasAnyTile = false;

        foreach (var cellPos in bounds.allPositionsWithin)
        {
            if (groundTilemap.HasTile(cellPos))
            {
                hasAnyTile = true;
                if (cellPos.x < min.x) min.x = cellPos.x;
                if (cellPos.y < min.y) min.y = cellPos.y;
                if (cellPos.x > max.x) max.x = cellPos.x;
                if (cellPos.y > max.y) max.y = cellPos.y;
            }
        }

        if (!hasAnyTile)
        {
            Debug.LogWarning("No tiles found on groundTilemap, returning zero.");
            return Vector3.zero;
        }

        Vector3Int centerCell = new Vector3Int(
            (min.x + max.x) / 2,
            (min.y + max.y) / 2,
            0
        );
        return groundTilemap.GetCellCenterWorld(centerCell);
    }
    public void GetWorldBounds(out float minX, out float maxX, out float minY, out float maxY)
    {
        if (groundTilemap == null)
        {
            minX = maxX = minY = maxY = 0;
            return;
        }

        BoundsInt bounds = groundTilemap.cellBounds;
        bool hasTile = false;
        float minWorldX = float.MaxValue, maxWorldX = float.MinValue;
        float minWorldY = float.MaxValue, maxWorldY = float.MinValue;

        foreach (var cellPos in bounds.allPositionsWithin)
        {
            if (groundTilemap.HasTile(cellPos))
            {
                hasTile = true;
                Vector3 worldPos = groundTilemap.GetCellCenterWorld(cellPos);

                float left = worldPos.x - 0.5f;
                float right = worldPos.x + 0.5f;
                float bottom = worldPos.y - 0.5f;
                float top = worldPos.y + 0.5f;

                if (left < minWorldX) minWorldX = left;
                if (right > maxWorldX) maxWorldX = right;
                if (bottom < minWorldY) minWorldY = bottom;
                if (top > maxWorldY) maxWorldY = top;
            }
        }

        if (!hasTile)
        {
            minX = maxX = minY = maxY = 0;
            return;
        }

        minX = minWorldX;
        maxX = maxWorldX;
        minY = minWorldY;
        maxY = maxWorldY;
    }

    public ProductionFactory TryGetProductionFactory(Vector3 position)
    {
        Vector3Int cell = buildingTilemap.WorldToCell(position);
        if (!BuildingManager.Instance.TryGetMainFactoryCell(cell, out Vector3Int mainCell)) return null;

        if(!buildingTilemap.HasTile(mainCell)) return null;

        GameObject tileGO = buildingTilemap.GetInstantiatedObject(mainCell);
        if(tileGO == null) return null;

        ProductionFactory productionFactory = tileGO.GetComponent<ProductionFactory>();
        if(productionFactory == null) return null;

        return productionFactory;
    }

    public ProductionFactory GetProductionFactoryInResourceCell(Vector3Int cell)
    {
        if (!occupiedCells.ContainsKey(cell)) return null;
        return occupiedCells[cell];
    }

    public Tilemap GetTilemapOfType(TilemapType type)
    {
        switch (type)
        {
            case TilemapType.Ground: return groundTilemap;
            case TilemapType.Resources: return resourcesTilemap;
            case TilemapType.Buildings: return buildingTilemap;
            default: return null;
        }
    }

    public TerrainType GetTerrainType(Vector3Int cell)
    {
        if (groundTilemap.GetTile(cell))
        {
            return groundTilemap.GetInstantiatedObject(cell).GetComponent<TileData>().terrainType;
        }
        return TerrainType.None;
    }

    public ResourceType GetResourceType(Vector3Int cell)
    {
        if (resourcesTilemap.GetTile(cell))
        {
            return resourcesTilemap.GetInstantiatedObject(cell).GetComponent<TileData>().resourceType;
        }
        return ResourceType.None;
    }

    public List<Vector3Int> GetResourceCellsInRadius(Vector3 center, ResourceType tileType, int  radius)
    {
        Vector3Int centerCell = resourcesTilemap.WorldToCell(center);
        List<Vector3Int> tilesInRadius = new();

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int cell = centerCell + new Vector3Int(x, y, 0);
                
                if (resourceCellsCache.TryGetValue(cell, out ResourceType type) && type == tileType)
                {
                    tilesInRadius.Add(cell);
                }
            }
        }
        return tilesInRadius;
    }

    public void OccupyCell(Vector3Int cell, ProductionFactory factory)
    {
        occupiedCells[cell] = factory;
    }
    public void OccupyCells(List<Vector3Int> cells, ProductionFactory factory)
    {
        foreach (var cell in cells) occupiedCells[cell] = factory;
    }

    public void ReleaseCell(Vector3Int cell)
    {
        occupiedCells.Remove(cell);
    }
    public void ReleaseCells(ProductionFactory factory)
    {
        List<Vector3Int> toRemove = new();
        foreach (var occupiedCell in occupiedCells) if (occupiedCell.Value == factory) toRemove.Add(occupiedCell.Key);
        foreach (var cell in toRemove) occupiedCells.Remove(cell);
    }

    private void BuiltResource(Vector3Int cell, BuildingData data)
    {
        resourcesTilemap.SetTile(cell, data.mainTile);
        if (TryGetCellResourceType(cell, out ResourceType tileResource))
        {
            resourceCellsCache.Add(cell, tileResource);
        }
        else
        {
            Debug.LogError("Тип ресурса не найден!");
        }
        
    }
    private void DeletedResource(Vector3Int cell)
    {
        resourcesTilemap.SetTile(cell, null);
        resourceCellsCache.Remove(cell);
    }

    private void BuiltBuilding(Vector3Int cell, BuildingData data)
    {
        buildingTilemap.SetTile(cell, data.mainTile);
        if (data.size != new Vector2Int(1, 1))
        {
            int index = 0;
            for (int x = 0; x < data.size.x; x++)
            {
                for (int y = 0;  y < data.size.y; y++)
                {
                    if (x == 0 && y == 0) continue;
                    Vector3Int cellPos = cell + new Vector3Int(x, y);
                    buildingTilemap.SetTile(cellPos, data.secondaryTiles[index]);
                    index++;
                }
            }
        }

    }
    private void DeletedBuilding(Vector3Int cell, BuildingData data)
    {
        buildingTilemap.SetTile(cell, null);
        if (data.size != new Vector2Int(1, 1))
        {
            int index = 0;
            for (int x = 0; x < data.size.x; x++)
            {
                for (int y = 0; y < data.size.y; y++)
                {
                    if (x == 0 && y == 0) continue;
                    Vector3Int cellPos = cell + new Vector3Int(x, y);
                    buildingTilemap.SetTile(cellPos, null);
                    index++;
                }
            }
        }
    }

    public bool IsCellOccupied(Vector3Int cell)
    {
        if (!occupiedCells.ContainsKey(cell)) return false;
        if (occupiedCells[cell] == null) return false;
        return true;
    }

    private void CreateGroundTile(Vector3Int cell, TerrainType type)
    {
        switch (type)
        {
            case TerrainType.Ground: groundTilemap.SetTile(cell, earthTile); break;
            case TerrainType.Water: groundTilemap.SetTile(cell, waterTile); break;
            case TerrainType.Sand: groundTilemap.SetTile(cell, sandTile); break;
            case TerrainType.Mountain: groundTilemap.SetTile(cell, mountainTile); break;
            case TerrainType.River: groundTilemap.SetTile(cell, riverTile); break;
            default: Debug.LogError("Incorrect TerrainType!"); break;
        }
    }
    private void CreateResourceTile(Vector3Int cell, ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Forest: resourcesTilemap.SetTile(cell, forestTile); break;
            case ResourceType.Oak_Tree: resourcesTilemap.SetTile(cell, oakTreeTile); break;
            case ResourceType.Ebony_Tree: resourcesTilemap.SetTile(cell, ebonyTreeTile); break;
            case ResourceType.Stone: resourcesTilemap.SetTile(cell, stoneTile); break;
            case ResourceType.Iron_Vein: resourcesTilemap.SetTile(cell, iron_VeinTile); break;
            case ResourceType.Copper_Vein: resourcesTilemap.SetTile(cell, copper_VeinTile); break;
            case ResourceType.Silver_Vein: resourcesTilemap.SetTile(cell, silver_VeinTile); break;
            case ResourceType.Gold_Vein: resourcesTilemap.SetTile(cell, gold_VeinTile); break;
            case ResourceType.Fish_Shoal: resourcesTilemap.SetTile(cell, fish_ShoalTile); break;
            case ResourceType.Pearl_Reef: resourcesTilemap.SetTile(cell, pearlReefTile); break;
            default: Debug.LogError("Incorrect ResourceType!"); break;
        }
    }
    private void CreateRiverMouthTile(Vector3Int cell, Vector2Int direction)
    {
        switch (direction)
        {
            case Vector2Int up when up == Vector2Int.up: groundTilemap.SetTile(cell, riverMouthUpTile); break;
            case Vector2Int down when down == Vector2Int.down: groundTilemap.SetTile(cell, riverMouthDownTile); break;
            case Vector2Int left when left == Vector2Int.left: groundTilemap.SetTile(cell, riverMouthLeftTile); break;
            case Vector2Int right when right == Vector2Int.right: groundTilemap.SetTile(cell, riverMouthRightTile); break;
            default: Debug.LogError("Incorrect River Mouth Direction!"); break;
        }
    }

    public bool IsCoastalCell(Vector3Int cell)
    {
        TerrainType type = GetTerrainType(cell);
        if (type != TerrainType.Water) return false;
        
        List<Vector3Int> cellsInRadius = new();
        for (int x = cell.x - 2; x < cell.x + 2; x++)
        {
            for (int y = cell.y - 2; y < cell.y + 2; y++)
            {
                if (x == 0 && y == 0) continue;

                cellsInRadius.Add(new Vector3Int(x, y));
            }
        }

        foreach (var cellInRadius in cellsInRadius)
        {
            if (GetTerrainType(cellInRadius) == TerrainType.Sand)
            {
                return true;
            }
        }
        return false;
    }
}
