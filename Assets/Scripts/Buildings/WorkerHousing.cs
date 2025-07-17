using System;
using System.Collections;
using UnityEngine;

public class WorkerHousing : MonoBehaviour
{
    [SerializeField] private int capacity;
    [SerializeField] private int serviceCost;
    private float serviceTime = 10f;

    private IEnumerator serviceTimeUpdate;

    private void Awake()
    {
        Built();
    }
    private void OnDestroy()
    {
        Destroyed();
    }
    private void Built()
    {
        EventBusManager.Instance.WorkerHousingBuilt(capacity);
    }
    private void Destroyed()
    {
        EventBusManager.Instance.WorkerHousingBuilt(-capacity);
        StopCoroutine(serviceTimeUpdate);
    }

    private void Start()
    {
        serviceTimeUpdate = ServiceTimeUpdate();

        StartCoroutine(serviceTimeUpdate);
    }

    private IEnumerator ServiceTimeUpdate()
    {
        yield return new WaitForSeconds(serviceTime);
        CurrencyManager.Instance.SpendMoney(serviceCost);
    }
}
