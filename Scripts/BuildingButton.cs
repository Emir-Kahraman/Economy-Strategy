using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingButton : MonoBehaviour
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

    public void UpdateCostColors(bool hasCost)
    {
        costText.color = hasCost ? Color.white : Color.red;
    }

    private void OnClick()
    {
        BuildMenuController.Instance.StartBuildingMode();
        BuildingSystem.Instance.StartBuilding(data);
    }
    
}
