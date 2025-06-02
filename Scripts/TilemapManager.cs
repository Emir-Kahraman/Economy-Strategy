using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour //Осталось, система сноса, деньги.
{
    public static TilemapManager Instance { get; private set; }

    public Tilemap groundTilemap;
    public Tilemap resourceTilemap;
    public Tilemap buildingTilemap;
    
    private string groundTilemapTag = "Ground Tilemap";
    private string resourceTilemapTag = "Resources Tilemap";
    private string buildingTilemapTag = "Buildings Tilemap";

    private void Awake()
    {
        InitializeSingleton();
        Initialize();
    }
    public void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "TilemapManager";

        Debug.Log($"Синглтон {gameObject.name} создан");
    }
    public void Initialize()
    {
        groundTilemap = GameObject.FindWithTag(groundTilemapTag).GetComponent<Tilemap>();
        resourceTilemap = GameObject.FindWithTag(resourceTilemapTag).GetComponent<Tilemap>();
        buildingTilemap = GameObject.FindWithTag(buildingTilemapTag).GetComponent<Tilemap>();

        groundTilemap.CompressBounds();
        resourceTilemap.RefreshAllTiles();

        Debug.Log($"Инициализация данных {gameObject.name} завершена");
    }
    public Tilemap GetTilemap(TilemapType type)
    {
        switch (type)
        {
            case TilemapType.Ground: return groundTilemap;
            case TilemapType.Resource: return resourceTilemap;
            case TilemapType.Obstacle: return buildingTilemap;
            default: return null;
        }
    }
}
