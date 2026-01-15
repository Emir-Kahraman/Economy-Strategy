using UnityEngine;
using System;
using System.Collections.Generic;

public class EventBusManager : MonoBehaviour
{
    public static EventBusManager Instance;
    public event Action OnBankruptcy;
    public event Action<float> OnBankruptcyProcess;
    public event Action<string> OnGameOver;
    public event Action<int> OnMoneyChanged;
    public event Action<ResourceType, int> OnResourceUpdated;
    public event Action<float> OnStorageCapacityUpdated;
    public event Action<float> OnStorageBuilt;
    public event Action<List<ResourceData>> OnResourceDataUpdated;
    public event Action<Vector3Int, BuildingData> OnResourceBuilt;
    public event Action<Vector3Int> OnResourceDeleted;
    public event Action<IUIWindow> OnWindowOpenRequested;
    public event Action<IUIWindow> OnWindowCloseRequested;
    public event Action<ProductionFactory> OnProductionFactoryRequested;
    public event Action OnSwitchToObservationGameMode;
    public event Action OnSwitchToBuildingGameMode;
    public event Action OnSwitchToDemolitionGameMode;
    public event Action OnSwitchToResourceAllocationMode;
    public event Action<OrderData> OnOrderCreated;
    public event Action OnOrderAccepted;
    public event Action<bool> OnOrderExpired;
    public event Action<float> OnSatisfactionLevelUpdate;
    public event Action<float> OnSatisfactionModifierUpdate;
    public event Action<List<BuildingData>> OnBuildingDataUpdated;
    public event Action<string> OnSceneLoadRequest;
    public event Action OnSceneLoaded;
    public event Action<MenuType> OnMenuSwitch;
    public event Action<Vector3Int, BuildingData> OnBuildingBuilt;
    public event Action<Vector3Int, BuildingData> OnBuildingDeleted;
    public event Action<Period> OnCurrentPeriodUpdated;
    public event Action<ResourceData, int> OnOrderCompleted;

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

    public void Bankruptcy() => OnBankruptcy?.Invoke();
    public void BankruptcyProcess(float value) => OnBankruptcyProcess?.Invoke(value);
    public void GameOver(string cause) => OnGameOver?.Invoke(cause);
    public void MoneyChanged(int value) => OnMoneyChanged?.Invoke(value);
    public void ResourceUpdated(ResourceType type, int value) => OnResourceUpdated?.Invoke(type, value);
    public void StorageCapacityUpdated(float value) => OnStorageCapacityUpdated?.Invoke(value);
    public void StorageBuilt(float value) => OnStorageBuilt?.Invoke(value);
    public void ResourceDataUpdated(List<ResourceData> resourceData) => OnResourceDataUpdated?.Invoke(resourceData);
    public void ResourceBuilt(Vector3Int cell, BuildingData data) => OnResourceBuilt?.Invoke(cell, data);
    public void ResourceDeleted(Vector3Int cell) => OnResourceDeleted?.Invoke(cell);
    public void WindowOpenRequested(IUIWindow window) => OnWindowOpenRequested?.Invoke(window);
    public void WindowCloseRequested(IUIWindow window) => OnWindowCloseRequested?.Invoke(window);
    public void ProductionFactoryRequested(ProductionFactory factory) => OnProductionFactoryRequested?.Invoke(factory);
    public void SwitchToObservationGameMode() => OnSwitchToObservationGameMode?.Invoke();
    public void SwitchToBuildingGameMode() => OnSwitchToBuildingGameMode?.Invoke();
    public void SwitchToDemolitionGameMode() => OnSwitchToDemolitionGameMode?.Invoke();
    public void SwitchToResourceAllocationMode() => OnSwitchToResourceAllocationMode?.Invoke();
    public void OrderCreated(OrderData orderData) => OnOrderCreated?.Invoke(orderData);
    public void OrderAccepted() => OnOrderAccepted?.Invoke();
    public void OrderExpired(bool isAcceptedOrder) => OnOrderExpired?.Invoke(isAcceptedOrder);
    public void SatisfactionLevelUpdate(float value) => OnSatisfactionLevelUpdate?.Invoke(value);
    public void SatisfactionModifierUpdate(float modifier) => OnSatisfactionModifierUpdate?.Invoke(modifier);
    public void BuildingDataUpdated(List<BuildingData> buildings) => OnBuildingDataUpdated?.Invoke(buildings);
    public void SceneLoadRequest(string sceneName) => OnSceneLoadRequest?.Invoke(sceneName);
    public void SceneLoaded() => OnSceneLoaded?.Invoke();
    public void MenuSwitch(MenuType activatedMenu) => OnMenuSwitch?.Invoke(activatedMenu);
    public void BuildingBuilt(Vector3Int startCell, BuildingData data) => OnBuildingBuilt?.Invoke(startCell, data);
    public void BuildingDelete(Vector3Int startCell, BuildingData data) => OnBuildingDeleted?.Invoke(startCell, data);
    public void CurrentPeriodUpdated(Period period) => OnCurrentPeriodUpdated?.Invoke(period);
    public void OrderCompleted(ResourceData resource, int reward) => OnOrderCompleted?.Invoke(resource, reward);
}
