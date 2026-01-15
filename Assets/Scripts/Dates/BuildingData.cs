using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum BuildingCategory
{
    Food,
    Cloth,
    Arrangement,
    Household,
    Others
}

[CreateAssetMenu(fileName = "NewBuilding", menuName ="Building Data")]
public class BuildingData : ScriptableObject
{
    [Serializable]
    public class ResourceForBuild
    {
        public ResourceData Resource;
        public int Amount;
    }


    public GameObject BuildingObject;
    [Space]
    public Period Period;
    public TileBase mainTile;
    public TileBase[] secondaryTiles;
    [Space]
    public TilemapType tilemapType;
    public Vector2Int size = Vector2Int.one;
    [Space]
    public TileBase placementRequirement;
    public bool canReplaceResources = false;
    [Space]
    public Sprite icon;
    public int cost;
    [Space]
    public BuildingCategory buildingCategory;
    [Space]
    public List<ResourceForBuild> resourcesForBuild = new();
}
