using System;
using System.Collections.Generic;
using UnityEngine;

public struct WorldGenerateParameters
{
    public int worldSizeIndex;
    public int startMoneyIndex;
    public int startPeriodIndex;
}

public class WorldGenerator : MonoBehaviour
{
    [Serializable]
    public class WorldSettings
    {
        [Header("Settings Name")]
        public string settingsName; //Название набора настроек для удобства, не влияет на генерацию.
        [Header("World Size Settings")]
        [Range(40, 100)] public int width = 40;
        [Range(40, 100)] public int height = 40;
        [Space, Header("Biome Settings")]
        public float noiseScale = 0.1f; //Чем меньше значение, тем более крупные и плавные биомы, чем больше - тем более мелкие и разнообразные. Обычно 0.05-0.2 для карт такого размера. Для карт 40х40 оптимально 0.05, для 100х100 - 0.1.
        public float waterLevel = 0.4f; //Порог для воды. Чем выше значение, тем больше будет воды на карте. Обычно 0.3-0.5 для карт такого размера. Для карт 40х40 оптимально 0.3, для 100х100 - 0.5.
        public float sandThreshold = 0.1f; //Толщина пляжа. Чем выше значение, тем шире будет пляж. Обычно 0.05-0.15 для карт такого размера. Для карт 40х40 оптимально 0.05, для 100х100 - 0.07.
        public float mountainThreshold = 0.85f; //Порог для гор. Чем выше значение, тем меньше будет гор на карте. Обычно 0.8-0.9 для карт такого размера. Для карт 40х40 оптимально 0.9, для 100х100 - 0.8.
        [Space, Header("Rivers Settings")]
        [Range(0, 10)] public int riverCount = 3; //Количество рек. Для карт 40х40 оптимально 2-4, для 100х100 - 6-8.
        public int minRiverLength = 4; //Минимальная длина сегмента реки. Для карт 40х40 оптимально 4-6, для 100х100 - 5-8.
        public int maxRiverLength = 10; //Максимальная длина сегмента реки. Для карт 40х40 оптимально 8-12, для 100х100 - 10-15.
        public float riverChance = 0.5f; //Вероятность создания реки в подходящей точке. Для карт 40х40 оптимально 0.6, для 100х100 - 0.3-0.5.
    }
    [Serializable]
    public class WorldResourceSettings
    {
        [Header("Settings Name")]
        public string settingsName; //Название набора настроек для удобства, не влияет на генерацию.
        [Header("Forest Resource Settings")]
        public float forestZoneThreshold = 0.6f; //Порог для лесной зоны. Чем выше значение, тем меньше будет лесов. Обычно 0.5-0.7 для карт такого размера. Для карт 40х40 оптимально 0.55, для 100х100 - 0.7.
        public float forestNoiseScale = 0.05f; //Масштаб шума для лесов. Чем меньше значение, тем более крупные и плотные леса, чем больше - тем более мелкие и разбросанные. Обычно 0.03-0.07 для карт такого размера. Для карт 40х40 оптимально 0.03, для 100х100 - 0.07.
        public float treeDensity = 0.7f; //Плотность деревьев в лесной зоне. Чем выше значение, тем больше деревьев будет в лесу. Обычно 0.5-0.8 для карт такого размера. Для карт 40х40 оптимально 0.7, для 100х100 - 0.6.
        [Range(0f, 1f)] public float oakProbability = 0.03f; //Вероятность появления дубов среди деревьев. Обычно 0.02-0.05 для карт такого размера. Для карт 40х40 оптимально 0.04, для 100х100 - 0.02.
        [Range(0f, 1f)] public float ebonyProbability = 0.01f; //Вероятность появления черного дерева среди деревьев. Обычно 0.005-0.02 для карт такого размера. Для карт 40х40 оптимально 0.02, для 100х100 - 0.005.

        [Space, Header("Mountain Resource Settings")]
        public List<ResourceSpawnSettings> mountainResources = new(); //Список добываемых ресурсов с их вероятностями и минимальным количеством на карте. Ресурсы с более высокой вероятностью будут появляться чаще.

        [Space, Header("Coastal Resource Settings")]
        public List<ResourceSpawnSettings> coastalResources = new(); //Список прибрежных ресурсов с их вероятностями и минимальным количеством на карте. Ресурсы с более высокой вероятностью будут появляться чаще.
    }
    [Serializable]
    public class ResourceSpawnSettings
    {
        public ResourceType resourceType; //Тип ресурса, который будет генерироваться.
        [Range(0f, 1f)] public float spawnProbability; //Вероятность появления данного ресурса при генерации. Ресурсы с более высокой вероятностью будут появляться чаще. Оптимальные значение для карт такого размера 0.01-0.1, в зависимости от желаемой редкости ресурса.
        [Range(0, 30)] public int minSpawnValue; //Минимальное количество данного ресурса на карте. Если после генерации их меньше, то будут созданы дополнительные ресурсы данного типа в случайных местах, подходящих для этого ресурса, до достижения этого количества.
    }
    [Header("World Settings")]
    [SerializeField] private List<WorldSettings> worldSettingsPrefabs; //Список настроек для разных размеров карты, индекс которых задается в WorldGenerateParameters.
    [SerializeField] private List<WorldResourceSettings> resourceSettingsPrefabs; //Список настроек для ресурсов, индекс которых задается в WorldGenerateParameters. Имеет один и тот же индекс, что и worldSettingsPrefabs, так как ресурсы должны соответствовать размеру карты.
    [Space(), Header("Seed")]
    [SerializeField] private int seed = 0; //0 - случайный сид, любое другое значение - фиксированный сид для воспроизводимости карты.

    private WorldSettings settings = new();
    private WorldResourceSettings resourceSettings;
    private Dictionary<ResourceType, int> mountainResourcesCount = new();
    private Dictionary<ResourceType, int> coastalResourcesCount = new();

    private readonly Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0,1),   // Up
        new Vector2Int(1,0),   // Right
        new Vector2Int(0,-1),  // Down
        new Vector2Int(-1,0)   // Left
    };

    public void Initialize(CurrentLevelRuntimeData runtimeData)
    {
        WorldGenerateParameters generateParameters = runtimeData.worldGenerateParameters;
        int worldSeed = runtimeData.worldSeed;
        
        if (runtimeData != null && worldSeed != 0)
        {
            seed = worldSeed;
        }
        else
        {
            if (seed == 0)
            {
                seed = UnityEngine.Random.Range(1, int.MaxValue);
            }
            if (runtimeData != null)
            {
                runtimeData.worldSeed = seed;
            }
        }
        if (seed != 0)
        {
            UnityEngine.Random.InitState(seed);
        }

        InitializeWorldGenerateParameters(generateParameters);
        GenerateWorld();
        GenerateRivers();
        InitializeParameters();
        GenerateForestResources();
        GenerateMountainResources();
        GenerateCoastalResources();
    }
    private void InitializeWorldGenerateParameters(WorldGenerateParameters generateParameters)
    {
        WorldGenerateParameters(generateParameters.worldSizeIndex);
    }
    private void WorldGenerateParameters(int mapeSizeIndex)
    {
        switch (mapeSizeIndex)
        {
            case 0:
                settings = worldSettingsPrefabs[0];
                resourceSettings = resourceSettingsPrefabs[0];
                break;
            case 1:
                settings = worldSettingsPrefabs[1];
                    resourceSettings = resourceSettingsPrefabs[1];
                break;
            case 2:
                settings = worldSettingsPrefabs[2];
                resourceSettings = resourceSettingsPrefabs[2];
                break;
            default:
                Debug.LogWarning("Invalid map size index! Using default settings.");
                settings = worldSettingsPrefabs[0];
                resourceSettings = resourceSettingsPrefabs[0];
                break;
        }
    }
    private void GenerateWorld()
    {
        if (seed != 0)
        {
            UnityEngine.Random.InitState(seed);
        }

        float offsetX = UnityEngine.Random.Range(0f, 9999f);
        float offsetY = UnityEngine.Random.Range(0f, 9999f);
        
        for (int x = 0; x < settings.width; x++)
        {
            for (int y = 0; y < settings.height; y++)
            {
                float noiseValue = Mathf.PerlinNoise(
                    (x + offsetX) * settings.noiseScale,
                    (y + offsetY) * settings.noiseScale
                );

                if (noiseValue < settings.waterLevel)
                {
                    EventBusManager.Instance.CreateGroundTile(new Vector3Int(x,y), TerrainType.Water);
                }
                else if (noiseValue < settings.waterLevel + settings.sandThreshold)
                {
                    EventBusManager.Instance.CreateGroundTile(new Vector3Int(x, y), TerrainType.Sand);
                }
                else if (noiseValue > settings.mountainThreshold)
                {
                    EventBusManager.Instance.CreateGroundTile(new Vector3Int(x, y), TerrainType.Mountain);
                }
                else
                {
                    EventBusManager.Instance.CreateGroundTile(new Vector3Int(x, y), TerrainType.Ground);
                }
            }
        }
        
        Debug.Log($"Карта сгенерирована! Размер: {settings.width}x{settings.height}");
    }
    private void GenerateRivers()
    {
        int riversCreated = 0;
        int attempts = 0;
        const int maxAttempts = 50;

        List<Vector2Int> possibleStarts = FindAllPossibleRiverStarts();
        
        ShuffleList(possibleStarts);

        foreach (var start in possibleStarts)
        {
            if (riversCreated >= settings.riverCount || attempts >= maxAttempts) break;

            if (UnityEngine.Random.value > settings.riverChance) continue;

            if (TryCreateRiver(start))
            {
                riversCreated++;
            }
            attempts++;
        }
    }
    private List<Vector2Int> FindAllPossibleRiverStarts()
    {
        List<Vector2Int> starts = new();

        for (int x = 0; x < settings.width; x++)
        {
            for (int y = 0; y < settings.height; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y);
                if (TilemapManager.Instance.GetTerrainType(cellPos) == TerrainType.Ground || TilemapManager.Instance.GetTerrainType(cellPos) == TerrainType.Sand)
                {
                    foreach (var dir in directions)
                    {
                        Vector3Int neighborPos = cellPos + new Vector3Int(dir.x, dir.y);
                        if (TilemapManager.Instance.GetTerrainType(neighborPos) == TerrainType.Water)
                        {
                            starts.Add(new Vector2Int(x, y));
                            break;
                        }
                    }
                }
            }
        }
        return starts;
    }
    private void ShuffleList(List<Vector2Int> possibleStarts)//Перемешивание списка
    {
        for (int i = 0; i < possibleStarts.Count; i++)
        {
            Vector2Int temp = possibleStarts[i];
            int randomIndex = UnityEngine.Random.Range(i, possibleStarts.Count);
            possibleStarts[i] = possibleStarts[randomIndex];
            possibleStarts[randomIndex] = temp;
        }
    }
    private bool TryCreateRiver(Vector2Int start)
    {
        List<Vector2Int> availableDirections = new();
        foreach (var dir in directions)
        {
            Vector2Int nextPos = start + dir;
            if (CanPlaceRiverSegment(nextPos))
            {
                availableDirections.Add(dir);
            }
        }

        if (availableDirections.Count == 0) return false;

        Vector2Int firstDirection = availableDirections[UnityEngine.Random.Range(0, availableDirections.Count)];
        int firstLength = UnityEngine.Random.Range(settings.minRiverLength, settings.maxRiverLength + 1);
        List<Vector2Int> firstSegment = CreateRiverSegment(start, firstDirection, firstLength);
        if (firstSegment.Count < 2) return false;

        List<Vector2Int> perpendicularDirections = GetPerpendicularDirections(firstDirection);
        List<Vector2Int> availableSecondDirections = new();
        foreach (var dir in perpendicularDirections)
        {
            Vector2Int lastPos = firstSegment[firstSegment.Count - 1];
            Vector2Int nextPos = lastPos + dir;

            if (CanPlaceRiverSegment(nextPos))
            {
                availableSecondDirections.Add(dir);
            }   
        }
        if (availableSecondDirections.Count == 0) return false;
        Vector2Int secondDirection = availableSecondDirections[UnityEngine.Random.Range(0, availableSecondDirections.Count)];
        int secondLength = UnityEngine.Random.Range(settings.minRiverLength, settings.maxRiverLength + 1);
        Vector2Int secondStart = firstSegment[firstSegment.Count - 1];
        List<Vector2Int> secondSegment = CreateRiverSegment(secondStart, secondDirection, secondLength);

        if (secondSegment.Count < 3) return false;

        List<Vector2Int> fullRiver = new();
        fullRiver.AddRange(firstSegment);
        fullRiver.AddRange(secondSegment);

        Vector2Int riverMouthDirection = GetWaterDirection(start); // Направление к воде от начала реки

        foreach (var pos in fullRiver)
        {
            if (pos == fullRiver[0])
            {
                EventBusManager.Instance.CreateRiverMouthTile(new Vector3Int(pos.x, pos.y), riverMouthDirection);
            }
            else
            {
                EventBusManager.Instance.CreateGroundTile(new Vector3Int(pos.x, pos.y), TerrainType.River);
            }
        }
        return true;
    }
    private Vector2Int GetWaterDirection(Vector2Int position)
    {
        foreach (var dir in directions)
        {
            Vector2Int neighborPos = position + dir;
            Vector3Int tilePos = new Vector3Int(neighborPos.x, neighborPos.y);
            if (TilemapManager.Instance.GetTerrainType(tilePos) == TerrainType.Water)
            {
                return dir;
            }
        }
        return Vector2Int.zero;
    }
    private List<Vector2Int> CreateRiverSegment(Vector2Int start, Vector2Int direction, int length)
    {
        List<Vector2Int> segment = new();

        Vector2Int currentPos = start;

        segment.Add(currentPos);

        for (int i = 0; i < length; i++)
        {
            currentPos += direction;
            
            if (!CanPlaceRiverSegment(currentPos))
            {
                break;
            }
            
            segment.Add(currentPos);
        }

        return segment;

    }
    private bool CanPlaceRiverSegment(Vector2Int segmentPos)
    {
        Vector3Int position = new Vector3Int(segmentPos.x, segmentPos.y);
        if (position.x < 0 || position.x >= settings.width || position.y < 0 || position.y >= settings.height) return false; // Проверка границ карты

        List<TerrainType> invalidTerrains = new() { TerrainType.River, TerrainType.Water, TerrainType.None }; // Невозможные типы местности для реки
        for (int i = 0; i< invalidTerrains.Count; i++)
        {
            if (TilemapManager.Instance.GetTerrainType(position) == invalidTerrains[i]) return false;
        }

        foreach (var dir in directions)
        {
            Vector3Int neighborPos = position + new Vector3Int(dir.x, dir.y);
            for (int i = 0; i< invalidTerrains.Count; i++)
            {
                if (TilemapManager.Instance.GetTerrainType(neighborPos) == invalidTerrains[i]) return false; // Проверка соседних клеток 
            }
        }
        return true;
    }
    private List<Vector2Int> GetPerpendicularDirections(Vector2Int direction)
    {
        List<Vector2Int> perpendicular = new();
        if (direction.x == 0)
        {
            perpendicular.Add(Vector2Int.right);
            perpendicular.Add(Vector2Int.left);
        }
        else
        {
            perpendicular.Add(Vector2Int.up);
            perpendicular.Add(Vector2Int.down);
        }
        return perpendicular;
    }

    private void InitializeParameters()
    {
        foreach (var resource in resourceSettings.mountainResources)
        {
            mountainResourcesCount[resource.resourceType] = 0;
        }
        foreach (var resource in resourceSettings.coastalResources)
        {
            coastalResourcesCount[resource.resourceType] = 0;
        }
    }

    private void GenerateForestResources()
    {
        float offsetX = UnityEngine.Random.Range(0f, 9999f);
        float offsetY = UnityEngine.Random.Range(0f, 9999f);

        for (int x = 0; x < settings.width; x++)
        {
            for (int y = 0; y < settings.height; y++)
            {
                if (TilemapManager.Instance.GetTerrainType(new Vector3Int(x, y)) != TerrainType.Ground) continue;

                float forestNoise = Mathf.PerlinNoise(
                   (x + offsetX) * resourceSettings.forestNoiseScale,
                   (y + offsetY) * resourceSettings.forestNoiseScale
               );

                if (forestNoise > resourceSettings.forestZoneThreshold)
                {
                    if (UnityEngine.Random.value < resourceSettings.treeDensity)
                    {
                        float random = UnityEngine.Random.value;
                        if (random < resourceSettings.ebonyProbability)
                        {
                            EventBusManager.Instance.CreateResourceTile(new Vector3Int(x, y), ResourceType.Ebony_Tree);
                        }
                        else if (random < resourceSettings.ebonyProbability + resourceSettings.oakProbability)
                        {
                            EventBusManager.Instance.CreateResourceTile(new Vector3Int(x, y), ResourceType.Oak_Tree);
                        }
                        else
                        {
                            EventBusManager.Instance.CreateResourceTile(new Vector3Int(x, y), ResourceType.Forest);
                        }
                    }
                }
            }
        }
    }

    private void GenerateMountainResources()
    {
        List<Vector3Int> availableCells = GetAvailableCellsForMountainResources();

        for (int i = 0; i < availableCells.Count; i++)
        {
            ResourceType resource = GetRandomMountainResource();
            int randomCell = UnityEngine.Random.Range(0, availableCells.Count);
            if (resource != ResourceType.None)
            {
                GenerateMountainResource(resource, availableCells[randomCell]);
            }
        }

        foreach (var resource in resourceSettings.mountainResources)
        {
            if (mountainResourcesCount[resource.resourceType] < resource.minSpawnValue)
            {
                int difference = resource.minSpawnValue - mountainResourcesCount[resource.resourceType];
                GenerateTargetMountainResource(resource.resourceType, difference);
            }
        }
    }
    private List<Vector3Int> GetAvailableCellsForMountainResources()
    {
        List<Vector3Int> cells = new();
        for (int x = 0; x < settings.width; x++)
        {
            for (int y = 0; y < settings.height; y++)
            {
                TerrainType terrain = TilemapManager.Instance.GetTerrainType(new Vector3Int(x, y));
                if (terrain == TerrainType.Ground)
                {
                    if (TilemapManager.Instance.GetResourceType(new Vector3Int(x, y)) == ResourceType.None || TilemapManager.Instance.GetResourceType(new Vector3Int(x, y)) == ResourceType.Forest)
                    {
                        cells.Add(new Vector3Int(x, y));
                    }
                }
            }
        }
        return cells;
    }
    
    private ResourceType GetRandomMountainResource()
    {
        float random = UnityEngine.Random.value;
        float cumulative = 0f;
        List<ResourceSpawnSettings> resources = resourceSettings.mountainResources;
        resources.Sort((a, b) => a.spawnProbability.CompareTo(b.spawnProbability));//список по вероятности
        for (int i = 0; i < resources.Count; i++)
        {
            cumulative += resources[i].spawnProbability;
            if (random < cumulative) return resources[i].resourceType;
        }

        return ResourceType.None;
    }

    private void GenerateMountainResource(ResourceType resource, Vector3Int cell)
    {
        EventBusManager.Instance.CreateResourceTile(cell, resource);
        mountainResourcesCount[resource] += 1;
    }

    private void GenerateTargetMountainResource(ResourceType resource, int count)
    {
        List<Vector3Int> availableCells = GetAvailableCellsForMountainResources();
        if (availableCells.Count == 0) return;
        for (int i = 0; i < count - 1; i++)
        {
            int randomCell = UnityEngine.Random.Range(0, availableCells.Count);
            GenerateMountainResource(resource, availableCells[randomCell]);
        }
    }

    private void GenerateCoastalResources()
    {
        List<Vector3Int> availableCells = GetAvailableCellsForCoastalResources();

        for (int i = 0; i < availableCells.Count; i++)
        {
            ResourceType resource = GetRandomCoastalResource();
            int randomCell = UnityEngine.Random.Range(0, availableCells.Count);
            if (resource != ResourceType.None)
            {
               GenerateCoastalResource(resource, availableCells[randomCell]);
            }
        }

        foreach (var resource in resourceSettings.coastalResources)
        {
            if (coastalResourcesCount[resource.resourceType] < resource.minSpawnValue)
            {
                int difference = resource.minSpawnValue - coastalResourcesCount[resource.resourceType];
                GenerateTargetCoastalResource(resource.resourceType, difference);
            }
        }
    }

    private List<Vector3Int> GetAvailableCellsForCoastalResources()
    {
        List<Vector3Int> cells = new();
        for (int x = 0; x < settings.width; x++)
        {
            for (int y = 0; y < settings.height; y++)
            {
                TerrainType terrain = TilemapManager.Instance.GetTerrainType(new Vector3Int(x, y));
                if (terrain == TerrainType.Water)
                {
                    if (TilemapManager.Instance.GetResourceType(new Vector3Int(x, y)) == ResourceType.None)
                    {
                        if (TilemapManager.Instance.IsCoastalCell(new Vector3Int(x ,y)))//Проверка, прибрежная ли клетка
                        { 
                            cells.Add(new Vector3Int(x, y));
                        }
                    }
                }
            }
        }
        return cells;
    }
    private ResourceType GetRandomCoastalResource()
    {
        float random = UnityEngine.Random.value;
        float cumulative = 0f;

        List<ResourceSpawnSettings> resources = resourceSettings.coastalResources;
        resources.Sort((a, b) => a.spawnProbability.CompareTo(b.spawnProbability));//список по вероятности

        for (int i = 0; i < resources.Count; i++)
        {
            cumulative += resources[i].spawnProbability;
            if (random < cumulative) return resources[i].resourceType;
        }

        return ResourceType.None;
    }
    private void GenerateCoastalResource(ResourceType resource, Vector3Int cell)
    {
        EventBusManager.Instance.CreateResourceTile(cell, resource);

    }
    private void GenerateTargetCoastalResource(ResourceType resource, int count)
    {
        List<Vector3Int> availableCells = GetAvailableCellsForCoastalResources();
        if (availableCells.Count == 0) return;
        for (int i = 0; i < count - 1; i++)
        {
            int randomCell = UnityEngine.Random.Range(0, availableCells.Count);
            GenerateCoastalResource(resource, availableCells[randomCell]);
        }
    }
}
