using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ProductionManager : MonoBehaviour
{
    public static ProductionManager Instance;

    private bool _bankruptcy = false;

    private readonly List<ProductionFactory> allFactories = new();//Пока нигде не используем
    private readonly List<ProductionFactory> activeFactories = new();
    private ProductionFactory targetFactory;

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
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "ProductionManager";
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnBankruptcy += Bankruptcy;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnBankruptcy -= Bankruptcy;
    }
    private void Bankruptcy()
    {
        _bankruptcy = true;
    }

    public void ManagerFactory(ProductionFactory factory)
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
