using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class UIResourceAllocationStorageSubController : MonoBehaviour, IUIWindow
{
    [SerializeField] private GameObject targetWindow;
    [SerializeField] private TextMeshProUGUI conditionNameText;
    [SerializeField] private TextMeshProUGUI certainAmountText;
    [SerializeField] private Slider certainAmountSlider;
    [SerializeField] private Button closeButton;

    private ProductionFactory targetFactory;
    private ProductionFactory.ProductionCondition targetCondition;
    private UIFactoryWindowController parentController;

    private int certainAmount;

    public void Initialize()
    {
        InitializeButtons();
        InitializeObjects();
        CloseWindow();
    }
    private void InitializeButtons()
    {
        closeButton.onClick.AddListener(CloseWindowRequest);
    }
    private void InitializeObjects()
    {
        certainAmountSlider.onValueChanged.AddListener(SliderValueChanged);
    }
    public void SetData(ProductionFactory factory, ProductionFactory.ProductionCondition condition, string conditionName, UIFactoryWindowController parent)
    {
        targetFactory = factory;
        targetCondition = condition;

        conditionNameText.text = conditionName;

        parentController = parent;

        SetSliderParameters();
    }
    private void SetSliderParameters()
    {
        certainAmount = targetCondition.requestedAmount;
        certainAmountText.text = certainAmount.ToString();
        certainAmountSlider.maxValue = targetCondition.requiredAmount;
        certainAmountSlider.value = certainAmount;
    }
    private void SliderValueChanged(float value)
    {
        certainAmount = (int)value;
        certainAmountText.text = certainAmount.ToString();
        targetFactory.ChangeRequestedAmount(certainAmount, targetCondition.requiredResource);
    }
    private void CloseWindowRequest()
    {
        EventBusManager.Instance.WindowCloseRequested(this);
    }
    public void OpenWindow()
    {
        targetWindow.SetActive(true);
    }
    public void CloseWindow()
    {
        if (targetFactory != null) targetFactory.SetPaused(false);
        targetWindow.SetActive(false);

    }
}
