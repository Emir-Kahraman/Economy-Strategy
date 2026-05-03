using UnityEngine;
using System;
using System.Collections.Generic;

public class EventBusManager : MonoBehaviour
{
    public static EventBusManager Instance;
    public event Action OnBankruptcy;
    public event Action<float> OnBankruptcyProcess;
    public event Action<int> OnMoneyChanged;
    public event Action<ResourceType, int> OnResourceAmountUpdated;
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
    public event Action<Vector3Int, TerrainType> OnCreateGroundTile;
    public event Action<Vector3Int, ResourceType> OnCreateResourceTile;
    public event Action<Vector3Int, Vector2Int> OnCreateRiverMouthTile;
    public event Action<int> OnAllQuestsCompleted;
    public event Action<int> OnLevelCompleted;
    public event Action<ProgressData> OnGameProgressChanged;
    public event Action OnResetGameProgress;
    public event Action OnRequestLevelProgressSave;
    public event Action<List<OrderData>, bool> OnOrdersLoadedFromSave;
    public event Action<string> OnOrderAccept;
    public event Action<ProductionFactory> OnProductionFactoryBuilt;
    public event Action<ProductionFactory> OnProductionFactoryDeleted;
    public event Action<float, float> OnPeriodTimerUpdated;
    public event Action<BuildingData> OnBuildingForBuiltSelected;
    public event Action<GameModeManager.GameMode> OnGameModeChanged;
    public event Action<QuestData> OnQuestProgressChanged;
    public event Action OnLanguageChanged;
    public event Action<SystemLanguage> OnLanguageSeleceted;
    public event Action<bool> OnMusicToggled;
    public event Action<bool> OnSoundToggled;
    public event Action OnLevelRestart;


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
    public void MoneyChanged(int value) => OnMoneyChanged?.Invoke(value);
    public void ResourceAmountUpdated(ResourceType type, int value) => OnResourceAmountUpdated?.Invoke(type, value);
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
    public void CreateGroundTile(Vector3Int cell, TerrainType terrain) => OnCreateGroundTile?.Invoke(cell, terrain);
    public void CreateResourceTile(Vector3Int cell, ResourceType type) => OnCreateResourceTile?.Invoke(cell, type);
    public void CreateRiverMouthTile(Vector3Int cell, Vector2Int direction) => OnCreateRiverMouthTile?.Invoke(cell, direction);
    public void AllQuestsCompleted(int levelIndex) => OnAllQuestsCompleted?.Invoke(levelIndex);
    public void LevelCompleted(int levelIndex) => OnLevelCompleted?.Invoke(levelIndex);
    public void ProgressChanged(ProgressData progress) => OnGameProgressChanged?.Invoke(progress);
    public void ResetGameProgress() => OnResetGameProgress?.Invoke();
    public void RequestLevelProgressSave() => OnRequestLevelProgressSave?.Invoke();
    public void OrdersLoadedFromSave(List<OrderData> orders, bool isAcceptedOrders) => OnOrdersLoadedFromSave?.Invoke(orders, isAcceptedOrders);
    public void OrderAccept(string orderId) => OnOrderAccept?.Invoke(orderId);
    public void ProductionFactoryBuilt(ProductionFactory factory) => OnProductionFactoryBuilt?.Invoke(factory);
    public void ProductionFactoryDeleted(ProductionFactory factory) => OnProductionFactoryDeleted?.Invoke(factory);
    public void PeriodTimerUpdated(float currentTime, float maxTime) => OnPeriodTimerUpdated?.Invoke(currentTime, maxTime);
    public void BuildingForBuiltSelected(BuildingData data) => OnBuildingForBuiltSelected?.Invoke(data);
    public void GameModeChanged(GameModeManager.GameMode gameModeManager) => OnGameModeChanged?.Invoke(gameModeManager);
    public void QuestProgressChanged(QuestData questData) => OnQuestProgressChanged?.Invoke(questData);
    public void LanguageChanged() => OnLanguageChanged?.Invoke();
    public void LanguageSelected(SystemLanguage lang) => OnLanguageSeleceted?.Invoke(lang);
    public void MusicToggled(bool isEnabled) => OnMusicToggled?.Invoke(isEnabled);
    public void SoundToggled(bool isEnabled) => OnSoundToggled?.Invoke(isEnabled);
    public void LevelRestart() => OnLevelRestart?.Invoke();
}
