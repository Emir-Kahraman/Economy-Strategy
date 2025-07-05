using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private List<GameObject> uiControllers = new();

    private IUIWindow currentWindow;

    private void Awake()
    {
        InitializeSingleton();
        Initialize();
    }
    private void OnDestroy()
    {
        UninitializeEvents();
    }

    private void InitializeSingleton()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "UIManager";
    }
    private void Initialize()
    {
        InitializeUIMenus();
        InitializeEvents();
    }
    private void InitializeUIMenus()
    {
        for(int i = 0;  i < uiControllers.Count; i++) 
            uiControllers[i].GetComponent<IUIWindow>().Initialize();
    }
    
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnWindowOpenRequested += OpenWindow;
        EventBusManager.Instance.OnWindowCloseRequested += CloseWindow;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnWindowOpenRequested -= OpenWindow;
        EventBusManager.Instance.OnWindowCloseRequested -= CloseWindow;
    }

    public UIFactoryWindowController GetUIMenuComponent()
    {
        for (int i = 0; i < uiControllers.Count; i++)
        {
            if (uiControllers[i].TryGetComponent<UIFactoryWindowController>(out var factoryWindowController))
            {
                return factoryWindowController;
            }
        }
        return null;
    }

    public void OpenWindow(IUIWindow window)
    {
        if (currentWindow == window) return;
        else if(currentWindow != null) CloseWindow(currentWindow);

        window.OpenWindow();
        currentWindow = window;
    }
    public void CloseWindow(IUIWindow window)
    {
        if (currentWindow == null || currentWindow != window) return;

        window.CloseWindow();
        currentWindow = null;
    }
}
