using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UISceneLoadButton : MonoBehaviour
{
    private void Awake()
    {
        Initialize();
    }
    private void Initialize()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        EventBusManager.Instance.SceneLoadRequest("Level Test");
    }
}
