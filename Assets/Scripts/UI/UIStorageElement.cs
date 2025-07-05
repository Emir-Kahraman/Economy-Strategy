using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIStorageElement : MonoBehaviour
{
    [SerializeField] private GameObject targetWindow;
    [Space]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI volumeText;

    private ResourceType resourceType;
    private string resourceName;
    private float perUnitVolume;

    private int currentAmount;
    private float currentVolume;

    public float GetCurrentVolume() => currentVolume;

    public void SetData(ResourceData data)
    {
        resourceType = data.Type;
        resourceName = data.Name;
        perUnitVolume = data.VolumePerUnit;

        currentAmount = 0;
        currentVolume = 0;

        SetUIElements(data);
    }
    private void SetUIElements(ResourceData data)
    {
        icon.sprite = data.Icon;
        nameText.text = data.Name;

        UIElementsUpdate();
    }
    public void SetAmount(int newAmount)
    {
        currentAmount = newAmount;
        currentVolume = currentAmount * perUnitVolume;

        UIElementsUpdate();
    }

    private void UIElementsUpdate()
    {
        amountText.text = currentAmount.ToString();
        volumeText.text = currentVolume.ToString();
    }
}
