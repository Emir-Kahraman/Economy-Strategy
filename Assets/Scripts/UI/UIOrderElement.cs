using System.Collections;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIOrderElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI acceptedTimeText;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Button orderButton;

    private UIOrdersMenuController parentController;
    private string id;
    private float existenceTime;
    private float completionTime;
    private ResourceData resourceData;
    private int resourceAmount;
    private int rewardAmount;

    private bool acceptedOrder;
    private float time;
    public void Initialize(OrderData data, UIOrdersMenuController parent)
    {
        id = data.id;
        existenceTime = data.existenceTime;
        completionTime = data.completionTime;
        resourceData = data.resourceData;
        resourceAmount = data.resourceAmount;
        rewardAmount = data.reward;

        time = existenceTime;
        acceptedOrder = false;

        parentController = parent;

        InitializeUIElements();
    }
    private void InitializeUIElements()
    {
        InitializeUITimer(timeText, time);
        InitializeUITimer(acceptedTimeText, completionTime);

        icon.sprite = resourceData.Icon;
        amountText.text = resourceAmount.ToString();
        rewardText.text = rewardAmount.ToString();

        orderButton.onClick.AddListener(AcceptOrder);
    }
    private void InitializeUITimer(TextMeshProUGUI text, float timer)
    {
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);

        text.text = $"{minutes:00}:{seconds:00}";
    }

    private void AcceptOrder()
    {
        if (acceptedOrder) return;
        if (!OrdersManager.Instance.CanAcceptOrder()) return;

        time = completionTime;
        acceptedOrder = true;
        acceptedTimeText.gameObject.SetActive(false);
        parentController.OrderAccepted(id);
        orderButton.onClick.RemoveAllListeners();
        orderButton.onClick.AddListener(SubmitOrderResources);
    }
    private void SubmitOrderResources()
    {
        int providedAmount = StorageManager.Instance.ConsumeResource(resourceData.Type, resourceAmount);
        resourceAmount -= providedAmount;
        amountText.text = resourceAmount.ToString();
        if (resourceAmount == 0) CompleteOrder();
    }
    private void CompleteOrder()
    {
        CurrencyManager.Instance.AddMoney(rewardAmount);
        DeleteOrder();
    }
    private void FailedOrder()
    {
        CurrencyManager.Instance.SpendMoney(rewardAmount);
        DeleteOrder();
    }
    public void UpdateTimer(float deltaTime)
    {
        if (time <= 0)
        {
            if (acceptedOrder) FailedOrder();
            else DeleteOrder();
            return;
        }
        time -= Time.deltaTime;
        time = Mathf.Max(time, 0);

        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    private void DeleteOrder()
    {
        parentController.OrderDeleted(id, acceptedOrder);
    }
}
