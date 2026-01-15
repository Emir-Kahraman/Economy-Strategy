using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public enum MenuType { None, MainMenu, PlaySelection, LevelSelection }
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private List<UIMenuBase> mainMenus = new();
    [Header("Level UIs")]
    [SerializeField] private List<GameObject> levelUIPanels = new();
    [SerializeField] private List<GameObject> levelUIControllers = new();

    private MenuType currentMenu;
    private IUIWindow currentWindow;

    private void Awake()
    {
        Initialize();
    }
    private void OnDestroy()
    {
        UninitializeEvents();
    }
    private void Initialize()
    {
        InitializeSingleton();
        InitializeEvents();
        InitializeMainMenu();
        InitializeUIMenus();
    }
    private void InitializeSingleton()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "UIManager";
    }        
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnSceneLoaded += SwitchScene;
        EventBusManager.Instance.OnMenuSwitch += MainMenuSwitch;
        EventBusManager.Instance.OnWindowOpenRequested += OpenWindow;
        EventBusManager.Instance.OnWindowCloseRequested += CloseWindow;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnSceneLoaded -= SwitchScene;
        EventBusManager.Instance.OnMenuSwitch -= MainMenuSwitch;
        EventBusManager.Instance.OnWindowOpenRequested -= OpenWindow;
        EventBusManager.Instance.OnWindowCloseRequested -= CloseWindow;
    }
    private void InitializeMainMenu()
    {
        foreach (var menu in mainMenus)
        {
            menu.gameObject.SetActive(false);
        }
        MainMenuSwitch(MenuType.MainMenu);
    }

    private GameObject GetMainMenu(MenuType requestedMenu)
    {
        if (requestedMenu == MenuType.None) return null;
        foreach (var menu in mainMenus)
        {
            if (menu.Type == requestedMenu)
                return menu.gameObject;
        }
        Debug.LogError("MenuType not found! " + requestedMenu);
        return null;
    }
    private void InitializeUIMenus()
    {
        for (int i = 0; i < levelUIControllers.Count; i++)
            levelUIControllers[i].gameObject.SetActive(false);

        for (int i = 0; i < levelUIPanels.Count; i++)
            levelUIPanels[i].gameObject.SetActive(false);
    }

    private void SwitchScene()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            ActivatedMainMenuUI();
            DeactivatedLevelUI();
        }
        else if (SceneManager.GetActiveScene().buildIndex > 1)
        {
            ActivatedLevelUI();
            DeactivatedMainMenuUI();
        }
    }
    private void ActivatedMainMenuUI()
    {
        currentMenu = MenuType.MainMenu;
        MainMenuSwitch(currentMenu);
    }
    private void DeactivatedMainMenuUI()
    {
        foreach (var menu in mainMenus)
        {
            menu.gameObject.SetActive(false);
        }
    }
    private void ActivatedLevelUI()
    {
        foreach (var uiController in levelUIControllers)
        {
            uiController.gameObject.SetActive(true);
            uiController.GetComponent<IUIWindow>().Initialize();
        }
        foreach (var uiPanel in levelUIPanels)
        {
            uiPanel.gameObject.SetActive(true);
        }
    }
    private void DeactivatedLevelUI()
    {
        foreach (var uiController in levelUIControllers)
        {
            uiController.gameObject.SetActive(false);
        }
        foreach (var uiPanel in levelUIPanels)
        {
            uiPanel.gameObject.SetActive(false);
        }
    }

    private void MainMenuSwitch(MenuType activatedMenu)
    {
        if (activatedMenu == currentMenu) return;
        CloseMenu();
        currentMenu = activatedMenu;
        OpenMenu();
    }
    private void CloseMenu()
    {
        var menuToClose = GetMainMenu(currentMenu);
        if (menuToClose == null) return;
        menuToClose.gameObject.SetActive(false);
    }
    private void OpenMenu()
    {
        var menuToOpen = GetMainMenu(currentMenu);
        if (menuToOpen == null) Debug.LogError("MenuType Selected NONE");
        menuToOpen.gameObject.SetActive(true);
    }

    public UIFactoryWindowController GetUIMenuComponent()
    {
        for (int i = 0; i < levelUIControllers.Count; i++)
        {
            if (levelUIControllers[i].TryGetComponent<UIFactoryWindowController>(out var factoryWindowController))
            {
                return factoryWindowController;
            }
        }
        return null;
    }

    private void OpenWindow(IUIWindow window)
    {
        if (currentWindow == window) return;
        else if(currentWindow != null) CloseWindow(currentWindow);

        window.OpenWindow();
        currentWindow = window;
    }
    private void CloseWindow(IUIWindow window)
    {
        if (currentWindow == null || currentWindow != window) return;

        window.CloseWindow();
        currentWindow = null;
    }
}
