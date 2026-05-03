using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProductionManager : MonoBehaviour //Данный менеджер должен быть всегда ниже в иерархии за TilemapManager.
{
    public static ProductionManager Instance;

    private bool _bankruptcy = false;

    private List<ProductionFactory> allFactories = new();
    private readonly List<ProductionFactory> activeFactories = new();
    private ProductionFactory targetFactory;

    public ProductionManagerData GetProductionManagerData()
    {
        var data = new ProductionManagerData();
        data.allFactories = new();
        foreach (var factory in allFactories)
        {
            if (factory != null)
            {
                data.allFactories.Add(factory.GetSaveData());
            }
        }
        return data;
    }

    private void Awake()
    {
        Initialize();
    }
    private void OnDestroy()
    {
        UninitializeEvents();
    }
    private void Initialize()
    {
        InitializeSingleton();
        InitializeEvents();
        IsLevelLoadFromSave();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameObject.name = "ProductionManager";
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnProductionFactoryBuilt += RegisterFactory;
        EventBusManager.Instance.OnProductionFactoryDeleted += UnregisterFactory;
        EventBusManager.Instance.OnBankruptcy += Bankruptcy;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnProductionFactoryBuilt -= RegisterFactory;
        EventBusManager.Instance.OnProductionFactoryDeleted -= UnregisterFactory;
        EventBusManager.Instance.OnBankruptcy -= Bankruptcy;
    }
    private void IsLevelLoadFromSave()
    {
        if (SaveManager.Instance.IsLoadLevelFromSave)
        {
           LoadProductionManagerData(SaveManager.Instance.LoadedLevelDates);
        }
    }
    private void LoadProductionManagerData(GameSessionData gameSessionData)
    {
        ProductionManagerData data = gameSessionData.productionManagerData;
        var buildings = TilemapManager.Instance.GetTilemapOfType(TilemapType.Buildings);
        var factories = buildings.transform.Cast<Transform>().Select(t => t.GetComponent<ProductionFactory>()).Where(f => f != null).ToList();
        
        allFactories = new(factories);
        Dictionary<Vector3Int, ProductionFactory> factoryByCell = new();
        foreach (var factory in allFactories)
        {
            if (factory != null)
            {
                factoryByCell[factory.GetCurrentCell()] = factory;
            }
        }
            

        foreach (var factoryData in data.allFactories)
        {
            Vector3Int cell = factoryData.originCell.ToVector3Int();
            if (factoryByCell.TryGetValue(cell, out ProductionFactory factory))
                factory.LoadFromSaveData(factoryData);
            else
                Debug.LogWarning($"Factory at cell {cell} not found during load");
        }

        activeFactories.Clear();
        foreach (var factory in allFactories)
        {
            if (factory != null && !factory.IsPaused)
            {
                activeFactories.Add(factory);
            }
        }
    }


    private void Bankruptcy()
    {
        _bankruptcy = true;
    }

    public void RegisterFactory(ProductionFactory factory)
    {
        if (factory == null) return;

        targetFactory = factory;

        if (!allFactories.Contains(factory)) RegisterNewFactory();

        UpdateFactoryActivation();
    }
    public void UnregisterFactory(ProductionFactory factory)
    {
        allFactories.Remove(factory);
        DeactivateFactory();
    }
    private void RegisterNewFactory()
    {
        allFactories.Add(targetFactory);
    }    
    private void UpdateFactoryActivation()
    {
        if (targetFactory.IsPaused) DeactivateFactory();
        else ActivateFactory();
    }
    private void ActivateFactory()
    {
        if (!activeFactories.Contains(targetFactory))
        {
            activeFactories.Add(targetFactory);
        }
    }
    private void DeactivateFactory()
    {
        activeFactories.Remove(targetFactory);
    }

    private void Update()
    {
        if (_bankruptcy) return;

        FactoriesProductionUpdate();
        FactoriesServiceUpdate();
    }
    private void FactoriesProductionUpdate()
    {
        float deltaTime = Time.deltaTime;
        foreach (var factory in activeFactories)
        {
            if (factory != null)
            {
                factory.UpdateProduction(deltaTime);
            }
        }
    }
    private void FactoriesServiceUpdate()
    {
        float deltaTime = Time.deltaTime;
        foreach (var factory in allFactories)
        {
            if (factory != null) factory.ServiceUpdate(deltaTime);
        }

    }
}
