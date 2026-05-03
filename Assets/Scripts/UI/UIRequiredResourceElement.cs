using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRequiredResourceElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI requiredText;
    [SerializeField] private TextMeshProUGUI availableText;
    [SerializeField] private Image iconImage;

    private ResourceType resourceType;
    private int requiredAmount;

    public ResourceType ResourceType => resourceType;

    public void Setup(ResourceType type, int required, Sprite icon)
    {
        resourceType = type;
        requiredAmount = required;
        iconImage.sprite = icon;
        requiredText.text = required.ToString();
        UpdateAvailableAmount(StorageManager.Instance.GetResourceCount(type));
    }

    public void UpdateAvailableAmount(int available)
    {
        availableText.text = available.ToString();
    }
}
