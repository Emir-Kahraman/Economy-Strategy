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
    public event Action<Vector3Int> OnResourceTilemapUpdated;
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
    public event Action<int> OnWorkerLimitUpdate;
    public event Action<int> OnWorkerCountUpdate;
    public event Action<int> OnDissolutionWorkers;
    public event Action<int> OnUnemploymentWorkerCountUpdate;
    public event Action<int> OnWorkerHousingBuilt;
    public event Action<float> OnSatisfactionLevelUpdate;
    public event Action<float> OnSatisfactionModifierUpdate;

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
    public void ResourceTilemapUpdated(Vector3Int cell) => OnResourceTilemapUpdated?.Invoke(cell);
    public void WindowOpenRequested(IUIWindow window) => OnWindowOpenRequested?.Invoke(window);
    public void WindowCloseRequested(IUIWindow window) => OnWindowCloseRequested?.Invoke(window);
    public void ProductionFactoryRequested(ProductionFactory factory) => OnProductionFactoryRequested?.Invoke(factory);
    public void SwitchToObservationGameMode() => OnSwitchToObservationGameMode?.Invoke();
    public void SwitchToBuildingGameMode() => OnSwitchToBuildingGameMode?.Invoke();
    public void SwitchToDemolitionGameMode() => OnSwitchToDemolitionGameMode?.Invoke();
    public void SwitchToResourceAllocationMode() => OnSwitchToResourceAllocationMode?.Invoke();//
    public void OrderCreated(OrderData orderData) => OnOrderCreated?.Invoke(orderData);
    public void OrderAccepted() => OnOrderAccepted?.Invoke();
    public void OrderExpired(bool isAcceptedOrder) => OnOrderExpired?.Invoke(isAcceptedOrder);
    public void WorkerLimitUpdate(int limit) => OnWorkerLimitUpdate?.Invoke(limit);
    public void WorkerCountUpdate(int count) => OnWorkerCountUpdate?.Invoke(count);
    public void DissolutionWorkers(int count) => OnDissolutionWorkers?.Invoke(count);
    public void UnemploymentWorkerCountUpdate(int count) => OnUnemploymentWorkerCountUpdate?.Invoke(count);
    public void WorkerHousingBuilt(int value) => OnWorkerHousingBuilt?.Invoke(value);
    public void SatisfactionLevelUpdate(float value) => OnSatisfactionLevelUpdate?.Invoke(value);
    public void SatisfactionModifierUpdate(float modifier) => OnSatisfactionModifierUpdate?.Invoke(modifier);
}
