using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public bool IsMusicEnabled { get; private set; } = true;
    public bool IsSoundEnabled { get; private set; } = true;

    private const string MusicPrefKey = "MusicEnabled";
    private const string SoundPrefKey = "SoundEnabled";

    private void Awake()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        InitializeSingleton();
        InitializeEvents();
        LoadSettings();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "SettingsManager";
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnMusicToggled += ToggleMusic;
        EventBusManager.Instance.OnSoundToggled += ToggleSound;
    }
    private void LoadSettings()
    {
        IsMusicEnabled = PlayerPrefs.GetInt(MusicPrefKey, 1) == 1;
        IsSoundEnabled = PlayerPrefs.GetInt(SoundPrefKey, 1) == 1;
    }

    public void ToggleMusic(bool isEnabled)
    {
        IsMusicEnabled = isEnabled;
        PlayerPrefs.SetInt(MusicPrefKey, isEnabled ? 1 : 0);
        EventBusManager.Instance.MusicToggled(isEnabled);
    }
    public void ToggleSound(bool isEnabled)
    {
        IsSoundEnabled = isEnabled;
        PlayerPrefs.SetInt(SoundPrefKey, isEnabled ? 1 : 0);
        EventBusManager.Instance.SoundToggled(isEnabled);
    }
}