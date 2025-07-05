using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System;

public class UIBuildMenuController : MonoBehaviour, IUIWindow
{
    [Header("Ссылки")]
    public GameObject targetWindow;
    public Button buildingMenuButton;
    public Button buildingMenuExitButton;
    public Button cancelBuildingButton;
    public Button demolitionButton;

    public Transform buildingsContent;
    public BuildingData[] allBuildings;

    [Header("Префабы")]
    public GameObject buildingButtonPrefab;

    [Header("Типы Зданий")]
    public Button[] typeButtons;

    public Color selectedColor = Color.yellow;

    private List<UIBuildingButton> spawnedButtons = new List<UIBuildingButton>();
    private BuildingType currentType;

    private void OnDestroy()
    {
        UninitializeEvents();
    }

    public void Initialize()
    {
        InitializeButtons();
        InitializeBuildingButtons();
        InitializeMenuSettings();
        InitializeEvents();
    }
    private void InitializeButtons()
    {
        buildingMenuExitButton.onClick.AddListener(OnExitButtonClick);
        cancelBuildingButton.onClick.AddListener(StartObservationMode);
        demolitionButton.onClick.AddListener(StartDemolitionMode);

        for (int i = 0; i < typeButtons.Length; i++)
        {
            BuildingType type = (BuildingType)i;
            typeButtons[i].onClick.AddListener(() => ShowBuildingsByType(type));
        }
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
        foreach (UIBuildingButton button in spawnedButtons)
        {
            bool shouldShow = button.Data.buildingType == currentType;
            button.SetVisible(shouldShow);
        }
    }
    private void InitializeBuildingButtons()
    {
        foreach (BuildingData data in allBuildings)
        {
            GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingsContent);
            UIBuildingButton button = buttonObj.GetComponent<UIBuildingButton>();
            button.Initialize(data);
            spawnedButtons.Add(button);
        }
    }
    private void InitializeMenuSettings()
    {
        targetWindow.SetActive(false);
        cancelBuildingButton.gameObject.SetActive(false);
        demolitionButton.gameObject.SetActive(false);
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnSwitchToBuildingGameMode += StartBuildingMode;//
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnSwitchToBuildingGameMode -= StartBuildingMode;
    }

    private void OnExitButtonClick()
    {
        EventBusManager.Instance.WindowCloseRequested(this);
    }

    public void OpenWindow()
    {
        targetWindow.SetActive(true);
        demolitionButton.gameObject.SetActive(true);
        buildingMenuButton.gameObject.SetActive(false);
        UpdateBuildingVisibility();
    }
    public void CloseWindow()
    {
        targetWindow.SetActive(false);
        demolitionButton.gameObject.SetActive(false);
        buildingMenuButton.gameObject.SetActive(true);
    }

    public void StartBuildingMode()
    {
        cancelBuildingButton.gameObject.SetActive(true);
    }
    private void StartObservationMode()
    {
        BuildingManager.Instance.CancelConstruction();
        EventBusManager.Instance.SwitchToObservationGameMode();
        cancelBuildingButton.gameObject.SetActive(false);
    }
    public void StartDemolitionMode()
    {
        BuildingManager.Instance.StartDemolition();
        cancelBuildingButton.gameObject.SetActive(true);
    }
}
