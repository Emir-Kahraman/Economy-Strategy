using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    public enum GameMode {Observation, Building, Demolition}
    public GameMode CurrentMode { get; private set; }

    void Awake()
    {        
        InitializeSingleton();
        Initialize();
    }

    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "GameModeManager";

        Debug.Log($"Синглтон {gameObject.name} создан");
    }

    private void Initialize()
    {
        CurrentMode = GameMode.Observation;

        Debug.Log($"Инициализация {gameObject.name} завершена");
    }

    public void EnterToBuildingMode()
    {
        CurrentMode = GameMode.Building;        
    }

    public void EnterToObservationMode()
    {
        CurrentMode = GameMode.Observation;
    }

    public void EnterToDemolitionMode()
    {
        CurrentMode = GameMode.Demolition;
    }
}
