using UnityEngine;
using UnityEngine.UI;

public class GameProgressReset : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(ResetProgress);
    }
    private void ResetProgress() => EventBusManager.Instance.ResetGameProgress();
}
