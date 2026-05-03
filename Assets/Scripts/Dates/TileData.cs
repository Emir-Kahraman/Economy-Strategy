using UnityEngine;

public enum TerrainType
{
    None,
    Water,
    Ground,
    Sand,
    Mountain,
    River
}
public class TileData : MonoBehaviour
{
    [Header("Настройки")]
    public bool isBuildable = true;
    public bool isResource = false;
    public bool isDestructible  = false;
    public ResourceType resourceType = ResourceType.None;
    public TerrainType terrainType = TerrainType.None;
}
