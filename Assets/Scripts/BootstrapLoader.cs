using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private GameObject gameManagerPrefab;
    private void Awake()
    {
        if (!GameManager.Exists)
        {
            Instantiate(gameManagerPrefab);
        }
        
        EventBusManager.Instance.SceneLoadRequest("MainMenu");
    }
}
