using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIConditionElement : MonoBehaviour
{
    public Image conditionIcon;
    public Button editButton;
    public TextMeshProUGUI conditionText;
    public Image statusIndicator;
    [Space]
    public Color metColor = Color.green;
    public Color partiallyColor = Color.yellow;
    public Color notMetColor = Color.red;

    private ProductionFactory.ProductionCondition condition;
    private UIFactoryWindowController parentWindow;

    public void Initialize(ConditionUIData data, ProductionFactory.ProductionCondition condition, UIFactoryWindowController parentWindow)
    {
        conditionIcon.sprite = data.icon;
        conditionText.text = data.description;

        switch (data.status)
        {
            case ConditionStatus.FullyMet:
                statusIndicator.color = metColor;
                break;
            case ConditionStatus.PartiallyMet:
                statusIndicator.color = partiallyColor;
                break;
            default:
                statusIndicator.color = notMetColor;
                break;
        }

        this.condition = condition;
        this.parentWindow = parentWindow;

        ConfigureEditButton();
    }
    private void ConfigureEditButton()
    {
        editButton.onClick.RemoveAllListeners();
        editButton.onClick.AddListener(OnEditButtonClicked);
    }
    private void OnEditButtonClicked()
    {
        switch (condition.conditionType)
        {
            case ProductionFactory.ProductionCondition.ConditionType.EnvironmentTile:
                parentWindow.OpenAllocationEnvironmentEditor(condition);
                break;
            case ProductionFactory.ProductionCondition.ConditionType.StorageResource:
                parentWindow.OpenAllocationStorageEditor(condition);
                break;
        }
    }
}
