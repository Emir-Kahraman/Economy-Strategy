using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Rendering;

public class UIBuildingButton : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI costText;

    private BuildingData data;
    public BuildingData Data => data;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }
    private void Update()
    {
        UpdateCostColors();
    }
    public void Initialize(BuildingData buildingData)
    {        
        data = buildingData;
        icon.sprite = data.icon;
        titleText.text = data.name;
        costText.text = $"Стоимость {buildingData.cost}";
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void UpdateCostColors()
    {
        costText.color = CurrencyManager.Instance.GetCurrentMoney() >= data.cost ? Color.white : Color.red;
    }

    private void OnClick()
    {
        EventBusManager.Instance.SwitchToBuildingGameMode();
        BuildingManager.Instance.StartBuilding(data);
    }
    
}
