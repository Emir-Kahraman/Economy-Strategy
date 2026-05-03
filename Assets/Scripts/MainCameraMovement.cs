using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainCameraMovement : MonoBehaviour
{
    [Header("Настройки управления")]
    [SerializeField] private float panSpeedMultiplier = 1.5f;
    [SerializeField, Range(0.9f, 0.99f)] private float inertia = 0.95f;  // коэффициент затухания
    [SerializeField] private float maxSpeed = 20f;   // максимальная скорость (мировые единицы/сек)
    [SerializeField] private float minVelocity = 0.01f; // порог остановки
    [SerializeField] private float edgeOffset = 2f;

    private Camera cam;
    private Vector3 lastMousePosition;
    private bool isPanning;
    private Vector2 velocity;

    private float mapMinX, mapMaxX, mapMinY, mapMaxY;
    private bool mapBoundsSet = false;
    private float minCamX, maxCamX, minCamY, maxCamY;

    private void Awake()
    {
        Initialize();
    }
    private void Initialize()
    {
        InitializeParameters();
    }
    private void InitializeParameters()
    {
        cam = GetComponent<Camera>();
    }
    public void SetPosition(Vector3 position)
    {
        transform.position = new Vector3(position.x, position.y, -10);
        velocity = Vector2.zero;
    }
    public void SetMapBounds(float newMapMinX, float newMapMaxX, float newMapMinY, float newMapMaxY)
    {
        mapMinX = newMapMinX;
        mapMaxX = newMapMaxX;
        mapMinY = newMapMinY;
        mapMaxY = newMapMaxY;
        mapBoundsSet = true;

        RecalculateCameraBounds();
        ClampPosition();
    }

    private void Update()
    {
        HandlePanInput();
        ApplyVelocity();
    }

    private void HandlePanInput()
    {
        if (Input.GetMouseButtonDown(2))
        {
            isPanning = true;
            lastMousePosition = Input.mousePosition;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Input.GetMouseButtonUp(2))
        {
            isPanning = false;
        }

        if (isPanning && Input.GetMouseButton(2))
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 delta = currentMousePosition - lastMousePosition;
            lastMousePosition = currentMousePosition;

            // Перевод пикселей в мировые единицы
            float worldPerPixel = cam.orthographicSize * 2f / Screen.height;
            float speedFactor = GetSpeedFactor(); // уже включает panSpeedMultiplier и размер камеры

            // Желаемая скорость (мировые единицы в секунду)
            Vector2 desiredVelocity = new Vector2(
                -delta.x * worldPerPixel * speedFactor,
                -delta.y * worldPerPixel * speedFactor
            ) / Time.deltaTime;

            // Устанавливаем скорость мгновенно (следование за мышью)
            velocity = desiredVelocity;

            // Ограничиваем максимальную скорость
            if (velocity.magnitude > maxSpeed)
                velocity = velocity.normalized * maxSpeed;
        }
        else
        {
            // Затухание инерции
            velocity *= inertia;
            if (velocity.magnitude < minVelocity)
                velocity = Vector2.zero;
        }
    }
    private void ApplyVelocity()
    {
        if (velocity == Vector2.zero) return;

        Vector3 move = new Vector3(velocity.x, velocity.y, 0) * Time.deltaTime;
        transform.position += move;
    }
    private float GetSpeedFactor()
    {
        return cam.orthographicSize * 0.01f * panSpeedMultiplier;
    }

    private void LateUpdate()
    {
        if (mapBoundsSet)
            ClampPosition();
    }
    private void RecalculateCameraBounds()
    {
        if (!mapBoundsSet) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // Допустимые границы с учётом отступа
        minCamX = mapMinX + halfWidth - edgeOffset;
        maxCamX = mapMaxX - halfWidth + edgeOffset;
        minCamY = mapMinY + halfHeight - edgeOffset;
        maxCamY = mapMaxY - halfHeight + edgeOffset;

        // Если карта слишком мала, ставим центр
        if (minCamX > maxCamX)
        {
            float centerX = (mapMinX + mapMaxX) / 2f;
            minCamX = maxCamX = centerX;
        }
        if (minCamY > maxCamY)
        {
            float centerY = (mapMinY + mapMaxY) / 2f;
            minCamY = maxCamY = centerY;
        }
    }
    private void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minCamX, maxCamX);
        pos.y = Mathf.Clamp(pos.y, minCamY, maxCamY);
        transform.position = pos;
    }
}