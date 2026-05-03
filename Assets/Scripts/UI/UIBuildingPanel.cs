using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIBuildingPanel : MonoBehaviour, IUIPanel
{
    [SerializeField] private GameObject resourceRequirementPrefab;
    [SerializeField] private Transform requirementContainer;

    private List<UIRequiredResourceElement> requirementItems = new();

    public void Initialize()
    {
        ClosePanel();
        EventBusManager.Instance.OnGameModeChanged += ActivePanel;
        EventBusManager.Instance.OnBuildingForBuiltSelected += SetBuildingData;
    }
    public void Uninitialize()
    {
        EventBusManager.Instance.OnGameModeChanged -= ActivePanel;
        EventBusManager.Instance.OnBuildingForBuiltSelected -= SetBuildingData;
    }
    private void ActivePanel(GameModeManager.GameMode gameMode)
    {
        gameObject.SetActive(gameMode == GameModeManager.GameMode.Building);
    }
    private void SetBuildingData(BuildingData buildingData)
    {
        foreach (var item in requirementItems)
            Destroy(item.gameObject);
        requirementItems.Clear();
        
        gameObject.SetActive(buildingData.resourcesForBuild.Count > 0);
        
        foreach (var req in buildingData.resourcesForBuild)
        {
            var go = Instantiate(resourceRequirementPrefab, requirementContainer);
            var item = go.GetComponent<UIRequiredResourceElement>();
            item.Setup(req.Resource.Type, req.Amount, req.Resource.Icon);
            requirementItems.Add(item);
        }
    }

    private void Update()
    {
        AvailableResourceUpdate();
    }
    private void AvailableResourceUpdate()
    {
        if (!gameObject.activeSelf) return;
        foreach (var item in requirementItems)
        {
            item.UpdateAvailableAmount(StorageManager.Instance.GetResourceCount(item.ResourceType));
        }
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
