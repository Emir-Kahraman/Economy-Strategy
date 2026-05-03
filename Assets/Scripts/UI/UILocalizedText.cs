using TMPro;
using UnityEngine;

public class UILocalizedText : MonoBehaviour
{
    [SerializeField] private string category = "ui";
    [SerializeField] private string key;
    [SerializeField] private TextMeshProUGUI textComponent;

    private void Awake()
    {
        if (LocalizationManager.Instance != null)
        {
            UpdateText();
            EventBusManager.Instance.OnLanguageChanged += UpdateText;
        }
    }
    private void OnDestroy()
    {
        EventBusManager.Instance.OnLanguageChanged -= UpdateText;
    }
    private void UpdateText()
    {
        textComponent.text = LocalizationManager.Instance.GetText(category, key);
    }
}
