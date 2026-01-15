using UnityEngine;

public class TileData : MonoBehaviour
{
    [Header("Настройки")]
    public bool isBuildable = true;
    public bool isResource = false;
    public bool isDestructible  = false;
    public ResourceType resourceType = ResourceType.None;
}
