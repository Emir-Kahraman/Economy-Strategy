using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Header("Префабы Менеджеров")]
    [SerializeField] private List<GameObject> managersPrefabs = new();

    public int money;
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

    private void Start()
    {
        BuildMenuController.Instance.UpdateUI();
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
        Debug.Log($"Все менеджеры в количестве {managersPrefabs.Count} были успешно созданы");
    }

    public void AddMoney(int value)
    {
        money += value;
        BuildMenuController.Instance.UpdateUI();
    }
}
