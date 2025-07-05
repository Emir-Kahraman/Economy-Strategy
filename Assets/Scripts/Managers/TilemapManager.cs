using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class TilemapManager : MonoBehaviour
{
    public static TilemapManager Instance { get; private set; }

    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap resourceTilemap;
    [SerializeField] private Tilemap buildingTilemap;
    
    private string groundTilemapTag = "Ground Tilemap";
    private string resourceTilemapTag = "Resources Tilemap";
    private string buildingTilemapTag = "Buildings Tilemap";

    private Dictionary<Vector3Int, ResourceType> resourceCellsCache = new();
    private Dictionary<Vector3Int, ProductionFactory> occupiedCells = new();

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
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "TilemapManager";
    }
    private void Initialize()
    {
        InitializeTilemapReferences();
        InitializeResourceCache();
        InitializeEvents();
    }
    private void InitializeTilemapReferences()
    {
        groundTilemap = GameObject.FindWithTag(groundTilemapTag).GetComponent<Tilemap>();
        resourceTilemap = GameObject.FindWithTag(resourceTilemapTag).GetComponent<Tilemap>();
        buildingTilemap = GameObject.FindWithTag(buildingTilemapTag).GetComponent<Tilemap>();

        groundTilemap.CompressBounds();
        resourceTilemap.RefreshAllTiles();
    }
    private void InitializeResourceCache()
    {
        BoundsInt bounds = resourceTilemap.cellBounds;

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
        EventBusManager.Instance.OnResourceTilemapUpdated += UpdateResourceCellCache;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnResourceTilemapUpdated -= UpdateResourceCellCache;
    }

    private bool TryGetCellResourceType(Vector3Int cell, out ResourceType tileResourceType)
    {
        tileResourceType = ResourceType.None;

        if (!resourceTilemap.HasTile(cell)) return false;

        GameObject tileGO = resourceTilemap.GetInstantiatedObject(cell);
        if (tileGO == null) return false;

        TileData data = tileGO.GetComponent<TileData>();
        if (data == null) return false;

        tileResourceType = data.resourceType;
        return true;
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

    public ProductionFactory GetProductionFactoryInResourceCell(Vector3Int cell)//
    {
        if (!occupiedCells.ContainsKey(cell)) return null;
        return occupiedCells[cell];
    }

    public Tilemap GetTilemapType(TilemapType type)
    {
        switch (type)
        {
            case TilemapType.Ground: return groundTilemap;
            case TilemapType.Resource: return resourceTilemap;
            case TilemapType.Obstacle: return buildingTilemap;
            default: return null;
        }
    }

    public List<Vector3Int> GetCellsInRadius(Vector3 center, ResourceType tileType, int  radius)
    {
        Vector3Int centerCell = resourceTilemap.WorldToCell(center);
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

    private void UpdateResourceCellCache(Vector3Int cell)
    {
        if (TryGetCellResourceType(cell, out ResourceType type)) 
            resourceCellsCache[cell] = type;
        else
        {
            resourceCellsCache.Remove(cell);
            HandleCellRemoved(cell);
        }
    }

    private void HandleCellRemoved(Vector3Int cell)
    {
        if (occupiedCells.TryGetValue(cell, out ProductionFactory factory))
        {
            occupiedCells.Remove(cell);
            factory.HandleCellRemoved(cell);
        }
    }

    public bool IsCellOccupied(Vector3Int cell)
    {
        if (!occupiedCells.ContainsKey(cell)) return false;
        if (occupiedCells[cell] == null) return false;
        return true;
    }
}
