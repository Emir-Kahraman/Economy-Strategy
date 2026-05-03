using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWorldGenerateMenuController : MonoBehaviour
{
    [Header("Data & Logic")]
    [SerializeField] private CurrentLevelRuntimeData runtimeData;
    [SerializeField] private UISceneLoadButton startButton;
    [SerializeField] private Slider worldSizeSlider;
    [Space, Header("World Parameters")]
    [SerializeField] private Slider startMoneySlider;
    [SerializeField] private Slider startPeriodSlider;
    [SerializeField] private LevelData levelData;
    [Space, Header("UI Text")]
    [SerializeField] private TextMeshProUGUI worldSizeText;
    [SerializeField] private TextMeshProUGUI startMoneyText;
    [SerializeField] private TextMeshProUGUI startPeriodText;
    [Space, Header("Localization")]
    [SerializeField] private string category = "ui";
    [SerializeField] private string keyOfWorldSizeSmall = "generate_world_size_small";
    [SerializeField] private string keyOfWorldSizeMedium = "generate_world_size_medium";
    [SerializeField] private string keyOfWorldSizeLarge = "generate_world_size_large";
    [SerializeField] private string keyOfStartMoneyLow = "generate_start_money_low";
    [SerializeField] private string keyOfStartMoneyMedium = "generate_start_money_medium";
    [SerializeField] private string keyOfStartMoneyHigh = "generate_start_money_high";

    private WorldGenerateParameters worldGenerateParameters = new();
    
    private void Awake()
    {
        Initialize();
    }
    private void Initialize()
    {
        InitializeButtons();
        InitializeUIText();
    }
    private void InitializeButtons()
    {
        startButton.Initialize(this);
    }
    private void InitializeUIText()
    {
        UpdateAllTextName();
        worldSizeSlider.onValueChanged.AddListener((value) => UpdateWorldSizeText());
        startMoneySlider.onValueChanged.AddListener((value) => UpdateStartMoneyText());
        startPeriodSlider.onValueChanged.AddListener((value) => UpdateStartPeriodText());
    }

    private void OnEnable()
    {
        UpdateAllTextName();
    }

    private void Update()
    {
        SetWorldParameters();
    }
    private void SetWorldParameters()
    {
        worldGenerateParameters.worldSizeIndex = (int)worldSizeSlider.value;
        worldGenerateParameters.startMoneyIndex = (int)startMoneySlider.value;
        worldGenerateParameters.startPeriodIndex = (int)startPeriodSlider.value;
    }

    public void StartGame()
    {
        runtimeData.worldGenerateParameters = worldGenerateParameters;
        runtimeData.worldSeed = 0;
        levelData.SetParameters(worldGenerateParameters.startMoneyIndex, worldGenerateParameters.startPeriodIndex);
        runtimeData.levelData = levelData;
        runtimeData.isLoadLevelFromSave = false;
        EventBusManager.Instance.SceneLoadRequest("Random_Level");
    }

    private void UpdateWorldSizeText()
    {
        if (worldSizeText != null)
            worldSizeText.text = $"{GetWorldSizeText((int)worldSizeSlider.value)}";
    }
    private void UpdateStartMoneyText()
    {
        if (startMoneyText != null)
            startMoneyText.text = $"{GetStartMoneyText((int)startMoneySlider.value)}";
    }
    private void UpdateStartPeriodText()
    {
        if (startPeriodText != null)
            startPeriodText.text = $"{GetStartPeriodText((int)startPeriodSlider.value)}";
    }

    private void UpdateAllTextName()
    {
        if (worldSizeText != null)
            worldSizeText.text = $"{GetWorldSizeText((int)worldSizeSlider.value)}";
        if (startMoneyText != null)
            startMoneyText.text = $"{GetStartMoneyText((int)startMoneySlider.value)}";
        if (startPeriodText != null)
            startPeriodText.text = $"{GetStartPeriodText((int)startPeriodSlider.value)}";
    }
    private string GetWorldSizeText(int index)
    {
        switch (index)
        {
            case 0: return LocalizationManager.Instance.GetText(category, keyOfWorldSizeSmall);
            case 1: return LocalizationManager.Instance.GetText(category, keyOfWorldSizeMedium);
            case 2: return LocalizationManager.Instance.GetText(category, keyOfWorldSizeLarge);
            default: return "Unknown";
        }
    }
    private string GetStartMoneyText(int index)
        {
            switch (index)
            {
                case 0: return LocalizationManager.Instance.GetText(category, keyOfStartMoneyLow);
                case 1: return LocalizationManager.Instance.GetText(category, keyOfStartMoneyMedium);
                case 2: return LocalizationManager.Instance.GetText(category, keyOfStartMoneyHigh);
                default: return "Unknown";
            }
    }
    private string GetStartPeriodText(int index)
    {
        switch (index)
        {
            case 0: return "I";
            case 1: return "II";
            case 2: return "III";
            case 3: return "IV";
            default: return "Unknown";
        }
    }
}
