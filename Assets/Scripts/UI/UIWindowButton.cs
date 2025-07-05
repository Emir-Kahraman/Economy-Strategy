using UnityEngine;
using UnityEngine.UI;

public class UIWindowButton : MonoBehaviour
{
    [SerializeField] private MonoBehaviour targetWindow;

    private IUIWindow windowInterface;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        windowInterface = targetWindow as IUIWindow;

        if (windowInterface == null)
        {
            button.interactable = false;
            return;
        }

        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick() => EventBusManager.Instance.WindowOpenRequested(windowInterface);
}
