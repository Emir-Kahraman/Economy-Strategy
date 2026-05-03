using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class BuildingManager : MonoBehaviour
{
    public class BuildingRecord
    {
        public BuildingData data;
        public Vector3Int startCell;
        public List<Vector3Int> occupiedCells;
    }

    public static BuildingManager Instance;

    [SerializeField] private List<BuildingData> allBuildings;

    [Header("Building Ghost")]
    [SerializeField] private GameObject highlightPrefab;

    private GameObject currentHighlight;
    private SpriteRenderer highlightSpriteRenderer;
    private BuildingData currentBuilding;

    private Dictionary<Vector3Int, BuildingRecord> buildingRegistry = new();

    public BuildingManagerData GetBuildingManagerData()
    {
        BuildingManagerData data = new BuildingManagerData
        {
            buildingRecordDataList = buildingRegistry.Values.Select(record => new BuildingManagerData.BuildingRecordData
            {
                buildingDataKey = record.data.BuildingKey,
                startCell = new SerializableVector3Int(record.startCell.x, record.startCell.y, record.startCell.z),
                occupiedCells = record.occupiedCells.Select(cell => new SerializableVector3Int(cell.x, cell.y, cell.z)).ToList()
            }).ToList()
        };
        return data;
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
        InitializeObjects();
        InitializeEvents();
        IsLoadLevelFromSave();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameObject.name = "BuildingManager";
    }
    private void InitializeObjects()
    {
        currentHighlight = Instantiate(highlightPrefab);
        highlightSpriteRenderer = currentHighlight.GetComponent<SpriteRenderer>();
        currentHighlight.SetActive(false);
    }
    private void InitializeEvents()
    {
        
    }
    private void UninitializeEvents()
    {
        
    }
    private void IsLoadLevelFromSave()
    {
        if (SaveManager.Instance.IsLoadLevelFromSave)
        {
            LoadBuildingManagerData(SaveManager.Instance.LoadedLevelDates);
        }
    }
    private void LoadBuildingManagerData(GameSessionData gameSessionData)
    {
        BuildingManagerData data = gameSessionData.buildingManagerData;
        buildingRegistry.Clear();
        foreach (var recordData in data.buildingRecordDataList)
        {
            BuildingData buildingData = BuildingLibrary.GetBuilding(recordData.buildingDataKey);
            if (buildingData == null)
            {
                Debug.LogWarning($"Building with ID {recordData.buildingDataKey} not found in BuildingLibrary. Skipping.");
                continue;
            }

            RegisterBuilding(recordData.startCell.ToVector3Int(), buildingData);
        }
    }

    private void Start()
    {
        InvokeStartEvents();
    }

    private void InvokeStartEvents()
    {
        EventBusManager.Instance.BuildingDataUpdated(allBuildings);
    }

    void Update()
    {
        if (GameModeManager.Instance.CurrentMode == GameModeManager.GameMode.Building && currentBuilding != null) Building();
        if (GameModeManager.Instance.CurrentMode == GameModeManager.GameMode.Demolition) Demolition();
        return;
    }

    private void Building()
    {
        Vector3Int cellPos = MouseToCellPositionOfGroundTilemap();
        bool isPlacementValid = IsPlacementValid(cellPos);
        UpdateHighlight(cellPos, isPlacementValid);
        PlaceBuilding(cellPos, isPlacementValid);
    }

    private Vector3Int MouseToCellPositionOfGroundTilemap()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return GetTilemapFromTilemapManager(TilemapType.Ground).WorldToCell(mouseWorldPos);
    }

    private bool IsPlacementValid(Vector3Int cell)
    {
        bool isTerrainValid = true;
        bool isTerrainClear = true;
        bool canPlace = true;

        List<Vector3Int> list = new();
        for (int x = 0; x < currentBuilding.size.x; x++)
        {
            for (int y = 0; y < currentBuilding.size.y; y++)
            {
                Vector3Int checkPos = cell + new Vector3Int(x, y);
                list.Add(checkPos);
                if (GetTilemapFromTilemapManager(TilemapType.Ground).GetTile(checkPos) != currentBuilding.placementRequirement) isTerrainValid = false;
                if (!IsTerrainClear(checkPos)) isTerrainClear = false;
            }
        }
        canPlace = isTerrainValid & isTerrainClear;
        return canPlace;
    }
    private void UpdateHighlight(Vector3Int cell, bool isPlacementValid)
    {
        Vector3 offset = new Vector3(
        (currentBuilding.size.x - 1) * 0.5f,
        (currentBuilding.size.y - 1) * 0.5f,
        0);
        currentHighlight.transform.position = GetTilemapFromTilemapManager(TilemapType.Ground).GetCellCenterWorld(cell) + offset;
        highlightSpriteRenderer.color = isPlacementValid ? new Color(0, 1, 0, 0.25f) : new Color(1, 0, 0, 0.25f);        
    }
    private void PlaceBuilding(Vector3Int cell, bool isPlacementValid)
    {
        if(EventSystem.current.IsPointerOverGameObject()) return;
        if(CurrencyManager.Instance.GetCurrentMoney() < currentBuilding.cost) return;//Изменить, позже.

        if (Input.GetMouseButtonDown(0) && isPlacementValid)
        {
            if (!CurrencyManager.Instance.TrySpendMoney(currentBuilding.cost)) return;

            if (currentBuilding.tilemapType == TilemapType.Resources)
            {
                EventBusManager.Instance.ResourceBuilt(cell, currentBuilding);
            }
            else if (currentBuilding.tilemapType == TilemapType.Buildings)
            {
                EventBusManager.Instance.BuildingBuilt(cell, currentBuilding);
                RegisterBuilding(cell, currentBuilding);
                ProductionFactory factory = currentBuilding.BuildingObject.GetComponent<ProductionFactory>();
                    
            }
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
        bool IsDemolitionCell = FindDemolitionCell(cellPos, out TileData demolitionCell);
        UpdateDemolitionHighlight(cellPos, IsDemolitionCell);

        if (Input.GetMouseButtonDown(0) && IsDemolitionCell && !EventSystem.current.IsPointerOverGameObject())
        {
            if (CanDemolish(demolitionCell))
            {
                DemolishAt(cellPos);
            }
            else
            {
                Debug.Log("Объект нельзя уничтожить!");
            }
        }
    }

    private bool FindDemolitionCell(Vector3Int cellPos, out TileData demolitionCell)
    {
        demolitionCell = null;

        GameObject tile = GetTilemapFromTilemapManager(TilemapType.Resources).GetInstantiatedObject(cellPos);
        if (tile != null)
        {
            TileData targetTileData = tile.GetComponent<TileData>();
            if (targetTileData.isDestructible)
            {
                demolitionCell = targetTileData;
                return true;
            }
            return false;
        }

        tile = GetTilemapFromTilemapManager(TilemapType.Buildings).GetInstantiatedObject(cellPos);
        if (tile != null)
        {
            TileData targetTileData = tile.GetComponent<TileData>();
            if (targetTileData.isDestructible)
            {
                demolitionCell = targetTileData;
                return true;
            }
            return false;
        }

        return false;
    }
    private void UpdateDemolitionHighlight(Vector3Int cell, bool canDemolish)
    {
        currentHighlight.transform.position = GetTilemapFromTilemapManager(TilemapType.Ground).GetCellCenterWorld(cell);
        highlightSpriteRenderer.color = canDemolish ? new Color(1, 0, 0, 0.5f) : new Color(0, 0, 0, 0);
        currentHighlight.transform.localScale = Vector3.one;
    }
    private bool CanDemolish(TileData demolitionCell)
    {
        if (demolitionCell.TryGetComponent(out StorageBuilding building))
        {
            if (StorageManager.Instance.GetTotalCapacity() - building.GetCapacity() < StorageManager.Instance.GetCurrentVolume())
            {
                return false;
            }
        }
        return true;
    }
    private void DemolishAt(Vector3Int cell)
    {
        if (TryGetBuildingDataAt(cell, out BuildingData building, out Vector3Int startCell))
        {
            EventBusManager.Instance.BuildingDelete(startCell, building);

            BuildingRecord record = buildingRegistry[startCell];
            foreach (Vector3Int tilePos in record.occupiedCells)
            {
                buildingRegistry.Remove(tilePos);
            }
        }
        else
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
    private bool IsTerrainClear(Vector3Int cell)
    {
        if (GetTilemapFromTilemapManager(TilemapType.Resources).GetTile(cell))
        {
            return false;
        }
        else if (GetTilemapFromTilemapManager(TilemapType.Buildings).GetTile(cell))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void DeleteResourceTile(Vector3Int cell)
    {
        EventBusManager.Instance.ResourceDeleted(cell);
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

    private Tilemap GetTilemapFromTilemapManager(TilemapType tilemapType) => TilemapManager.Instance.GetTilemapOfType(tilemapType);
}
