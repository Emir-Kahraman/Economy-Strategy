using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBankruptcyPanel : MonoBehaviour, IUIPanel
{
    [SerializeField] private GameObject bankruptcyPanel;
    [SerializeField] private TextMeshProUGUI bankruptcyText;
    [SerializeField] private Slider bankruptcyProcessSlider;

    private string bankruptcyName = "Bankruptcy!";
    private float bankruptcyProcess = 0f;

    public void Initialize()
    {
        InitializeParameters();
        InitializeEvents();
    }
    public void Uninitialize()
    {
        UninitializeEvents();
    }
    private void InitializeParameters()
    {
        bankruptcyText.text = bankruptcyName;
        bankruptcyProcessSlider.minValue = 0f;
        bankruptcyProcessSlider.maxValue = 30f;
        bankruptcyProcessSlider.value = 0f;
        bankruptcyPanel.SetActive(false);
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnBankruptcyProcess += UpdateBankruptcyProcess;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnBankruptcyProcess -= UpdateBankruptcyProcess;
    }

    private void UpdateBankruptcyProcess(float value)
    {
        if (value <= 0f) bankruptcyPanel.SetActive(false);
        else bankruptcyPanel.SetActive(true);

        bankruptcyProcess = value;
        bankruptcyProcessSlider.value = bankruptcyProcess;
    }
}
