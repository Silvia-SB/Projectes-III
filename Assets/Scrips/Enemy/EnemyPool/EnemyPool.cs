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

public class EnemyPool: MonoBehaviour
{
    [Serializable]
    public struct EnemyInMemory
    {
        public EnemyType enemyType;
        public GameObject enemyGameObject;
        public int quantityInMemory;
    }
    
    public List<EnemyInMemory> poolSettings;
    public static EnemyPool Instance { get; private set; }
    private Dictionary<EnemyType, Queue<GameObject>> enemyPool;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        enemyPool = new Dictionary<EnemyType, Queue<GameObject>>();
        
        foreach (EnemyInMemory config in poolSettings)
        {
            Queue<GameObject> newQueue = new Queue<GameObject>();
            for (int i = 0; i < config.quantityInMemory; i++)
            {
                GameObject enemyInstance = Instantiate(config.enemyGameObject, transform);
                enemyInstance.SetActive(false);
                newQueue.Enqueue(enemyInstance);
            }
            enemyPool.Add(config.enemyType, newQueue);
        }
    }

    public GameObject GetEnemy(EnemyType enemyType)
    {
        if (enemyPool.ContainsKey(enemyType) && enemyPool[enemyType].Count > 0)
        {
            GameObject enemy = enemyPool[enemyType].Dequeue();
            return enemy;
        }
        return null;
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
