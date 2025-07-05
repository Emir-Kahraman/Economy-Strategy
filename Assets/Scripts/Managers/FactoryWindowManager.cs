using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.EventSystems;

public class FactoryWindowManager : MonoBehaviour
{
    public static FactoryWindowManager Instance;

    private UIFactoryWindowController factoryWindowController;

    private void Awake()
    {
        InitializeSingleton();
        InitializeEvents();
    }
    private void OnDestroy()
    {
        UnitializeEvents();
    }
    private void InitializeSingleton()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "FactoryWindowManager";
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnProductionFactoryRequested += HandleFactoryWindowRequest;
    }
    private void UnitializeEvents()
    {
        EventBusManager.Instance.OnProductionFactoryRequested -= HandleFactoryWindowRequest;
    }

    private void Update()
    {
        if (GameModeManager.Instance.CurrentMode != GameModeManager.GameMode.Observation || EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            ProductionFactory factory = TilemapManager.Instance.TryGetProductionFactory(mousePosition);
            
            if (factory != null)
            {
                EventBusManager.Instance.ProductionFactoryRequested(factory);
            }
        }
    }

    private void HandleFactoryWindowRequest(ProductionFactory factory)
    {
        if (factoryWindowController == null)
        {
            factoryWindowController = UIManager.Instance.GetUIMenuComponent();

            if (factoryWindowController == null) return;
        }

        factoryWindowController.SetFactory(factory);
        EventBusManager.Instance.WindowOpenRequested(factoryWindowController);
    }
}
