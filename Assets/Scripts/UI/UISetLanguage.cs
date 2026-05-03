using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UISetLanguage : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    private List<SystemLanguage> languages = new();

    private void Awake()
    {
        InitializeDropDown();
    }
    private void OnDestroy()
    {
        UninitializeDropDown();
    }

    private void InitializeDropDown()
    {
        if (dropdown == null)
        {
            Debug.LogError("UISetLanguage: dropdown is not assigned.");
            return;
        }

        dropdown.ClearOptions();
        languages.Clear();

        var manager = LocalizationManager.Instance;
        if (manager == null)
        {
            Debug.LogError("UISetLanguage: LocalizationManager.Instance is null.");
            return;
        }

        var locales = manager.AvailableLocales;
        List<TMP_Dropdown.OptionData> options = new();

        if (locales == null || locales.Count == 0)
        {
            // fallback Ч минимум English
            options.Add(new TMP_Dropdown.OptionData("English"));
            languages.Add(SystemLanguage.English);
        }
        else
        {
            foreach (var entry in locales)
            {
                var option = new TMP_Dropdown.OptionData(entry.DisplayName, entry.Flag, Color.white);
                options.Add(option);
                languages.Add(entry.Language);
            }
        }

        dropdown.AddOptions(options);
        dropdown.onValueChanged.AddListener(SetLanguage);

        GetSelectedLanguage();
    }

    private void UninitializeDropDown()
    {
        if (dropdown != null)
            dropdown.onValueChanged.RemoveListener(SetLanguage);
    }

    private void SetLanguage(int index)
    {
        if (index < 0 || index >= languages.Count) return;
        var selected = languages[index];
        if (EventBusManager.Instance != null)
            EventBusManager.Instance.LanguageSelected(selected);
    }

    private void GetSelectedLanguage()
    {
        if (LocalizationManager.Instance == null || dropdown == null) return;

        SystemLanguage currentLanguage = LocalizationManager.Instance.CurrentLanguage;
        int index = languages.IndexOf(currentLanguage);
        if (index >= 0)
        {
            dropdown.value = index;
        }
        else if (languages.Count > 0)
        {
            dropdown.value = 0;
        }
    }
}
