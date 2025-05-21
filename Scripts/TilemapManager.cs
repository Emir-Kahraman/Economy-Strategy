using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour //ќсталось, исправить баг с размером строимового завершение второго этапа. ƒобавить систему GameManager
{
    public static TilemapManager Instance;

    public Tilemap groundTilemap;
    public Tilemap resourceTilemap;
    public Tilemap obstacleTilemap;
    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public Tilemap GetTilemap(TilemapType type)
    {
        switch (type)
        {
            case TilemapType.Ground: return groundTilemap;
            case TilemapType.Resource: return resourceTilemap;
            case TilemapType.Obstacle: return obstacleTilemap;
            default: return null;
        }
    }
}
