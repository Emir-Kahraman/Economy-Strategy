using UnityEngine;
using UnityEngine.Tilemaps;

public enum TilemapType
{
    Ground,
    Resource,
    Obstacle
}

public enum BuildingType
{
    Type_1,
    Type_2,
    Type_3,
    Others
}

[CreateAssetMenu(fileName = "NewBuilding", menuName ="Building Data")]
public class BuildingData : ScriptableObject
{
    public TileBase mainTile;
    public TileBase[] secondaryTiles;
    public TilemapType tilemapType;
    public Vector2Int size = Vector2Int.one;
    public bool canReplaceResources = false;

    public Sprite icon;
    public int cost;
    public BuildingType buildingType;
}
