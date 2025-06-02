using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class BuildMenuController : MonoBehaviour
{
    public static BuildMenuController Instance { get; private set; }

    [Header("Ссылки")]
    public GameObject buildMenu;
    public Transform buildingsContent;
    public BuildingData[] allBuildings;
    public Button buildingMenuButton;
    public Button buildingMenuExitButton;
    public Button cancelBuildingButton;
    public Button demolitionButton;

    [Header("Префабы")]
    public GameObject buildingButtonPrefab;

    [Header("Типы Зданий")]
    public Button[] typeButtons;

    public Color selectedColor = Color.yellow;

    private List<BuildingButton> spawnedButtons = new List<BuildingButton>();
    private BuildingType currentType;

    private void Awake()
    {
        InitializeSingleton();
        Initialize();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "BuildMenuController";

        Debug.Log($"Синглтон {gameObject.name} создан");
    }
    private void Initialize()
    {
        InitializeButton();
        GenerateBuildingButtons();
        StartMenuSettings();

        Debug.Log($"Инициализация {gameObject.name} завершена");
    }
    private void InitializeButton()
    {
        buildingMenuButton.onClick.AddListener(() => ToggleBuildMenu(true));
        buildingMenuExitButton.onClick.AddListener(() => ToggleBuildMenu(false));
        cancelBuildingButton.onClick.AddListener(() => StartObservationMode());
        demolitionButton.onClick.AddListener(() => StartDemolitionMode());

        for (int i = 0; i < typeButtons.Length; i++)
        {
            BuildingType type = (BuildingType)i;
            typeButtons[i].onClick.AddListener(() => ShowBuildingsByType(type));
        }
    }
    private void ToggleBuildMenu(bool active)
    {
        buildMenu.SetActive(active);
        demolitionButton.gameObject.SetActive(active);
        buildingMenuButton.gameObject.SetActive(!active);
        if (buildMenu.activeSelf) UpdateBuildingVisibility();
    }
    private void ShowBuildingsByType(BuildingType type)
    {
        currentType = type;
        UpdateTypeButtonsVisual();
        UpdateBuildingVisibility();
    }
    private void UpdateTypeButtonsVisual()
    {
        for (int i = 0; i < typeButtons.Length; i++)
        {
            Image buttonImage = typeButtons[i].GetComponent<Image>();
            buttonImage.color = ((BuildingType)i == currentType) ? selectedColor : Color.white;
        }
    }
    private void UpdateBuildingVisibility()
    {
        foreach (BuildingButton button in spawnedButtons)
        {
            bool shouldShow = button.Data.buildingType == currentType;
            button.SetVisible(shouldShow);
        }
    }

    private void GenerateBuildingButtons()
    {
        foreach (BuildingData data in allBuildings)
        {
            GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingsContent);
            BuildingButton button = buttonObj.GetComponent<BuildingButton>();
            button.Initialize(data);
            spawnedButtons.Add(button);
        }
    }

    private void StartMenuSettings()
    {
        buildMenu.SetActive(false);
        cancelBuildingButton.gameObject.SetActive(false);
        demolitionButton.gameObject.SetActive(false);
    }

    public void UpdateUI()
    {
        foreach (BuildingButton button in spawnedButtons)
        {            
            button.UpdateCostColors(GameManager.Instance.money >= button.Data.cost);
        }
    }
    public void StartBuildingMode()
    {
        GameModeManager.Instance.EnterToBuildingMode();
        cancelBuildingButton.gameObject.SetActive(true);
    }
    private void StartObservationMode()
    {
        BuildingSystem.Instance.CancelConstruction();
        GameModeManager.Instance.EnterToObservationMode();
        cancelBuildingButton.gameObject.SetActive(false);
    }
    public void StartDemolitionMode()
    {
        BuildingSystem.Instance.StartDemolition();
        cancelBuildingButton.gameObject.SetActive(true);
    }
}
