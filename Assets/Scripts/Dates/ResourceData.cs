using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "ResourceData")]
public class ResourceData : ScriptableObject
{
    public ResourceType Type;
    public string Name;
    public Sprite Icon;
    public float VolumePerUnit = 1f;
    public int ProductionLevel;
    public int ProductionHard;
}
