using Unity.VisualScripting;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildMenuController : MonoBehaviour
{
    public static BuildMenuController Instance;

    [Header("Ссылки")]
    public GameObject buildMenu;
    public Transform buildingsContent;
    public BuildingData[] allBuildings;
    public Button buildingMenuButton;
    public Button buildingMenuExitButton;
    public Button cancelBuildingButton;

    [Header("Префабы")]
    public GameObject buildingButtonPrefab;

    [Header("Типы Зданий")]
    public Button[] typeButtons;

    public Color selectedColor = Color.yellow;

    private List<BuildingButton> spawnedButtons = new List<BuildingButton>();
    private BuildingType currentType;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        buildMenu.SetActive(false);
        cancelBuildingButton.gameObject.SetActive(false);
        GenerateBuildingButtons();
        InitializeButton();
    }
    
    void GenerateBuildingButtons()
    {
        foreach (BuildingData data in allBuildings)
        {
            GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingsContent);
            BuildingButton button = buttonObj.GetComponent<BuildingButton>();
            button.Initialize(data);
            spawnedButtons.Add(button);
        }
    }

    private void InitializeButton()
    {
        buildingMenuButton.onClick.AddListener(() => ToggleBuildMenu(true));
        buildingMenuExitButton.onClick.AddListener(() => ToggleBuildMenu(false));
        cancelBuildingButton.onClick.AddListener(() => ExitBuildingMode());

        for(int i = 0; i < typeButtons.Length; i++)
        {
            BuildingType type = (BuildingType)i;
            typeButtons[i].onClick.AddListener(() => ShowBuildingsByType(type));
        }
    }

    public void ShowBuildingsByType(BuildingType type)
    {
        currentType = type;
        UpdateTypeButtonsVisual();
        UpdateBuildingVisibility();
    }

    void UpdateTypeButtonsVisual()
    {
        for (int i = 0; i < typeButtons.Length; i++)
        {
            Image buttonImage = typeButtons[i].GetComponent<Image>();
            buttonImage.color = ((BuildingType)i == currentType) ? selectedColor : Color.white;
        }
    }

    void UpdateBuildingVisibility()
    {
        foreach (BuildingButton button in spawnedButtons)
        {
            bool shouldShow = button.Data.buildingType == currentType;
            button.SetVisible(shouldShow);
        }
    }

    private void ToggleBuildMenu(bool active)
    {
        buildMenu.SetActive(active);
        buildingMenuButton.gameObject.SetActive(!active);
        if (buildMenu.activeSelf) UpdateBuildingVisibility();
    }
    private void ExitBuildingMode()
    {
        BuildingSystem.Instance.CancelBuilding();
        GameModeManager.Instance.ExitBuildingMode();
        cancelBuildingButton.gameObject.SetActive(false);
    }
    public void OnBuildingSelected()
    {
        GameModeManager.Instance.EnterBuildingMode();
        cancelBuildingButton.gameObject.SetActive(true);
    }
}
