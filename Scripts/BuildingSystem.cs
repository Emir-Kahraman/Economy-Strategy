using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem Instance;

    [Header("References")]
    public Tilemap groundTilemap;
    public Tilemap resourceTilemap;
    public Tilemap buildingTilemap;
    public TileBase waterTile;

    [Header("Building Ghost")]
    public GameObject highlightPrefab;

    private GameObject currentHighlight;
    private SpriteRenderer highlightSpriteRenderer;
    private BuildingData currentBuilding;

    private string groundTilemapTag = "Ground Tilemap";
    private string resourceTilemapTag = "Resources Tilemap";
    private string buildingTilemapTag = "Buildings Tilemap";

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
        gameObject.name = "BuildingSystem";

        Debug.Log($"Синглтон {gameObject.name} создан");
    }
    private void Initialize()
    {
        InitializeObjects();
        CreateObjects();

        Debug.Log($"Инициализация {gameObject.name} завершена");
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
                if (groundTilemap.GetTile(checkPos) == waterTile || buildingTilemap.GetTile(checkPos) != null) return false;
                GameObject resourceObject = resourceTilemap.GetInstantiatedObject(checkPos);
                if (resourceObject != null)
                {
                    TileData tileData = resourceObject.GetComponent<TileData>();
                    if (tileData == null) return false;

                    destroyableResource = tileData.isDestructibleResource && currentBuilding.canReplaceResources ? true : false;
                }
            }
        }
        return destroyableResource ? true : false;
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
        if (Input.GetMouseButtonDown(0) && isPlacementValid)
        {
            if (GameManager.Instance.money < currentBuilding.cost)
            {
                Debug.Log("Не хватает денег!"); 
                return;
            }
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

            Tilemap targetTilemap = TilemapManager.Instance.GetTilemap(currentBuilding.tilemapType);
            targetTilemap.SetTile(cell, currentBuilding.mainTile);
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
        }
    }

    private void Demolition()
    {
        Vector3Int cellPos = MouseToCellPositionOfGroundTilemap();
        bool canDemolish = CanDemolish(cellPos);
        UpdateDemolitionHighlight(cellPos, canDemolish);

        if (Input.GetMouseButtonDown(0) && canDemolish && !EventSystem.current.IsPointerOverGameObject())
            DemolishAt(cellPos);
    }
    private bool CanDemolish(Vector3Int cell)
    {        
        return buildingTilemap.GetTile(cell) != null ||
               (resourceTilemap.GetTile(cell) != null && IsDestructibleResource(cell));
    }
    private bool IsDestructibleResource(Vector3Int cell)
    {
        GameObject resourceObject = resourceTilemap.GetInstantiatedObject(cell);
        if (resourceObject == null) return false;

        TileData tileData = resourceObject.GetComponent<TileData>();
        return tileData != null && tileData.isDestructibleResource;
    }
    private void UpdateDemolitionHighlight(Vector3Int cell, bool canDemolish)
    {
        currentHighlight.transform.position = groundTilemap.GetCellCenterWorld(cell);
        highlightSpriteRenderer.color = canDemolish ? new Color(1, 0, 0, 0.5f) : new Color(0, 0, 0, 0);
        currentHighlight.transform.localScale = Vector3.one;
    }
    private void DemolishAt(Vector3Int cell)
    {
        // Удаляем здание
        if (buildingTilemap.GetTile(cell) != null)
        {
            buildingTilemap.SetTile(cell, null);
            Debug.Log("Здание снесено");
        }
        else if (resourceTilemap.GetTile(cell) != null)
        {
            resourceTilemap.SetTile(cell, null);
            Debug.Log("Ресурс уничтожен");
        }
    }

    private void DeleteResourceTile(Vector3Int cell)
    {
        resourceTilemap.SetTile(cell, null);
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
        GameModeManager.Instance.EnterToDemolitionMode();
    }
    public void CancelDemolition()
    {
        currentHighlight.SetActive(false);
        GameModeManager.Instance.EnterToObservationMode();
    }
}
