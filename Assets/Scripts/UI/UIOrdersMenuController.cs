using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;
using System;

public class UIOrdersMenuController : MonoBehaviour, IUIWindow
{
    [SerializeField] private GameObject targetWindow;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform activeOrdersContainer;
    [SerializeField] private Transform acceptedOrdersContainer;
    [SerializeField] private GameObject orderElementPrefab;
    [SerializeField] private Button menuButton;

    private Dictionary<string, UIOrderElement> activeOrders = new();
    private Dictionary<string, UIOrderElement> acceptedOrders = new();

    public List<UIOrderElement> GetAcceptedOrders()
    {
        List<UIOrderElement> orders = new List<UIOrderElement>(acceptedOrders.Values);
        return orders;
    }
    public List<UIOrderElement> GetActiveOrders()
    {
        List<UIOrderElement> orders = new List<UIOrderElement>(activeOrders.Values);
        return orders;
    }
    public void Initialize()
    {
        InitializeButtons();
        ClearOrders();
        InitializeEvents();
        CloseWindow();
    }
    public void Uninitialize()
    {
        UninitializeButtons();
        UninitializeEvents();
        ClearOrders();
    }
    private void UninitializeButtons()
    {
        closeButton.onClick.RemoveAllListeners();
    }
    private void InitializeButtons()
    {
        closeButton.onClick.AddListener(CloseWindowRequest);
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnOrdersLoadedFromSave += LoadOrdersFromSave;
        EventBusManager.Instance.OnOrderCreated += CreateOrder;
        EventBusManager.Instance.OnOrderAccept += OrderAccepted;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnOrderCreated -= CreateOrder;
        EventBusManager.Instance.OnOrdersLoadedFromSave -= LoadOrdersFromSave;
        EventBusManager.Instance.OnOrderAccept -= OrderAccepted;
    }
    private void ClearOrders()
    {
        foreach (var order in activeOrders.Values)
        {
            Destroy(order.gameObject);
        }
        activeOrders.Clear();
        foreach (var order in acceptedOrders.Values)
        {
            Destroy(order.gameObject);
        }
        acceptedOrders.Clear();
    }

    private void LoadOrdersFromSave(List<OrderData> orders, bool isOrdersAccepted)
    {
        foreach (OrderData orderData in orders)
        {
            if (isOrdersAccepted) CreateAcceptedOrder(orderData);
            else CreateOrder(orderData);
        }
    }
    private void CreateAcceptedOrder(OrderData orderData)
    {
        UIOrderElement order = Instantiate(orderElementPrefab, acceptedOrdersContainer).GetComponent<UIOrderElement>();
        acceptedOrders[orderData.id] = order;
        order.GetComponent<UIOrderElement>().Initialize(orderData, this);
        order.GetComponent<UIOrderElement>().OrderAccepted();
    }
    private void CreateOrder(OrderData orderData)
    {
        UIOrderElement order = Instantiate(orderElementPrefab, activeOrdersContainer).GetComponent<UIOrderElement>();
        activeOrders[orderData.id] = order;
        order.GetComponent<UIOrderElement>().Initialize(orderData, this);
    }

    private void OrderAccepted(string id)
    {
        acceptedOrders[id] = activeOrders[id];
        activeOrders.Remove(id);
        acceptedOrders[id].transform.SetParent(acceptedOrdersContainer.transform);
        EventBusManager.Instance.OrderAccepted();
    }
    public void OrderDeleted(string id, bool isAcceptedOrder)
    {
        if (isAcceptedOrder)
        {
            Destroy(acceptedOrders[id].gameObject);
            acceptedOrders.Remove(id);
        }
        else
        {
            Destroy(activeOrders[id].gameObject);
            activeOrders.Remove(id);
        }
        EventBusManager.Instance.OrderExpired(isAcceptedOrder);
    }
    private void Update()
    {
        OrdersTimeUpdate(activeOrders);
        OrdersTimeUpdate(acceptedOrders);
    }
    private void OrdersTimeUpdate(Dictionary<string, UIOrderElement> orders)
    {
        float deltaTime = Time.deltaTime;
        foreach (var order in orders.Values.ToList())
        {
            order.UpdateTimer(deltaTime);
        }
    }
    private void CloseWindowRequest()
    {
        EventBusManager.Instance.WindowCloseRequested(this);
    }
    public void OpenWindow()
    {
        targetWindow.SetActive(true);
        menuButton.gameObject.SetActive(false);
    }
    public void CloseWindow()
    {
        targetWindow.SetActive(false);
        menuButton.gameObject.SetActive(true);
    }
}
