using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Header("������� ����������")]
    [SerializeField] private List<GameObject> managersPrefabs = new(); //�������� ������, EventBusManager ������ ���� ����� ������!.

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;            
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else Destroy(gameObject);
    }
    private void InitializeManagers()
    {
        for(int i = 0; i < managersPrefabs.Count; i++)
        {
            if (managersPrefabs[i] != null)
            {
                GameObject managerObj = Instantiate(managersPrefabs[i]);
            }
        }
        Debug.Log($"��� ��������� � ���������� {managersPrefabs.Count} ���� ������� �������");
    }
}
