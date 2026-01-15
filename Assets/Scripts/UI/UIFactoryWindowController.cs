using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public enum ConditionStatus
{
    FullyMet,
    PartiallyMet,
    NotMet,
}
public class ConditionUIData
{
    public Sprite icon;
    public string description;
    public ConditionStatus status;
}

public class UIFactoryWindowController : MonoBehaviour, IUIWindow
{
    [SerializeField] private GameObject targetWindow;
    [SerializeField] private TextMeshProUGUI factoryNameText;
    [SerializeField] private Slider productionProgressSlider;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI outputResourceText;
    [SerializeField] private Button switchStatusButton;
    [SerializeField] private Button closeButton;

    [Header("Condition UI")]
    [SerializeField] private Transform inputConditionContainer;
    [SerializeField] private GameObject conditionElementPrefab;
    [SerializeField] private Sprite storageConditionIcon;
    [SerializeField] private Sprite environmentConditionIcon;
    [SerializeField] private TextMeshProUGUI serviceCostText;

    [Header("Sub Controllers")]
    [SerializeField] private UIResourceAllocationEnvironmentSubController allocationEnvironmentController;
    [SerializeField] private UIResourceAllocationStorageSubController allocationStorageController;
    
    private List<UIConditionElement> conditionElements = new();
    private ProductionFactory targetFactory;
    private bool isUpdating;

    public void Initialize()
    {
        InitializeButtons();
        InitializeSubControllers();
        InitializeStartSettings();
    }
    private void InitializeButtons()
    {
        switchStatusButton.onClick.AddListener(SwitchStatus);
        closeButton.onClick.AddListener(CloseWindowRequest);
    }
    private void InitializeSubControllers()
    {
        allocationEnvironmentController.Initialize();
        allocationStorageController.Initialize();
    }
    private void InitializeStartSettings()
    {
        CloseWindow();
    }

    public void SetFactory(ProductionFactory factory)
    {
        targetFactory = factory;
        CreateConditionElements();
        SetUIElements();
    }
    private void SetUIElements()
    {
        factoryNameText.text = targetFactory.FactoryProductionData.factoryName;
        outputResourceText.text = GetResourceName(targetFactory.FactoryProductionData.outputResource.Type);
        serviceCostText.text = targetFactory.ServiceCost.ToString();

    }
    private void CreateConditionElements()
    {
        ClearConditionElements();

        foreach (var condition in targetFactory.ProductionConditions)
        {
            GameObject elementObj = Instantiate(conditionElementPrefab, inputConditionContainer);
            UIConditionElement element = elementObj.GetComponent<UIConditionElement>();
            conditionElements.Add(element);
        }
    }
    private void ClearConditionElements()
    {
        List<UIConditionElement> toDestroy = new List<UIConditionElement>(conditionElements);
        conditionElements.Clear();

        foreach(var element in toDestroy)
        {
            if(element != null && element.gameObject != null) Destroy(element.gameObject);
        }
    }
    private void Update()
    {
        if (isUpdating && targetFactory != null) UpdateUIElements();
    }
    private void UpdateUIElements()
    {
        UpdateProductionUI();
    }

    private void UpdateProductionUI()
    {
        if (targetFactory == null) return;

        productionProgressSlider.value = targetFactory.CurrentProgress;
        statusText.text = targetFactory.IsPaused ? "Paused" : (targetFactory.IsOperational ? "Active" : "Inactive");
        switchStatusButton.GetComponentInChildren<TextMeshProUGUI>().text = targetFactory.IsPaused ? "Paused" : "Active";//сложная операция

        UpdateConditionData();
    }
    private void UpdateConditionData()
    {
        if (targetFactory == null || conditionElements.Count == 0) return;

        for (int i = 0; i < conditionElements.Count; i++)
        {
            var condition = targetFactory.ProductionConditions[i];
            conditionElements[i].Initialize(GetConditionData(condition), condition, this);
        }
    }
    private ConditionUIData GetConditionData(ProductionFactory.ProductionCondition condition)
    {
        ConditionUIData data = new();
        float efficiency = 1f;

        switch (condition.conditionType)
        {
            case ProductionFactory.ProductionCondition.ConditionType.StorageResource:
                data.icon = storageConditionIcon;

                int currentAmount = targetFactory.GetAmountResourceInProduction(condition.requiredResource.Type);
                data.description = $"{condition.requiredResource}: {currentAmount}/ {condition.requiredAmount}";
               
                efficiency = (float)currentAmount / condition.requiredAmount;

                data.status = GetStatusFromEfficiency(efficiency);
                break;

            case ProductionFactory.ProductionCondition.ConditionType.EnvironmentTile:
                data.icon = environmentConditionIcon;

                int actualAmount = targetFactory.GetAmountResourceInProduction(condition.requiredResource.Type);
                
                data.description = $"{GetResourceName(condition.requiredResource.Type)}: {actualAmount}/ {condition.requiredAmount}";

                efficiency = (float)actualAmount / condition.requiredAmount;
                data.status = GetStatusFromEfficiency(efficiency);
                break;
        }
        return data;
    }
    private ConditionStatus GetStatusFromEfficiency(float efficiency)
    {
        if(efficiency >= 0.99f) return ConditionStatus.FullyMet;
        else if (efficiency >= 0.01f) return ConditionStatus.PartiallyMet;
        return ConditionStatus.NotMet;
    }

    private string GetResourceName(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Test: return "Test";
            case ResourceType.Forest: return "Forest";
            case ResourceType.Stone_Ore: return "Stone Ore";
            case ResourceType.Wood: return "Wood";
            case ResourceType.Stone: return "Stone";
            default: return type.ToString();
        }
    }

    public void OpenAllocationEnvironmentEditor(ProductionFactory.ProductionCondition condition)
    {
        targetFactory.SetEditPaused(true);
        allocationEnvironmentController.SetData(targetFactory, condition, GetResourceName(condition.requiredResource.Type), this);
        EventBusManager.Instance.WindowOpenRequested(allocationEnvironmentController);
    }
    public void OpenAllocationStorageEditor(ProductionFactory.ProductionCondition condition)
    {
        targetFactory.SetEditPaused(true);
        allocationStorageController.SetData(targetFactory, condition, GetResourceName(condition.requiredResource.Type), this);
        EventBusManager.Instance.WindowOpenRequested(allocationStorageController);
    }
    
    private void SwitchStatus()
    {
        if(targetFactory != null)
        {
            targetFactory.SetPaused(!targetFactory.IsPaused);
        }
    }

    private void CloseWindowRequest()
    {
        EventBusManager.Instance.WindowCloseRequested(this);
    }

    public void OpenWindow()
    {
        targetWindow.SetActive(true);
        targetWindow.SetActive(true);
        isUpdating = true;
    }
    public void CloseWindow()
    {
        targetWindow.SetActive(false);
        isUpdating = false;
        ClearConditionElements();
        targetFactory = null;
    }
}
