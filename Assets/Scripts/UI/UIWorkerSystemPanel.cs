using System;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class UIWorkerSystemPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI workerCountText;
    [SerializeField] private TextMeshProUGUI unemploymentWorkerCountText;
    [SerializeField] private TextMeshProUGUI satisfactionLevelText;
    [SerializeField] private TextMeshProUGUI satisfactionModifierText;

    private int workerCount;
    private int workerLimit;
    private int freeWorkerCount;

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
        InitializeEvents();
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnWorkerLimitUpdate += WorkerLimitUpdate;
        EventBusManager.Instance.OnWorkerCountUpdate += WorkerCountUpdate;
        EventBusManager.Instance.OnUnemploymentWorkerCountUpdate += UnemploymentWorkerCountUpdate;
        EventBusManager.Instance.OnSatisfactionLevelUpdate += SatisfactionLevelUpdate;
        EventBusManager.Instance.OnSatisfactionModifierUpdate += SatisfactionModifierUpdate;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnWorkerLimitUpdate -= WorkerLimitUpdate;
        EventBusManager.Instance.OnWorkerCountUpdate -= WorkerCountUpdate;
        EventBusManager.Instance.OnUnemploymentWorkerCountUpdate -= UnemploymentWorkerCountUpdate;
        EventBusManager.Instance.OnSatisfactionLevelUpdate -= SatisfactionLevelUpdate;
        EventBusManager.Instance.OnSatisfactionModifierUpdate -= SatisfactionModifierUpdate;
    }

    private void WorkerLimitUpdate(int limit)
    {
        workerLimit = limit;
        UpdateUIElements();
    }
    private void WorkerCountUpdate(int count)
    {
        workerCount = count;
        UpdateUIElements();
    }
    private void UnemploymentWorkerCountUpdate(int count)
    {
        freeWorkerCount = count;
        UpdateUIElements();
    }
    private void SatisfactionLevelUpdate(float value)
    {
        satisfactionLevelText.text = Mathf.FloorToInt(value).ToString();
    }
    private void SatisfactionModifierUpdate(float modifier)
    {
        satisfactionModifierText.text = modifier.ToString();
    }
    private void UpdateUIElements()
    {
        workerCountText.text = $"{workerCount} / {workerLimit}";
        unemploymentWorkerCountText.text = freeWorkerCount.ToString();
    }
}
