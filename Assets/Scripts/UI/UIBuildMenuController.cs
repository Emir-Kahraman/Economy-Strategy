using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;
using System.Linq;

public class UIBuildMenuController : MonoBehaviour, IUIWindow
{
    [Header("Ссылки")]
    [SerializeField] private GameObject targetWindow;
    [SerializeField] private Button enterDemolitionModeButton;
    [SerializeField] private Button enterObservationModeButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button menuButton;
    [Space]
    [SerializeField] private Transform buildingsContent;
    [SerializeField] private Transform buildingTypesContent;
    

    [Header("Prefabs")]
    [SerializeField] private GameObject buildingButtonPrefab;
    [SerializeField] private GameObject buildingTypeButtonPrefab;

    [Space]
    [SerializeField] private Color selectedBuildingsTypeColor = Color.yellow;

    private List<BuildingData> allBuildings = new();
    private List<Button> typeButtons = new();
    private Dictionary<BuildingData, UIBuildingButton> lockedBuildingButtons = new();
    private List<UIBuildingButton> unlockedBuildingButtons = new();

    private BuildingCategory currentType;

    public void Initialize()
    {
        ClearButtonCollections(typeButtons);
        ClearButtonCollections(lockedBuildingButtons);
        ClearButtonCollections(unlockedBuildingButtons);
        
        InitializeButtons();        
        InitializeMenuSettings();
        InitializeEvents();
    }
    public void Uninitialize()
    {
        UninitializeButtons();
        UninitializeEvents();
        
        ClearButtonCollections(typeButtons);
        ClearButtonCollections(lockedBuildingButtons);
        ClearButtonCollections(unlockedBuildingButtons);
    }
    private void UninitializeButtons()
    {
        UninitializeUIButtons();
        UninitializeBuildingsTypeButtons();
    }
    private void UninitializeUIButtons()
    {
        closeButton.onClick.RemoveAllListeners();
        enterObservationModeButton.onClick.RemoveAllListeners();
        enterDemolitionModeButton.onClick.RemoveAllListeners();
    }
    private void UninitializeBuildingsTypeButtons()
    {
        // ★ Очищаем слушатели перед удалением кнопок
        foreach (var button in typeButtons)
        {
            if (button != null)
                button.onClick.RemoveAllListeners();
        }
        ClearButtonCollections(typeButtons);
    }

    private void InitializeButtons()
    {
        InitializeUIButtons();
        InitializeBuildingsTypeButtons();
    }
    private void InitializeUIButtons()
    {
        closeButton.onClick.AddListener(WindowCloseRequested);
        enterObservationModeButton.onClick.AddListener(StartObservationMode);
        enterDemolitionModeButton.onClick.AddListener(StartDemolitionMode);
    }
    private void InitializeBuildingsTypeButtons()
    {
        foreach (BuildingCategory buildingType in Enum.GetValues(typeof(BuildingCategory)))
        {
            GameObject buildingTypeGO = Instantiate(buildingTypeButtonPrefab, buildingTypesContent);
            Button buildingTypeButton = buildingTypeGO.GetComponent<Button>();

            string typeName = GetTypeBuildingName(buildingType);

            buildingTypeGO.name = typeName;
            buildingTypeGO.GetComponentInChildren<TextMeshProUGUI>().text = typeName;
            
            buildingTypeButton.onClick.AddListener(() => ShowBuildingsByType(buildingType));

            typeButtons.Add(buildingTypeButton);
        }
    }
    private void InitializeMenuSettings()
    {
        currentType = BuildingCategory.Food;
        CloseWindow();
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnBuildingDataUpdated += SetBuildingDates;
        EventBusManager.Instance.OnSwitchToBuildingGameMode += StartBuildingMode;
        EventBusManager.Instance.OnCurrentPeriodUpdated += CurrentPeriodUpdated;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnBuildingDataUpdated -= SetBuildingDates;
        EventBusManager.Instance.OnSwitchToBuildingGameMode -= StartBuildingMode;
        EventBusManager.Instance.OnCurrentPeriodUpdated -= CurrentPeriodUpdated;
    }

    private void SetBuildingDates(List<BuildingData> buildings)
    {
        allBuildings = buildings;
        ClearButtonCollections(lockedBuildingButtons);
        ClearButtonCollections(unlockedBuildingButtons);
        foreach (BuildingData data in allBuildings)
        {
            GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingsContent);
            UIBuildingButton button = buttonObj.GetComponent<UIBuildingButton>();
            button.SetData(data, this);

            lockedBuildingButtons[data] = button;
            button.SetVisible(false);
        }
    }
    private void ClearButtonCollections<T>(List<T> buttonList) where T : MonoBehaviour
    {
        if (buttonList == null) return;

        for (int i = 0; i < buttonList.Count; i++)
        {
            T button = buttonList[i];
            if (button != null && button.gameObject != null)
            {
                Destroy(button.gameObject);
            }
        }
        buttonList.Clear();
    }
    private void ClearButtonCollections<TKey, TValue>(Dictionary<TKey, TValue> buttonDict) where TValue : MonoBehaviour
    {
        if (buttonDict == null) return;
        var keys = buttonDict.Keys.ToList();
        foreach (var key in keys)
        {
            if (buttonDict.TryGetValue(key, out TValue button) && button != null && button.gameObject != null)
            {
                Destroy(button.gameObject);
            }
        }
        buttonDict.Clear();
    }

    private void CurrentPeriodUpdated(Period period)
    {
        List<BuildingData> newBuildings = new List<BuildingData>();
        foreach (var building in lockedBuildingButtons)
        {
            if (building.Key.Period <= period)
            {
                unlockedBuildingButtons.Add(building.Value);
                newBuildings.Add(building.Key);
            }
        }

        foreach (var building in newBuildings)
        {
            lockedBuildingButtons.Remove(building);
        }
    }

    private void ShowBuildingsByType(BuildingCategory type)
    {
        currentType = type;
        UpdateTypeButtonsVisual();
        ChangeBuildingType();
    }
    private void UpdateTypeButtonsVisual()
    {
        for (int i = 0; i < typeButtons.Count; i++)
        {
            typeButtons[i].GetComponent<Image>().color = ((BuildingCategory)i == currentType) ? selectedBuildingsTypeColor : Color.white;
        }
    }
    private void ChangeBuildingType()
    {
        foreach (UIBuildingButton button in unlockedBuildingButtons)
        {
            bool shouldShow = button.Data.buildingCategory == currentType;
            button.SetVisible(shouldShow);
        }
    }

    private void StartBuildingMode()//
    {
        enterObservationModeButton.gameObject.SetActive(true);
    }
    private void StartObservationMode()
    {
        BuildingManager.Instance.CancelConstruction();
        EventBusManager.Instance.SwitchToObservationGameMode();
        enterObservationModeButton.gameObject.SetActive(false);
    }
    public void StartDemolitionMode()
    {
        BuildingManager.Instance.StartDemolition();
        enterObservationModeButton.gameObject.SetActive(true);
    }

    private string GetTypeBuildingName(BuildingCategory type)
    {
        switch (type)
        {
            case BuildingCategory.Food: return "<Food>";
            case BuildingCategory.Cloth: return "<Cloth>";
            case BuildingCategory.Arrangement: return "<Arrangement>";
            case BuildingCategory.Household: return "<Household>";
            default: return "<Others>";
        }
    }

    private void WindowCloseRequested()
    {
        EventBusManager.Instance.WindowCloseRequested(this);
    }

    public void OpenWindow()
    {
        targetWindow.SetActive(true);
        enterDemolitionModeButton.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(false);
        ChangeBuildingType();
    }
    public void CloseWindow()
    {
        targetWindow.SetActive(false);
        enterObservationModeButton.gameObject.SetActive(false);
        enterDemolitionModeButton.gameObject.SetActive(false);
        menuButton.gameObject.SetActive(true);
    }
}
