using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class LocalizationManager : MonoBehaviour //AI
{
    public static LocalizationManager Instance;

    public class LanguageEntry
    {
        public string Code;
        public SystemLanguage Language;
        public string DisplayName;
        public Sprite Flag;
        public TextAsset JsonAsset;
    }

    private SystemLanguage currentLanguage;
    private Dictionary<string, Dictionary<string, string>> currentJsonData;
    private readonly Dictionary<SystemLanguage, Dictionary<string, Dictionary<string, string>>> localeCache = new();

    private readonly List<LanguageEntry> availableLocales = new();
    public SystemLanguage CurrentLanguage => currentLanguage;
    public IReadOnlyList<LanguageEntry> AvailableLocales => availableLocales;

    private void Awake()
    {
        Initialize();
    }
    private void OnDestroy()
    {
        UninitializeEvents();
    }
    private void Initialize()
    {
        InitializeSingleton();
        ScanLocalesInResources();
        InitializeLanguageSettings();
        InitializeEvents();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "LocalizationManager";
    }
    private void ScanLocalesInResources()
    {
        availableLocales.Clear();

        var jsonAssets = Resources.LoadAll<TextAsset>("Localization");
        var flagSprites = Resources.LoadAll<Sprite>("Localization/Flags") ?? Array.Empty<Sprite>();

        foreach (var asset in jsonAssets)
        {
            if (asset == null) continue;

            string code = asset.name.ToLowerInvariant();
            SystemLanguage lang = CodeToSystemLanguage(code);
            string display = CodeToDisplayName(code);

            Sprite flag = flagSprites.FirstOrDefault(s => string.Equals(s.name, code, StringComparison.OrdinalIgnoreCase));
            availableLocales.Add(new LanguageEntry
            {
                Code = code,
                Language = lang,
                DisplayName = display,
                Flag = flag,
                JsonAsset = asset
            });
        }
    }
    private SystemLanguage CodeToSystemLanguage(string code)
    {
        return code.ToLower() switch
        {
            "ru" => SystemLanguage.Russian,
            "en" => SystemLanguage.English,
            "de" => SystemLanguage.German,
            "fr" => SystemLanguage.French,
            "es" => SystemLanguage.Spanish,
            "zh" => SystemLanguage.Chinese,
            "ja" => SystemLanguage.Japanese,
            "ko" => SystemLanguage.Korean,
            _ => SystemLanguage.English
        };
    }
    private string CodeToDisplayName(string code)
    {
        return code.ToLower() switch
        {
            "ru" => "Russian",
            "en" => "English",
            "de" => "German",
            "fr" => "French",
            "es" => "Spanish",
            "zh" => "Chinese",
            "ja" => "Japanese",
            "ko" => "Korean",
            _ => code.ToUpper()
        };
    }
    private void InitializeLanguageSettings()
    {
        SystemLanguage savedLang = GetSavedLanguage();
        SetLanguage(savedLang);
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnLanguageSeleceted += SetLanguage;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnLanguageSeleceted -= SetLanguage;
    }

    public void SetLanguage(SystemLanguage language)
    {
        currentLanguage = language;

        LoadJsonLocalization(language);
        
        SaveLanguage(language);
        EventBusManager.Instance.LanguageChanged();
    }
    private void LoadJsonLocalization(SystemLanguage language)
    {
        if (localeCache.TryGetValue(language, out var cached))
        {
            currentJsonData = cached;
            return;
        }

        string code = GetLanguageCode(language);
        string path = $"Localization/{code}";
        TextAsset jsonAsset = Resources.Load<TextAsset>(path);

        if (jsonAsset == null)
        {
            Debug.LogWarning($"Localization JSON not found at Resources/{path}. Trying fallback to English.");

            if (language != SystemLanguage.English)
            {
                LoadJsonLocalization(SystemLanguage.English);
                return;
            }

            currentJsonData = null;
            localeCache[language] = currentJsonData;
            return;
        }

        try
        {
            var parsed = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonAsset.text) ?? new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            var normalized = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var cat in parsed)
            {
                var inner = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var kv in cat.Value)
                {
                    inner[kv.Key] = kv.Value;
                }
                normalized[cat.Key] = inner;
            }

            currentJsonData = normalized;
            localeCache[language] = normalized;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse localization JSON for '{code}': {ex.Message}");
            currentJsonData = null;
            localeCache[language] = currentJsonData;
        }
    }
    private string GetLanguageCode(SystemLanguage language)
    {
        return language switch
        {
            SystemLanguage.English => "en",
            SystemLanguage.Russian => "ru",
            _ => "en"
        };
    }
    public string GetText(string category, string key)
    {
        if (!string.IsNullOrEmpty(category) && currentJsonData != null)
        {
            if (currentJsonData.TryGetValue(category, out var categoryDict))
            {
                if (categoryDict != null && categoryDict.TryGetValue(key, out var value))
                {
                    // ★ КЛЮЧЕВОЕ: если значение пустое — использовать fallback
                    if (!string.IsNullOrEmpty(value))
                        return value;

                    Debug.LogWarning($"Localization: empty value for key '{key}' in category '{category}' ({currentLanguage}). Using fallback...");
                    
                    // Fallback 1: попробуем английский (если текущий язык не английский)
                    if (currentLanguage != SystemLanguage.English)
                    {
                        string enValue = GetEnglishFallback(category, key);
                        if (!string.IsNullOrEmpty(enValue))
                        {
                            Debug.Log($"Localization: fallback to English for '{category}/{key}'");
                            return enValue;
                        }
                    }

                    // Fallback 2: если всё ещё пусто — возвращаем ключ
                    Debug.LogError($"Localization: no translation found for key '{key}' in category '{category}' ({currentLanguage}). Returning key.");
                    return key;
                }
            }
        }

        Debug.LogWarning($"Localization: missing key '{key}' in category '{category}' for language '{currentLanguage}'. Returning key as fallback.");
        return key;
    }

    // ★ Новый метод: получить английский перевод
    private string GetEnglishFallback(string category, string key)
    {
        // Если у нас уже загружен английский — используем кэш
        if (localeCache.TryGetValue(SystemLanguage.English, out var enDict))
        {
            if (enDict != null && enDict.TryGetValue(category, out var enCategoryDict))
            {
                if (enCategoryDict != null && enCategoryDict.TryGetValue(key, out var enValue))
                    return enValue;
            }
        }
        
        // Если английский ещё не загружен в кэш — загружаем
        string code = GetLanguageCode(SystemLanguage.English); // "en"
        string path = $"Localization/{code}";
        TextAsset jsonAsset = Resources.Load<TextAsset>(path);

        if (jsonAsset != null)
        {
            try
            {
                var parsed = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonAsset.text)
                             ?? new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

                if (parsed.TryGetValue(category, out var enCategoryDict))
                {
                    if (enCategoryDict.TryGetValue(key, out var enValue) && !string.IsNullOrEmpty(enValue))
                        return enValue;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse English fallback: {ex.Message}");
            }
        }

        return null;
    }

    private SystemLanguage GetSavedLanguage()
    {
        return (SystemLanguage)PlayerPrefs.GetInt("Language", (int)SystemLanguage.English);
    }
    private void SaveLanguage(SystemLanguage lang)
    {
        PlayerPrefs.SetInt("Language", (int)lang);
    }
}
