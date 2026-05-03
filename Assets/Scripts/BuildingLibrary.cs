using System.Collections.Generic;
using UnityEngine;

public class BuildingLibrary : MonoBehaviour
{
    private static Dictionary<string, BuildingData> _buildings;
    private static bool _initialized;

    private static void Initialize()
    {
        if (_initialized) return;

        _buildings = new Dictionary<string, BuildingData>();
        BuildingData[] all = Resources.LoadAll<BuildingData>("Buildings");
        foreach (var buildingData in all)
        {
            string key = buildingData.BuildingKey;
            if (!_buildings.ContainsKey(key))
                _buildings.Add(key, buildingData);
            else
                Debug.LogError($"Duplicate building name detected: {key}. Skipping.");
        }
        _initialized = true;
    }
    public static BuildingData GetBuilding(string name)
    {
        Initialize();
        if (_buildings.TryGetValue(name, out var buildingData))
            return buildingData;
        Debug.LogError($"Building not found: {name}");
        return null;
    }
}
