using System;
using System.Collections;
using JetBrains.Annotations;
using NUnit.Framework.Constraints;
using UnityEngine;

public class WorkerSystemManager : MonoBehaviour
{
    public static WorkerSystemManager Instance;

    [SerializeField] private int baseWorkerLimit;
    [Header("Unemployment Impact")]
    [SerializeField] private float highEmploymentBoost;
    [SerializeField] private float lowUnemploymentBoost;
    [SerializeField] private float mediumUnemploymentPenalty;
    [SerializeField] private float highUnemploymentPenalty;
    [Header("Homelessness Impact")]
    [SerializeField] private float homelessnessImpactFactor;
    [Header("Satisfaction Impact")]
    [SerializeField] private int highestSatisfaction;
    [SerializeField] private int highSatisfaction;
    [SerializeField] private int mediumSatisfaction;
    [SerializeField] private int lowSatisfaction;
    [SerializeField] private int lowestSatisfaction;

    private bool bankruptcy = false;

    private int workerLimit;
    private int workerCount;
    private float workerReplenishmentTime = 2.5f;
    private int unemployedWorkers;

    private float satisfactionUpdateCooldown = 1f;
    private float maxSatisfaction = 5;
    private float minSatisfaction = 1;
    private float satisfactionLevel = 3;

    private IEnumerator workforceRecruitmentRoutine;
    private IEnumerator satisfactionUpdateRoutine;

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
        InitializeParameters();
        InitializeEvents();
    }
    private void InitializeSingleton()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        gameObject.name = "WorkerSystemManager";
    }
    private void InitializeParameters()
    {
        workerLimit = baseWorkerLimit;
        workerCount = workerLimit;
        unemployedWorkers = workerCount;

        workforceRecruitmentRoutine = WorkforceRecruitmentRoutine();
        satisfactionUpdateRoutine = SatisfactionUpdateRoutine();
    }
    private void InitializeEvents()
    {
        EventBusManager.Instance.OnBankruptcy += Bankruptcy;
        EventBusManager.Instance.OnDissolutionWorkers += DissolutionWorkers;
        EventBusManager.Instance.OnWorkerHousingBuilt += UpdateWorkerLimit;
    }
    private void UninitializeEvents()
    {
        EventBusManager.Instance.OnBankruptcy -= Bankruptcy;
        EventBusManager.Instance.OnDissolutionWorkers -= DissolutionWorkers;
        EventBusManager.Instance.OnWorkerHousingBuilt -= UpdateWorkerLimit;
    }

    private void Start()
    {
        InvokeStartEvents();
        StartCoroutine(workforceRecruitmentRoutine);
        StartCoroutine(satisfactionUpdateRoutine);
    }
    private void InvokeStartEvents()
    {
        EventBusManager.Instance.WorkerLimitUpdate(workerLimit);
        EventBusManager.Instance.WorkerCountUpdate(workerCount);
        EventBusManager.Instance.UnemploymentWorkerCountUpdate(unemployedWorkers);
    }

    private IEnumerator WorkforceRecruitmentRoutine()
    {
        while (!bankruptcy)
        {
            if (workerCount < workerLimit)
            {
                yield return new WaitForSeconds(workerReplenishmentTime);
                int newWorker = CalculateNewWorkers();
                workerCount = Mathf.Clamp(workerCount + newWorker, 0, workerLimit);
                EventBusManager.Instance.WorkerCountUpdate(workerCount);
            }
            else
            {
                yield return null;
            }
        }
    }
    private int CalculateNewWorkers()
    {
        return satisfactionLevel switch
        {
            >= 5f => highestSatisfaction,
            >= 4f => highSatisfaction,
            >= 3f => mediumSatisfaction,
            >= 2f => lowSatisfaction,
            _ => lowestSatisfaction,
        };
    }
    private IEnumerator SatisfactionUpdateRoutine()
    {
        yield return new WaitForSeconds(satisfactionUpdateCooldown);

        while (!bankruptcy)
        {
            float unemploymentEffect = CalculateUnemploymentEffect();
            float homelessnessEffect = CalculateHomelessnessEffect();
            satisfactionLevel += (unemploymentEffect + homelessnessEffect) * Time.deltaTime;
            Mathf.Clamp(satisfactionLevel, minSatisfaction, maxSatisfaction);
            EventBusManager.Instance.SatisfactionLevelUpdate(satisfactionLevel);
            EventBusManager.Instance.SatisfactionModifierUpdate(unemploymentEffect + homelessnessEffect);

            yield return null;
        }
    }
    private float CalculateUnemploymentEffect()
    {
        float unemploymentRate = unemployedWorkers / (float)workerCount;
        if (unemploymentRate == 0) return highEmploymentBoost;
        else if (unemploymentRate <= 0.1f) return lowUnemploymentBoost;
        else if (unemploymentRate <= 0.3f) return mediumUnemploymentPenalty;
        else return highUnemploymentPenalty;
    }
    private float CalculateHomelessnessEffect()
    {
        int homeless = Mathf.Max(workerCount - workerLimit, 0);
        float homelessnessRate = homeless / (float)workerCount * 100f;

        return -homelessnessRate * homelessnessImpactFactor;
    }

    private void Bankruptcy()
    {
        bankruptcy = true;
    }

    public int GetFreeWorkers(int count)
    {
        int value = Mathf.Min(unemployedWorkers, count);
        unemployedWorkers -= value;
        EventBusManager.Instance.UnemploymentWorkerCountUpdate(unemployedWorkers);
        return value;
    }
    private void DissolutionWorkers(int count)
    {
        unemployedWorkers += count;
        EventBusManager.Instance.UnemploymentWorkerCountUpdate(unemployedWorkers);
    }

    private void UpdateWorkerLimit(int limit)
    {
        workerLimit += limit;
        EventBusManager.Instance.WorkerLimitUpdate(workerLimit);
    }
}
