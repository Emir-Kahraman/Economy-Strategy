using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ProductionManager : MonoBehaviour
{
    public static ProductionManager Instance;

    private readonly List<ProductionFactory> allFactories = new();//Пока нигде не используем
    private readonly List<ProductionFactory> activeFactories = new();
    private ProductionFactory targetFactory;

    private void Awake()
    {
        InitializeSingleton();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "ProductionManager";
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
            SortActiveFactoriesByPriority();
        }
    }
    private void DeactivateFactory()
    {
        if (activeFactories.Remove(targetFactory)) SortActiveFactoriesByPriority();
    }
    private void SortActiveFactoriesByPriority()
    {
        activeFactories.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    private void Update()
    {        
        FactoriesProductionUpdate();
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

    }
}
