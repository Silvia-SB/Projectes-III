using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Caballero,
    Desatado,
    Marchito,
    Medico
}

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }
    private Dictionary<EnemyType, Queue<GameObject>> enemyPool;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        enemyPool = new Dictionary<EnemyType, Queue<GameObject>>();
    }

    private void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        foreach (var blueprint in EnemyFactory.Instance.enemyBlueprints)
        {
            Queue<GameObject> newQueue = new Queue<GameObject>();
            
            for (int i = 0; i < blueprint.initialQuantity; i++)
            {
                GameObject enemyInstance = EnemyFactory.Instance.CreateEnemy(blueprint.enemyType);
                
                enemyInstance.transform.SetParent(this.transform); 
                enemyInstance.SetActive(false);
                newQueue.Enqueue(enemyInstance);
            }
            
            enemyPool.Add(blueprint.enemyType, newQueue);
        }
    }

    public GameObject GetEnemy(EnemyType enemyType)
    {
        if (enemyPool.ContainsKey(enemyType) && enemyPool[enemyType].Count > 0)
        {
            return enemyPool[enemyType].Dequeue();
        }
        
        GameObject newEnemy = EnemyFactory.Instance.CreateEnemy(enemyType);
        newEnemy.transform.SetParent(this.transform);
        
        return newEnemy;
    }

    public void ReturnEnemyToPool(EnemyType enemyType, GameObject enemyToReturn)
    {
        enemyToReturn.SetActive(false);
        
        if (!enemyPool.ContainsKey(enemyType))
        {
            enemyPool.Add(enemyType, new Queue<GameObject>());
        }
        
        enemyPool[enemyType].Enqueue(enemyToReturn);
    }
}