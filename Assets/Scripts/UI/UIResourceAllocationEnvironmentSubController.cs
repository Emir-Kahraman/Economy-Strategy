using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class UIResourceAllocationEnvironmentSubController : MonoBehaviour, IUIWindow //После же упраления условий склада.
{
    [SerializeField] private GameObject targetWindow;
    [SerializeField] private TextMeshProUGUI conditionNameText;
    [SerializeField] private TextMeshProUGUI currentAmountResourceText;
    [SerializeField] private TextMeshProUGUI conditionAmountResourceText;
    [Space]
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject highlightPrefab;
    private GameObject highlightsParent;
    [Space]
    [SerializeField] private Color emptyCellColor = new();
    [SerializeField] private Color occupiedCellColor = new();
    [SerializeField] private Color otherOccupiedCellColor = new();

    private Tilemap resourceTilemap;
    private ProductionFactory targetFactory;
    private ProductionFactory.ProductionCondition targetCondition;
    private UIFactoryWindowController parentController;
    private Dictionary<Vector3Int, ProductionFactory> cellsInRadius = new();
    private Dictionary<Vector3Int, GameObject> highlights = new();

    public void Initialize()
    {
        InitializeParameters();
        InitializeObjects();
        InitializeButtons();
        CloseWindow();
    }
    private void InitializeParameters()
    {
        resourceTilemap = TilemapManager.Instance.GetTilemapType(TilemapType.Resource);
    }
    private void InitializeObjects()
    {
        highlightsParent = new GameObject("HighlightsFromResourceAllocation");
    }
    private void InitializeButtons()
    {
        closeButton.onClick.AddListener(CloseWindowRequest);
    }

    public void SetData(ProductionFactory factory, ProductionFactory.ProductionCondition condition, string conditionName, UIFactoryWindowController parent)
    {
        targetFactory = factory;
        targetCondition = condition;
        parentController = parent;
        conditionNameText.text = conditionName;

        UIAmountPanelUpdate();
    }
    private void UIAmountPanelUpdate()
    {
        currentAmountResourceText.text = targetFactory.GetAmountResourceInProduction(targetCondition.requiredResource).ToString();
        conditionAmountResourceText.text = "/ " + targetCondition.requiredAmount.ToString();
    }
    private void CellsHighlightsUpdate()
    {
        var cells = TilemapManager.Instance.GetCellsInRadius(targetFactory.transform.position, targetCondition.requiredResource, targetCondition.requiredTileRadius);
        foreach (var cell in cells)
        {
            cellsInRadius[cell] = TilemapManager.Instance.GetProductionFactoryInResourceCell(cell);

            if (cellsInRadius[cell] == null)
            {
                CreateHighlight(cell, emptyCellColor);
            }
            else if (cellsInRadius[cell] == targetFactory)
            {
                CreateHighlight(cell, occupiedCellColor);
            }
            else
            {
                CreateHighlight(cell, otherOccupiedCellColor);
            }
        }
    }

    private void Update()
    {
        if (GameModeManager.Instance.CurrentMode != GameModeManager.GameMode.Observation && EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cell = resourceTilemap.WorldToCell(mousePosition);

            if (!cellsInRadius.TryGetValue(cell, out ProductionFactory factory)) return;

            if (factory == null && targetFactory.GetAmountResourceInProduction(targetCondition.requiredResource) < targetCondition.requiredAmount) targetFactory.HandleCellOccupy(cell, targetCondition);
            else if (factory == targetFactory) targetFactory.HandleCellRelease(cell, targetCondition);
            else return;

            DeleteHighlight(cell);
            CellHighlightUpdate(cell);
            UIAmountPanelUpdate();
        }
    }

    private void CellHighlightUpdate(Vector3Int cell)
    {
        cellsInRadius[cell] = TilemapManager.Instance.GetProductionFactoryInResourceCell(cell);
        if (cellsInRadius[cell] == null) CreateHighlight(cell, emptyCellColor);
        else if (cellsInRadius[cell] == targetFactory) CreateHighlight(cell, occupiedCellColor);
        else CreateHighlight(cell, otherOccupiedCellColor);
    }

    private void CreateHighlight(Vector3Int cell, Color color)
    {
        Vector3 offset = new Vector3(0.5f, 0.5f, 0f);

        var highlight = Instantiate(highlightPrefab, cell + offset, Quaternion.identity, highlightsParent.transform);
        highlight.GetComponent<SpriteRenderer>().color = color;
        highlights[cell] = highlight;
    }

    private void DeleteHighlight(Vector3Int cell)
    {
        Destroy(highlights[cell]);
    }
    private void DeleteAllHighlights()
    {
        foreach (var cell in highlights)
        {
            Destroy(cell.Value); //Можно создать пул объектов
        }
        highlights.Clear();
    }

    private void CloseWindowRequest()
    {
        EventBusManager.Instance.WindowCloseRequested(this);
    }

    public void OpenWindow()
    {
        targetWindow.SetActive(true);
        CellsHighlightsUpdate();
    }
    public void CloseWindow()
    {
        if (targetFactory != null) targetFactory.SetPaused(false);
        targetWindow.SetActive(false);
        cellsInRadius.Clear();
        DeleteAllHighlights();
    }
}
