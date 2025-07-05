using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    public enum GameMode {Observation, Building, Demolition, ResourceAllocation}
    public GameMode CurrentMode { get; private set; }

    void Awake()
    {        
        InitializeSingleton();
        Initialize();
    }
    private void OnDestroy()
    {
        UninitializeEvents();
    }

    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "GameModeManager";
    }

    private void Initialize()
    {
        InitializeStartGameParameters();
        InitializeEvents();
    }
    private void InitializeStartGameParameters()
    {
        CurrentMode = GameMode.Observation;
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnSwitchToObservationGameMode += EnterToObservationMode;
        EventBusManager.Instance.OnSwitchToBuildingGameMode += EnterToBuildingMode;
        EventBusManager.Instance.OnSwitchToDemolitionGameMode += EnterToDemolitionMode;
        EventBusManager.Instance.OnSwitchToResourceAllocationMode += EnterToResourceAllocationMode;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnSwitchToObservationGameMode -= EnterToObservationMode;
        EventBusManager.Instance.OnSwitchToBuildingGameMode -= EnterToBuildingMode;
        EventBusManager.Instance.OnSwitchToDemolitionGameMode -= EnterToDemolitionMode;
        EventBusManager.Instance.OnSwitchToResourceAllocationMode -= EnterToResourceAllocationMode;
    }

    private void EnterToObservationMode() => CurrentMode = GameMode.Observation;
    private void EnterToBuildingMode() => CurrentMode = GameMode.Building;
    private void EnterToDemolitionMode() => CurrentMode = GameMode.Demolition;
    private void EnterToResourceAllocationMode() => CurrentMode = GameMode.ResourceAllocation;
}
