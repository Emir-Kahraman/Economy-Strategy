using Mono.Cecil;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
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
    public Tilemap obstacleTilemap;
    public TileBase waterTile;

    [Header("Building Ghost")]
    public GameObject highlightPrefab;

    private GameObject currentHighlight;
    private SpriteRenderer highlightSpriteRenderer;
    private BuildingData currentBuilding;    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        CreateObjects();
    }
    void Update()
    {
        if ((GameModeManager.Instance.CurrentMode != GameModeManager.GameMode.Building)) return;
        if (currentBuilding == null) return;
        Building();
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
        bool isPlacementValid = IsPlacementValid(cellPos, out bool destroyableResource); // Можно было бы использовать struct.
        UpdateHighlight(cellPos, isPlacementValid);
        PlaceBuilding(cellPos, isPlacementValid, destroyableResource);
    }

    private Vector3Int MouseToCellPositionOfGroundTilemap()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return groundTilemap.WorldToCell(mouseWorldPos);
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

    bool IsPlacementValid(Vector3Int cell, out bool destroyableResource)
    {
        destroyableResource = false;
        for(int x = 0; x < currentBuilding.size.x; x++)
        {
            for (int y = 0; y < currentBuilding.size.y; y++)
            {
                Vector3Int checkPos = cell + new Vector3Int(x, y);
                if (groundTilemap.GetTile(checkPos) == waterTile || obstacleTilemap.GetTile(checkPos) != null) return false;

                GameObject resourceObject = resourceTilemap.GetInstantiatedObject(checkPos);
                if (resourceObject != null)
                {
                    TileData tileData = resourceObject.GetComponent<TileData>();
                    if (tileData == null) return false;

                    destroyableResource = tileData.isDestructibleResource && currentBuilding.canReplaceResources ? true : false;
                }                
            }
        }
        return true;
    }
    private void PlaceBuilding(Vector3Int cell, bool isPlacementValid, bool destroyableResource)
    {
        if(EventSystem.current.IsPointerOverGameObject()) return;
        if (Input.GetMouseButtonDown(0) && isPlacementValid)
        {
            if (destroyableResource)//
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
            for (int x = 0; x < currentBuilding.size.x; x++)
            {
                for (int y = 0; y < currentBuilding.size.y; y++)
                {
                    Vector3Int tellPos = cell + new Vector3Int(x, y);
                    targetTilemap.SetTile(tellPos, currentBuilding.tileBase);
                }
            }
            
        }        
    }    
    private void DeleteResourceTile(Vector3Int cell)
    {
        resourceTilemap.SetTile(cell, null);
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
}
