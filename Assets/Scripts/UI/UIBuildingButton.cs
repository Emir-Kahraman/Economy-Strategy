using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Rendering;

public enum BuildingType
{
    Raw,
    Storage,
    Administrative,
    Factory
}
public class UIBuildingButton : MonoBehaviour
{
    [Header("Main Information")]
    [SerializeField] private Button selectedButton;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image costIcon;
    [SerializeField] private TextMeshProUGUI serviceCost;
    [SerializeField] private Image serviceCostIcon;
    [Header("Build Information"), Space]
    [SerializeField] private Transform ResourceForBuildContent;
    [Header("Finally Product Information"), Space]
    [SerializeField] private Transform FinallyProductContent;
    [Header("Raw Materials Information"), Space]
    [SerializeField] private Transform RawMaterialContent;
    [Header("Prefabs"), Space]
    [SerializeField] private GameObject ResourceForBuildPrefab;
    [SerializeField] private GameObject FinallyProductPrefab;
    [SerializeField] private GameObject RawMaterialPrefab;
    [Header("Sprites"), Space]
    [SerializeField] private Sprite storageIcon;
    [SerializeField] private Sprite moneyIcon;
    [SerializeField] private Sprite serviceIcon;

    private BuildingData data;
    private GameObject relatedBuilding;
    private string id;
    private UIBuildMenuController parentController;

    public BuildingData Data => data;
    public string Id => id;

    public void SetData(BuildingData buildingData, UIBuildMenuController parent)
    {
        data = buildingData;
        icon.sprite = data.icon;
        titleText.text = data.GetLocalizedName();
        costText.text = buildingData.cost.ToString();
        costIcon.sprite = moneyIcon;
        relatedBuilding = buildingData.BuildingObject;

        if (data.resourcesForBuild.Count > 0)
        {
            foreach (var resource in data.resourcesForBuild)
            {
                GameObject resourceForBuildGO = Instantiate(ResourceForBuildPrefab, ResourceForBuildContent);
                resourceForBuildGO.GetComponentInChildren<TextMeshProUGUI>().text = resource.Amount.ToString();
                resourceForBuildGO.GetComponentInChildren<Image>().sprite = resource.Resource.Icon;
            }
        }
        
        parentController = parent;

        Initialize();
    }
    private void Initialize()
    {
        InitializeRelatedBuilding();
        InitializeParameters();
    }

    private void InitializeRelatedBuilding()
    {
        if (relatedBuilding == null)// If there is no related building, we assume it's a building that doesn't have specific information to display, so we hide the service cost and product information.
        {
            serviceCost.gameObject.SetActive(false);
            serviceCostIcon.gameObject.SetActive(false);
            return;
        }
        else if (relatedBuilding.GetComponent<StorageBuilding>())// If the building has a StorageBuilding component, we assume it's a storage building and display its service cost and capacity as the final product.
        {
            StorageBuilding storageBuilding = relatedBuilding.GetComponent<StorageBuilding>();

            serviceCost.text = storageBuilding.ServiceCost.ToString();
            serviceCostIcon.sprite = serviceIcon;
            GameObject finallyProductGO = Instantiate(FinallyProductPrefab, FinallyProductContent);
            finallyProductGO.GetComponentInChildren<TextMeshProUGUI>().text = storageBuilding.GetCapacity().ToString();
            finallyProductGO.GetComponentInChildren<Image>().sprite = storageIcon;
        }
        else if (relatedBuilding.GetComponent<ProductionFactory>())// If the building has a ProductionFactory component, we assume it's a production factory and display its service cost, final product, and raw materials.
        {
            ProductionFactory productionFactory = relatedBuilding.GetComponent<ProductionFactory>();

            serviceCost.text = productionFactory.ServiceCost.ToString();
            serviceCostIcon.sprite = serviceIcon;
            GameObject finallyProductGO = Instantiate(FinallyProductPrefab, FinallyProductContent);
            finallyProductGO.GetComponentInChildren<TextMeshProUGUI>().text = productionFactory.FactoryProductionData.baseOutputAmount.ToString();
            finallyProductGO.GetComponentInChildren<Image>().sprite = productionFactory.FactoryProductionData.outputResource.Icon;

            foreach (var condition in productionFactory.ProductionConditions)
            {
                GameObject rawMaterialGO = Instantiate(RawMaterialPrefab, RawMaterialContent);
                rawMaterialGO.GetComponentInChildren<TextMeshProUGUI>().text = condition.requiredAmount.ToString();
                rawMaterialGO.GetComponentInChildren<Image>().sprite = condition.requiredResource.Icon;
            }
        }
        else// If the building is not a storage or production factory, we assume it has no service cost or specific product information to display.
        {
            serviceCost.gameObject.SetActive(false);
            serviceCostIcon.gameObject.SetActive(false);
        }
    }

    private void InitializeParameters()
    {
        selectedButton.onClick.AddListener(OnBuildingSelected);
    }

    private void OnBuildingSelected()
    {
        EventBusManager.Instance.SwitchToBuildingGameMode();
        BuildingManager.Instance.StartBuilding(data);
        EventBusManager.Instance.BuildingForBuiltSelected(data);
    }
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
