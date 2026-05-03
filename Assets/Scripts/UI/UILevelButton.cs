using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILevelButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button button;

    private LevelData data;
    private UILevelSelectionController parentController;

    public LevelData Data => data;

    public void Initialize(LevelData levelData, UILevelSelectionController parent)
    {
        data = levelData;
        levelText.text = levelData.index.ToString();
        button.interactable = levelData.isUnlocked;
        parentController = parent;
        button.onClick.AddListener(() => parentController.LoadLevelWithData(data));
    }
    public void InteractiveStatus(bool status, Color statusColor)
    {
        button.interactable = status;
        button.gameObject.GetComponent<Image>().color = statusColor;
    }
}
