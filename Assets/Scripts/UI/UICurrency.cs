using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class UICurrency : MonoBehaviour
{
    [SerializeField] private TMP_Text _moneyText;
    private void Awake()
    {
        EventBusManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
        
    }
    private void OnDestroy()
    {
        EventBusManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
    }
    private void UpdateMoneyDisplay(int newAmount)
    {
        _moneyText.text = newAmount.ToString();
    }
}
