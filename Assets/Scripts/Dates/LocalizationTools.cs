#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public static class LocalizationTools //AI
{
    private const string LocalizationFolder = "Assets/Resources/Localization";
    private const string FlagsSubfolder = "flags";
    private const string DefaultMasterCode = "en";

    [MenuItem("Localization/Sync Locales (from master)")]
    public static void SyncLocalesMenu()
    {
        try
        {
            SyncLocales();
            AssetDatabase.Refresh();
            Debug.Log("Localization sync finished.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Localization sync failed: {ex}");
        }
    }

    [MenuItem("Localization/Validate Locales")]
    public static void ValidateLocalesMenu()
    {
        try
        {
            ValidateLocales();
            Debug.Log("Localization validation finished.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Localization validation failed: {ex}");
        }
    }

    [MenuItem("Localization/Import from CSV")]
    public static void ImportFromCsvMenu()
    {
        string csvPath = EditorUtility.OpenFilePanel("Select CSV file", "", "csv");
        if (string.IsNullOrEmpty(csvPath)) return;

        try
        {
            ImportFromCsv(csvPath);
            AssetDatabase.Refresh();
            Debug.Log("CSV import finished successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"CSV import failed: {ex}");
        }
    }

    public static void SyncLocales()
    {
        string masterPath = Path.Combine(LocalizationFolder, DefaultMasterCode + ".json");
        var jsonFiles = FindJsonFiles(LocalizationFolder);

        if (!jsonFiles.Any())
        {
            Debug.LogWarning($"No JSON localization files found in '{LocalizationFolder}'.");
            return;
        }

        if (!File.Exists(masterPath))
        {
            masterPath = jsonFiles.First();
        }

        var masterDict = LoadLocaleFile(masterPath);
        if (masterDict == null)
        {
            Debug.LogError($"Failed to parse master localization file: {masterPath}");
            return;
        }

        foreach (var path in jsonFiles)
        {
            if (path.Contains(Path.Combine(LocalizationFolder, FlagsSubfolder))) continue;
            if (string.Equals(path, masterPath, StringComparison.OrdinalIgnoreCase)) continue;

            var localeDict = LoadLocaleFile(path);
            if (localeDict == null)
            {
                localeDict = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            }

            bool modified = false;
            string bak = path + ".bak";

            foreach (var masterCategory in masterDict)
            {
                if (!localeDict.TryGetValue(masterCategory.Key, out var localeCategory))
                {
                    localeCategory = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    localeDict[masterCategory.Key] = localeCategory;
                    modified = true;
                }

                foreach (var kv in masterCategory.Value)
                {
                    if (!localeCategory.ContainsKey(kv.Key))
                    {
                        localeCategory[kv.Key] = kv.Value;
                        modified = true;
                    }
                }
            }

            if (modified)
            {
                if (!File.Exists(bak))
                {
                    File.Copy(path, bak, overwrite: true);
                }
                SaveLocaleFile(path, localeDict);
            }
        }
    }

    public static void ValidateLocales()
    {
        string masterPath = Path.Combine(LocalizationFolder, DefaultMasterCode + ".json");
        var jsonFiles = FindJsonFiles(LocalizationFolder);

        if (!jsonFiles.Any())
        {
            Debug.LogWarning($"No JSON localization files found in '{LocalizationFolder}'.");
            return;
        }

        if (!File.Exists(masterPath))
        {
            masterPath = jsonFiles.First();
        }

        var masterDict = LoadLocaleFile(masterPath);
        if (masterDict == null)
        {
            Debug.LogError($"Failed to parse master localization file: {masterPath}");
            return;
        }

        foreach (var path in jsonFiles)
        {
            if (path.Contains(Path.Combine(LocalizationFolder, FlagsSubfolder))) continue;
            if (string.Equals(path, masterPath, StringComparison.OrdinalIgnoreCase)) continue;

            var localeDict = LoadLocaleFile(path) ?? new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            var missing = new List<string>();
            var extra = new List<string>();

            foreach (var masterCategory in masterDict)
            {
                if (!localeDict.TryGetValue(masterCategory.Key, out var localeCategory))
                {
                    missing.Add($"Category '{masterCategory.Key}' (all keys)");
                    continue;
                }

                foreach (var kv in masterCategory.Value)
                {
                    if (!localeCategory.ContainsKey(kv.Key))
                        missing.Add($"Key '{masterCategory.Key}/{kv.Key}'");
                }
            }

            foreach (var localeCategory in localeDict)
            {
                if (!masterDict.ContainsKey(localeCategory.Key))
                {
                    extra.Add($"Category '{localeCategory.Key}' (not in master)");
                    continue;
                }

                foreach (var kv in localeCategory.Value)
                {
                    if (!masterDict[localeCategory.Key].ContainsKey(kv.Key))
                        extra.Add($"Key '{localeCategory.Key}/{kv.Key}'");
                }
            }

            if (missing.Count == 0 && extra.Count == 0)
            {
                Debug.Log($"[Validate] {Path.GetFileName(path)} OK");
            }
            else
            {
                Debug.LogWarning($"[Validate] Issues in {Path.GetFileName(path)}: Missing({missing.Count}), Extra({extra.Count})");
                foreach (var m in missing) Debug.LogWarning($"[Validate][MISSING] {Path.GetFileName(path)}: {m}");
                foreach (var e in extra) Debug.LogWarning($"[Validate][EXTRA]   {Path.GetFileName(path)}: {e}");
            }
        }
    }

    public static void ImportFromCsv(string csvPath)
    {
        // Читаем файл с правильной кодировкой (UTF-8 с BOM для Excel)
        var lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8);
        if (lines.Length < 2)
        {
            Debug.LogError("CSV must have header row and at least one data row.");
            return;
        }

        // ★ Автоматически определяем разделитель
        char delimiter = DetectDelimiter(lines[0]);
        Debug.Log($"Detected delimiter: '{delimiter}'");

        // Парсим заголовок
        var headers = ParseCsvLine(lines[0], delimiter);
        
        // Ищем индексы нужных колонок (case-insensitive)
        int categoryIdx = -1;
        int keyIdx = -1;

        for (int i = 0; i < headers.Count; i++)
        {
            if (headers[i].Equals("category", StringComparison.OrdinalIgnoreCase))
                categoryIdx = i;
            if (headers[i].Equals("key", StringComparison.OrdinalIgnoreCase))
                keyIdx = i;
        }

        if (categoryIdx < 0 || keyIdx < 0)
        {
            Debug.LogError($"CSV must have 'Category' and 'Key' columns. Found columns: {string.Join(", ", headers)}");
            return;
        }

        // Языки — остальные колонки
        var languages = new Dictionary<string, int>();
        for (int i = 0; i < headers.Count; i++)
        {
            if (i != categoryIdx && i != keyIdx)
            {
                languages[headers[i]] = i;
            }
        }

        if (languages.Count == 0)
        {
            Debug.LogError("CSV must have at least one language column (en, ru, etc.)");
            return;
        }

        // Инициализируем словари для каждого языка
        var langDicts = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        foreach (var lang in languages.Keys)
        {
            langDicts[lang] = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        }

        // Парсим данные
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var parts = ParseCsvLine(lines[i], delimiter);
            if (parts.Count < keyIdx + 1) continue;

            string category = parts[categoryIdx].Trim();
            string key = parts[keyIdx].Trim();

            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(key)) continue;

            // Добавляем в каждый язык
            foreach (var kvp in languages)
            {
                string lang = kvp.Key;
                int colIdx = kvp.Value;

                if (colIdx >= parts.Count) continue;

                string value = parts[colIdx].Trim();

                if (!langDicts[lang].ContainsKey(category))
                {
                    langDicts[lang][category] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                langDicts[lang][category][key] = value;
            }
        }

        // Сохраняем JSON-файлы
        foreach (var kvp in langDicts)
        {
            string lang = kvp.Key;
            var dict = kvp.Value;

            string path = Path.Combine(LocalizationFolder, $"{lang}.json");
            SaveLocaleFile(path, dict);
            Debug.Log($"Created/Updated: {path}");
        }
    }

    // ★ Определяет разделитель CSV (запятая или точка с запятой)
    private static char DetectDelimiter(string headerLine)
    {
        int commaCount = headerLine.Count(c => c == ',');
        int semicolonCount = headerLine.Count(c => c == ';');
        
        // Какой разделитель встречается чаще — тот и используем
        return semicolonCount > commaCount ? ';' : ',';
    }

    // ★ Обновленный парсер с поддержкой разного разделителя
    private static List<string> ParseCsvLine(string line, char delimiter = ',')
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == delimiter && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result;
    }

    private static IEnumerable<string> FindJsonFiles(string folder)
    {
        if (!Directory.Exists(folder)) return Enumerable.Empty<string>();
        return Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly)
                        .Where(f => !f.Contains(Path.Combine(folder, FlagsSubfolder)));
    }

    private static Dictionary<string, Dictionary<string, string>> LoadLocaleFile(string path)
    {
        try
        {
            var text = File.ReadAllText(path);
            var parsed = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(text);
            return parsed ?? new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load/parse JSON '{path}': {ex.Message}");
            return null;
        }
    }

    private static void SaveLocaleFile(string path, Dictionary<string, Dictionary<string, string>> data)
    {
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(path, json);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }
}
#endif