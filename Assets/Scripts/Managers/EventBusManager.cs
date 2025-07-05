using UnityEngine;
using System;
using System.Collections.Generic;

public class EventBusManager : MonoBehaviour
{
    public static EventBusManager Instance;
    public event Action<int> OnMoneyChanged;
    public event Action<ResourceType, int> OnResourceUpdated;
    public event Action<float> OnStorageCapacityUpdated;
    public event Action<float> OnStorageBuilt;
    public event Action<List<ResourceData>> OnResourceDataUpdated;
    public event Action<Vector3Int> OnResourceTilemapUpdated;
    public event Action<IUIWindow> OnWindowOpenRequested;
    public event Action<IUIWindow> OnWindowCloseRequested;
    public event Action<ProductionFactory> OnProductionFactoryRequested;
    public event Action OnSwitchToObservationGameMode;
    public event Action OnSwitchToBuildingGameMode;
    public event Action OnSwitchToDemolitionGameMode;
    public event Action OnSwitchToResourceAllocationMode;

    private void Awake()
    {
        InitializeSingleton();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "EventBusManager";
    }

    public void MoneyChanged(int value) => OnMoneyChanged?.Invoke(value);
    public void ResourceUpdated(ResourceType type, int value) => OnResourceUpdated?.Invoke(type, value);
    public void StorageCapacityUpdated(float value) => OnStorageCapacityUpdated?.Invoke(value);
    public void StorageBuilt(float value) => OnStorageBuilt?.Invoke(value);
    public void ResourceDataUpdated(List<ResourceData> resourceData) => OnResourceDataUpdated?.Invoke(resourceData);
    public void ResourceTilemapUpdated(Vector3Int cell) => OnResourceTilemapUpdated?.Invoke(cell);
    public void WindowOpenRequested(IUIWindow window) => OnWindowOpenRequested?.Invoke(window);
    public void WindowCloseRequested(IUIWindow window) => OnWindowCloseRequested?.Invoke(window);
    public void ProductionFactoryRequested(ProductionFactory factory) => OnProductionFactoryRequested?.Invoke(factory);
    public void SwitchToObservationGameMode() => OnSwitchToObservationGameMode?.Invoke();
    public void SwitchToBuildingGameMode() => OnSwitchToBuildingGameMode?.Invoke();
    public void SwitchToDemolitionGameMode() => OnSwitchToDemolitionGameMode?.Invoke();
    public void SwitchToResourceAllocationMode() => OnSwitchToResourceAllocationMode?.Invoke();
}
