using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "ResourceData", menuName = "ResourceData")]
public class ResourceData : ScriptableObject
{
    [SerializeField] private const string category = "resources";
    public Period Period;
    public ResourceType Type;
    public bool CanBeOrdered;
    public TileBase Tile;
    public string Key;
    public Sprite Icon;
    public float VolumePerUnit;
    public int Price;

    public string GetLocalizedName()
    {
        return LocalizationManager.Instance.GetText(category, Key);
    }
}
