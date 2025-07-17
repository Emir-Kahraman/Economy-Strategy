using UnityEngine;

public class StorageBuilding : MonoBehaviour
{
    [SerializeField] private float capacity;
    [SerializeField] private int serviceCost;
    private float serviceTime = 10f;
    public float GetCapacity() => capacity;

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
        EventBusManager.Instance.StorageBuilt(capacity);
    }
    private void Destroyed()
    {
        EventBusManager.Instance.StorageBuilt(-capacity);
    }
    private void Update()
    {
        ServiceUpdate();
    }
    private void ServiceUpdate()
    {
        serviceTime -= Time.deltaTime;
        if (serviceTime <= 0)
        {
            serviceTime = 10f;
            CurrencyManager.Instance.SpendMoney(serviceCost);
        }
    }
}
