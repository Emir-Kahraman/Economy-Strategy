using System.Collections.Generic;
using UnityEngine;

public static class ResourceLibrary
{
    private static Dictionary<string, ResourceData> _resources;
    private static bool _initialized = false;

    private static void Initialize()
    {
        if (_initialized) return;
        _resources = new Dictionary<string, ResourceData>();
        
        ResourceData[] resourceDataArray = Resources.LoadAll<ResourceData>("");
        foreach (var resourceData in resourceDataArray)
        {
            string key = resourceData.Key;
            if (!_resources.ContainsKey(key))
            {
                _resources.Add(key, resourceData);
            }
            else
                Debug.LogError($"Duplicate resource name detected: {key}. Skipping.");
        }
        
        _initialized = true;
    }
    public static ResourceData GetResource(string name)
    {
        Initialize();
        if (_resources.TryGetValue(name, out var resourceData))
            return resourceData;
        Debug.LogError($"Resource not found: {name}");
        return null;
    }
    public static List<ResourceData> GetAllResources()
    {
        Initialize();
        return new List<ResourceData>(_resources.Values);
    }
}
