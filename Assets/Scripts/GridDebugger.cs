#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Grid))]
public class GridDebugger : MonoBehaviour
{
    public Color originCellColor = new Color(1f, 0.3f, 0.3f, 0.4f);  // Полупрозрачный красный для заливки
    public Color originBorderColor = new Color(1f, 0.1f, 0.1f, 1f);  // Ярко-красная граница
    public bool showCoordinates = true;

    private Grid grid;

    private void OnValidate()
    {
        grid = GetComponent<Grid>();
    }

    private void OnDrawGizmos()
    {
        if (grid == null) return;

        // 1. Получаем мировые координаты ЦЕНТРА клетки (0,0)
        Vector3 cellCenter = grid.CellToWorld(Vector3Int.zero);
        cellCenter += new Vector3(0.5f, 0.5f);

        // 2. Рассчитываем размеры ПОЛНОЙ клетки с учётом ячеек грида
        Vector2 cellSize = new Vector2(grid.cellSize.x, grid.cellSize.y);
        Vector3 cellHalfSize = new Vector3(cellSize.x * 0.5f, cellSize.y * 0.5f, 0f);

        // 3. Вычисляем углы клетки для отрисовки
        Vector3 bottomLeft = cellCenter - cellHalfSize;
        Vector3 topRight = cellCenter + cellHalfSize;

        // 4. Рисуем заливку клетки (0,0)
        Handles.color = originCellColor;
        Handles.DrawSolidRectangleWithOutline(
            new[] { bottomLeft, new Vector3(topRight.x, bottomLeft.y), topRight, new Vector3(bottomLeft.x, topRight.y) },
            originCellColor,
            Color.clear
        );

        // 5. Рисуем яркую границу клетки (0,0)
        Handles.color = originBorderColor;
        Handles.DrawWireCube(cellCenter, cellSize);

        // 6. Добавляем текст "0,0" в центре клетки
        if (showCoordinates)
        {
            Handles.color = Color.white;
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            Handles.Label(cellCenter + Vector3.forward * -1f, "0,0", style);
        }
    }
}
#endif