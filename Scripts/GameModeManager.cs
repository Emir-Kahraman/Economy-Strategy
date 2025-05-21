using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    public enum GameMode { Observation, Building}
    public GameMode CurrentMode { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CurrentMode = GameMode.Observation;
    }

    public void EnterBuildingMode()
    {
        CurrentMode = GameMode.Building;        
    }

    public void ExitBuildingMode()
    {
        CurrentMode = GameMode.Observation;
    }
}
