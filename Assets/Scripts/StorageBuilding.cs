using UnityEngine;

public class StorageBuilding : MonoBehaviour
{
    [SerializeField] private float capacity;

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
}
