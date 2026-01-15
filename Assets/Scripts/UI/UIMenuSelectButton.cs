using System;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuSelectButton : MonoBehaviour
{
    [SerializeField] private UIMenuBase targetMenu;

    private Button button;
    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick() => EventBusManager.Instance.MenuSwitch(targetMenu.Type);
}
