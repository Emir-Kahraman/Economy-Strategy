using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "ResourceData", menuName = "ResourceData")]
public class ResourceData : ScriptableObject
{
    public Period Period;
    public ResourceType Type;
    public bool CanBeOrdered;
    public TileBase Tile;
    public string Name;
    public Sprite Icon;
    public float VolumePerUnit;
    public int Price;
}
