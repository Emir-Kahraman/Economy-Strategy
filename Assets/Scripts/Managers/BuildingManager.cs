using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;


public class BuildingManager : MonoBehaviour
{
    public class BuildingRecord
    {
        public BuildingData data;
        public Vector3Int startCell;
        public List<Vector3Int> occupiedCells;
    }

    public static BuildingManager Instance;

    [Header("References")]
    public Tilemap groundTilemap;
    public Tilemap resourceTilemap;
    public Tilemap buildingTilemap;
    public TileBase groundTile;

    [Header("Building Ghost")]
    public GameObject highlightPrefab;

    private GameObject currentHighlight;
    private SpriteRenderer highlightSpriteRenderer;
    private BuildingData currentBuilding;

    private string groundTilemapTag = "Ground Tilemap";
    private string resourceTilemapTag = "Resources Tilemap";
    private string buildingTilemapTag = "Buildings Tilemap";

    private Dictionary<Vector3Int, BuildingRecord> buildingRegistry = new();

    private void Awake()
    {
        InitializeSingleton();
        Initialize();
    }
    void Update()
    {        
        if (GameModeManager.Instance.CurrentMode == GameModeManager.GameMode.Building && currentBuilding != null) Building();
        if (GameModeManager.Instance.CurrentMode == GameModeManager.GameMode.Demolition) Demolition();
        return;
    }

    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "BuildingManager";
    }
    private void Initialize()
    {
        InitializeObjects();
        CreateObjects();
    }
    private void InitializeObjects()
    {
        groundTilemap = GameObject.FindWithTag(groundTilemapTag).GetComponent<Tilemap>();
        resourceTilemap = GameObject.FindWithTag(resourceTilemapTag).GetComponent<Tilemap>();
        buildingTilemap = GameObject.FindWithTag(buildingTilemapTag).GetComponent<Tilemap>();
    }
    private void CreateObjects()
    {
        currentHighlight = Instantiate(highlightPrefab);
        highlightSpriteRenderer = currentHighlight.GetComponent<SpriteRenderer>();
        currentHighlight.SetActive(false);
    }

    private void Building()
    {
        Vector3Int cellPos = MouseToCellPositionOfGroundTilemap();
        bool isPlacementValid = IsPlacementValid(cellPos, out bool destroyableResource);        
        UpdateHighlight(cellPos, isPlacementValid);
        PlaceBuilding(cellPos, isPlacementValid, destroyableResource);
    }

    private Vector3Int MouseToCellPositionOfGroundTilemap()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return groundTilemap.WorldToCell(mouseWorldPos);
    }

    bool IsPlacementValid(Vector3Int cell, out bool destroyableResource)
    {
        destroyableResource = true;
        for (int x = 0; x < currentBuilding.size.x; x++)
        {
            for (int y = 0; y < currentBuilding.size.y; y++)
            {
                Vector3Int checkPos = cell + new Vector3Int(x, y);
                if (groundTilemap.GetTile(checkPos) != groundTile || buildingTilemap.GetTile(checkPos) != null) return false;

                if (resourceTilemap.GetTile(checkPos) != null)
                destroyableResource = TryGetDestructibleResource(checkPos) && currentBuilding.canReplaceResources ? true : false;
            }
        }
        return destroyableResource;
    }
    private void UpdateHighlight(Vector3Int cell, bool isPlacementValid)
    {
        Vector3 offset = new Vector3(
        (currentBuilding.size.x - 1) * groundTilemap.cellSize.x * 0.5f,
        (currentBuilding.size.y - 1) * groundTilemap.cellSize.y * 0.5f,
        0
        );
        currentHighlight.transform.position = groundTilemap.GetCellCenterWorld(cell) + offset;
        highlightSpriteRenderer.color = isPlacementValid ? new Color(0, 1, 0, 0.25f) : new Color(1, 0, 0, 0.25f);        
    }
    private void PlaceBuilding(Vector3Int cell, bool isPlacementValid, bool destroyableResource)
    {
        if(EventSystem.current.IsPointerOverGameObject()) return;
        if(CurrencyManager.Instance.GetCurrentMoney() < currentBuilding.cost) return;

        if (Input.GetMouseButtonDown(0) && isPlacementValid)
        {
            CurrencyManager.Instance.TrySpendMoney(currentBuilding.cost);

            if (destroyableResource)
            {
                for (int x = 0; x < currentBuilding.size.x; x++)
                {
                    for (int y = 0; y < currentBuilding.size.y; y++)
                    {
                        DeleteResourceTile(cell + new Vector3Int(x, y));
                    }
                }
            }

            Tilemap targetTilemap = TilemapManager.Instance.GetTilemapType(currentBuilding.tilemapType);
            targetTilemap.SetTile(cell, currentBuilding.mainTile);

            if (currentBuilding.tilemapType == TilemapType.Resource)
                EventBusManager.Instance.ResourceTilemapUpdated(cell);

            int index = 0;
            for (int x = 0; x < currentBuilding.size.x; x++)
            {
                for (int y = 0; y < currentBuilding.size.y; y++)
                {
                    if(x == 0 && y == 0) continue;

                    Vector3Int tellPos = cell + new Vector3Int(x, y);
                    targetTilemap.SetTile(tellPos, currentBuilding.secondaryTiles[index]);                    
                    index++;
                }
            }
            if(currentBuilding.tilemapType == TilemapType.Obstacle)
                RegisterBuilding(cell, currentBuilding);
        }
    }
    private void RegisterBuilding(Vector3Int startCell, BuildingData building)
    {
        var record = new BuildingRecord
        {
            data = building,
            startCell = startCell,
            occupiedCells = new()
        };

        for (int x = 0; x < building.size.x; x++)
        {
            for (int y = 0; y < building.size.y; y++)
            {
                Vector3Int cell = startCell + new Vector3Int(x, y);
                buildingRegistry[cell] = record;                
                record.occupiedCells.Add(cell);
            }
        }
    }

    private void Demolition()
    {
        Vector3Int cellPos = MouseToCellPositionOfGroundTilemap();
        bool canDemolish = IsDestructibleCell(cellPos) && CanDemolish(cellPos);
        UpdateDemolitionHighlight(cellPos, canDemolish);

        if (Input.GetMouseButtonDown(0) && canDemolish && !EventSystem.current.IsPointerOverGameObject())
            DemolishAt(cellPos);
    }
    private bool IsDestructibleCell(Vector3Int cell)
    {        
        return buildingTilemap.GetTile(cell) != null ||
               TryGetDestructibleResource(cell);
    }
    private bool CanDemolish(Vector3Int cell)
    {
        if (buildingTilemap.GetTile(cell) && buildingTilemap.GetInstantiatedObject(cell).TryGetComponent(out StorageBuilding building))
        {
            if (StorageManager.Instance.GetTotalCapacity() - building.GetCapacity() < StorageManager.Instance.GetCurrentVolume())
            {
                return false;
            }
        }
        return true;
    }
    private void UpdateDemolitionHighlight(Vector3Int cell, bool canDemolish)
    {
        currentHighlight.transform.position = groundTilemap.GetCellCenterWorld(cell);
        highlightSpriteRenderer.color = canDemolish ? new Color(1, 0, 0, 0.5f) : new Color(0, 0, 0, 0);
        currentHighlight.transform.localScale = Vector3.one;
    }
    private void DemolishAt(Vector3Int cell)
    {
        if(TryGetBuildingDataAt(cell, out BuildingData building, out Vector3Int startCell))
        {
            BuildingRecord record = buildingRegistry[startCell];

            foreach (Vector3Int tilePos in record.occupiedCells)
            {
                buildingTilemap.SetTile(tilePos, null);
                buildingRegistry.Remove(tilePos);
            }
        }
        else if(TryGetDestructibleResource(cell))
        {
            DeleteResourceTile(cell);
        }
    }
    private bool TryGetBuildingDataAt(Vector3Int cell, out BuildingData building, out Vector3Int startCell)
    {
        building = null;
        startCell = Vector3Int.zero;

        if (buildingRegistry.TryGetValue(cell, out BuildingRecord record)) {
            building = record.data;
            startCell = record.startCell;
            return true;
        }
        return false;
    }
    private bool TryGetDestructibleResource(Vector3Int cell)
    {
        if(resourceTilemap.GetTile(cell) == null) return false;

        GameObject resourceOjb = resourceTilemap.GetInstantiatedObject(cell);
        if(resourceOjb == null) return false;

        TileData tileData = resourceOjb.GetComponent<TileData>();

        return tileData != null && tileData.isDestructibleResource;
    }

    private void DeleteResourceTile(Vector3Int cell)
    {
        resourceTilemap.SetTile(cell, null);
        EventBusManager.Instance.ResourceTilemapUpdated(cell);
    }

    public bool TryGetMainFactoryCell(Vector3Int cell, out Vector3Int mainCell)
    {
        mainCell = Vector3Int.zero;
        if(buildingRegistry.TryGetValue(cell, out BuildingRecord record))
        {
            mainCell = record.startCell;
            return true;
        }
        return false;
    }

    public void CancelConstruction()
    {
        if(currentBuilding != null) CancelBuilding();
        else CancelDemolition();
    }

    public void StartBuilding(BuildingData data)
    {
        currentBuilding = data;
        currentHighlight.SetActive(true);
        currentHighlight.transform.localScale = new Vector3(data.size.x, data.size.y, 1);
    }    
    public void CancelBuilding()
    {
        currentBuilding = null;
        currentHighlight.SetActive(false);
    }

    public void StartDemolition()
    {
        currentHighlight.SetActive(true);
        EventBusManager.Instance.SwitchToDemolitionGameMode();
    }
    public void CancelDemolition()
    {
        currentHighlight.SetActive(false);
        EventBusManager.Instance.SwitchToObservationGameMode();
    }
}
