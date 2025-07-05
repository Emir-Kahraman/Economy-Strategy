using UnityEngine;

public class TileData : MonoBehaviour
{
    [Header("���������")]
    public bool isBuildable = true;
    public bool isResource = false;
    public bool isDestructibleResource  = false;
    public ResourceType resourceType = ResourceType.None;
}
