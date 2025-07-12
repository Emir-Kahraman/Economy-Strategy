using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using System.Linq;

public class UIStorageMenuController : MonoBehaviour, IUIWindow
{
    [SerializeField] private GameObject targetWindow;
    [SerializeField] private TextMeshProUGUI storageVolumeText;
    [SerializeField] private Slider storageVolumeSlider;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject storageElementPrefab;
    [SerializeField] private GameObject storageElementsParent;
    [SerializeField] private Button menuButton;

    private List<ResourceData> allResources;
    private Dictionary<ResourceType, UIStorageElement> storageElements = new();

    private float currentVolume;
    private float totalCapacity;

    private float volumeUpdaterTimer = 1f;

    public void Initialize()
    {
        InitializeButtons();
        InitializeEvents();
        CloseWindow();
    }
    private void OnDestroy()
    {
        UninitializeEvents();
    }
    private void InitializeButtons()
    {
        closeButton.onClick.AddListener(CloseRequested);
    }
    private void InitializeUIElements()
    {
        storageVolumeSlider.maxValue = totalCapacity;

        UpdateUIElements();
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnResourceDataUpdated += SetResourceDates;
        EventBusManager.Instance.OnResourceUpdated += UpdateResourcesAmount;
        EventBusManager.Instance.OnStorageCapacityUpdated += UpdateStorageCapacity;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnResourceDataUpdated -= SetResourceDates;
        EventBusManager.Instance.OnResourceUpdated -= UpdateResourcesAmount;
        EventBusManager.Instance.OnStorageCapacityUpdated -= UpdateStorageCapacity;
    }

    private void SetResourceDates(List<ResourceData> resourceDates)
    {
        allResources = resourceDates;

        CreateStorageElements();

        foreach (ResourceData resourceData in allResources)
        {
            storageElements[resourceData.Type].SetData(resourceData);
        }

        UpdateConditionElementsStatus();
    }
    private void CreateStorageElements()
    {
        var allTypes = allResources.Select(r => r.Type).ToList();
        foreach (ResourceType resource in allTypes)
        {
            CreateStorageElement(resource);
        }
    }
    private void CreateStorageElement(ResourceType type)
    {
        UIStorageElement storageElement = Instantiate(storageElementPrefab, storageElementsParent.transform).GetComponent<UIStorageElement>();
        storageElements[type] = storageElement;
    }
    private void UpdateConditionElementsStatus()
    {
        foreach (ResourceType element in storageElements.Keys)
        {
            UpdateConditionElementStatus(element, 0);
        }
    }

    private void UpdateResourcesAmount(ResourceType type, int newAmount)
    {
        foreach (ResourceType element in storageElements.Keys)
        {
            if (element == type)
            {
                storageElements[element].SetAmount(newAmount);
                UpdateConditionElementStatus(type, newAmount);
            }
        }
    }

    private void UpdateConditionElementStatus(ResourceType type, int amount)
    {
        bool elementStatus = amount > 0;
        storageElements[type].gameObject.SetActive(elementStatus);
    }

    private void UpdateStorageCapacity(float newTotalCapacity)
    {
        totalCapacity = newTotalCapacity;
        storageVolumeSlider.maxValue = totalCapacity;
        InitializeUIElements();
    }
    private void Update()
    {
        UpdateCurrentVolume();
    }

    private void UpdateCurrentVolume()
    {
        volumeUpdaterTimer -= Time.deltaTime;
        if (volumeUpdaterTimer < 0)
        {
            float volumeOfResources = 0f;
            foreach (var element in storageElements.Keys)
            {
                volumeOfResources += storageElements[element].GetCurrentVolume();
            }
            currentVolume = volumeOfResources;

            UpdateUIElements();

            volumeUpdaterTimer = 1f;
        }
    }
    private void UpdateUIElements()
    {
        storageVolumeText.text = $"{currentVolume} / {totalCapacity}";
        storageVolumeSlider.value = currentVolume;
    }

    private void CloseRequested()
    {
        EventBusManager.Instance.WindowCloseRequested(this);
    }
    public void OpenWindow()
    {
        targetWindow.SetActive(true);
        menuButton.gameObject.SetActive(false);
    }
    public void CloseWindow()
    {
        targetWindow.SetActive(false);
        menuButton.gameObject.SetActive(true);
    }
}
