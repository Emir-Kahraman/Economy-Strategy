using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UISceneLoadButton : MonoBehaviour
{
    UIWorldGenerateMenuController parentController;
    public void Initialize(UIWorldGenerateMenuController parent)
    {
        parentController = parent;
        gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        parentController.StartGame();
    }
}
