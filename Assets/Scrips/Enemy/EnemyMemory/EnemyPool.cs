using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public enum EnemyType
{
    Caballero,
    Desatado,
    Marchito,
    Medico,
    Cuervo
}

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }
    private Dictionary<EnemyType, Queue<GameObject>> enemyPool;
    private List<(EnemyType enemyType, GameObject enemy)> activeEnemies = new List<(EnemyType, GameObject)>();
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        enemyPool = new Dictionary<EnemyType, Queue<GameObject>>();
    }

    private async void Start()
    {
        await PreloadEnemiesAsync();
    }

    private async Task PreloadEnemiesAsync()
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
                await Task.Yield();
            }
            
            enemyPool.Add(blueprint.enemyType, newQueue);
        }
    }

    public GameObject GetEnemy(EnemyType enemyType)
    {
        GameObject enemy;

        if (enemyPool.ContainsKey(enemyType) && enemyPool[enemyType].Count > 0)
        {
            enemy = enemyPool[enemyType].Dequeue();
        }
        else
        {
            enemy = EnemyFactory.Instance.CreateEnemy(enemyType);
            enemy.transform.SetParent(this.transform);
        }

        activeEnemies.Add((enemyType, enemy));
        return enemy;
    }

    public void ReturnEnemyToPool(EnemyType enemyType, GameObject enemyToReturn)
    {
        if (enemyToReturn == null) return;

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i].enemy == enemyToReturn)
            {
                activeEnemies.RemoveAt(i);
                break;
            }
        }

        enemyToReturn.SetActive(false);

        if (!enemyPool.ContainsKey(enemyType))
        {
            enemyPool.Add(enemyType, new Queue<GameObject>());
        }

        enemyPool[enemyType].Enqueue(enemyToReturn);
    }

    public void ReturnAllEnemiesToPool()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            EnemyType enemyType = activeEnemies[i].enemyType;
            GameObject enemy = activeEnemies[i].enemy;

            if (enemy == null)
            {
                activeEnemies.RemoveAt(i);
                continue;
            }

            enemy.SetActive(false);
            enemy.transform.SetParent(this.transform);

            if (!enemyPool.ContainsKey(enemyType))
            {
                enemyPool.Add(enemyType, new Queue<GameObject>());
            }

            enemyPool[enemyType].Enqueue(enemy);

            activeEnemies.RemoveAt(i);
        }
    }
}