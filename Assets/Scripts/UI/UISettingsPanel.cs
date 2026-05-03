using UnityEngine;
using UnityEngine.UI;

public class UISettingsPanel : MonoBehaviour
{
    [Header("Toggles")]
    [SerializeField] private Toggle musicToggleToggle;
    [SerializeField] private Toggle soundToggleToggle;
    [Header("Sprites")]
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        musicToggleToggle.onValueChanged.AddListener(MusicToggleChanged);
        soundToggleToggle.onValueChanged.AddListener(SoundToggleChanged);
    }
    private void MusicToggleChanged(bool isEnabled)
    {
        if (isEnabled) musicToggleToggle.image.sprite = musicOnSprite;
        else musicToggleToggle.image.sprite = musicOffSprite;

        EventBusManager.Instance.MusicToggled(isEnabled);
    }
    private void SoundToggleChanged(bool isEnabled)
    {
        if (isEnabled) soundToggleToggle.image.sprite = soundOnSprite;
        else soundToggleToggle.image.sprite = soundOffSprite;

        EventBusManager.Instance.SoundToggled(isEnabled);
    }
}
