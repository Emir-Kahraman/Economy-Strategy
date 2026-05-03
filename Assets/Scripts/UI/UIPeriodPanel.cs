using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPeriodPanel : MonoBehaviour, IUIPanel
{
    [SerializeField] private TextMeshProUGUI periodText;
    [SerializeField] private Slider periodTimer;

    public void Initialize()
    {
        EventBusManager.Instance.OnCurrentPeriodUpdated += UpdateCurrentPeriodText;
        EventBusManager.Instance.OnPeriodTimerUpdated += UpdatePeriodTimer;
    }
    public void Uninitialize()
    {
        EventBusManager.Instance.OnCurrentPeriodUpdated -= UpdateCurrentPeriodText;
        EventBusManager.Instance.OnPeriodTimerUpdated -= UpdatePeriodTimer;
    }

    private void UpdateCurrentPeriodText(Period currentPeriod)
    {
        switch (currentPeriod)
        {
            case Period.I:
                periodText.text = "I";
                break;
            case Period.II:
                periodText.text = "II";
                break;
            case Period.III:
                periodText.text = "III";
                break;
            case Period.IV:
                periodText.text = "IV";
                break;
            default:
                periodText.text = "Error";
                break;
        }
    }
    private void UpdatePeriodTimer(float currentTime, float maxTime)
    {
        if (maxTime == 0) maxTime = 1;
        periodTimer.value = 1 - (currentTime / maxTime);
    }
}
