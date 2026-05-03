using TMPro;
using UnityEngine;

public class UICurrencyPanel : MonoBehaviour, IUIPanel
{
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private TextMeshProUGUI requiredMoneyText;
    public void Initialize()
    {
        HideRequiredMoney();
        EventBusManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
        EventBusManager.Instance.OnGameModeChanged += ShowRequiredMoneyText;
        EventBusManager.Instance.OnBuildingForBuiltSelected += RequiredMoneyUpdate;
    }
    public void Uninitialize()
    {
        EventBusManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
        EventBusManager.Instance.OnGameModeChanged -= ShowRequiredMoneyText;
        EventBusManager.Instance.OnBuildingForBuiltSelected -= RequiredMoneyUpdate;
    }
    private void UpdateMoneyDisplay(int newAmount)
    {
        currencyText.text = newAmount.ToString();
    }
    private void ShowRequiredMoneyText(GameModeManager.GameMode gameMode)
    {
        requiredMoneyText.gameObject.SetActive(gameMode == GameModeManager.GameMode.Building);
    }
    private void RequiredMoneyUpdate(BuildingData data)
    {
        requiredMoneyText.text = data.cost.ToString();
    }
    private void HideRequiredMoney()
    {
        requiredMoneyText.gameObject.SetActive(false);
    }
}
