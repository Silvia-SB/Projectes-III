using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyZone : MonoBehaviour
{
    [Serializable]
    public class ZoneData
    {
        public Transform spawnPoint;
        public EnemyType enemyType;
        public int quantity;
    }

    [SerializeField] private List<ZoneData> zoneData = new();
    private bool hasSpawned = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasSpawned)
        {
            hasSpawned = true;
            SpawnEnemies();
        }
    }

    private void SpawnEnemies()
    {
        foreach (var data in zoneData)
        {
            for (int i = 0; i < data.quantity; i++)
            {
                GameObject enemy = EnemyPool.Instance.GetEnemy(data.enemyType);
            
                if (enemy != null)
                {
                    Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * 1.5f;
                    randomOffset.y = data.spawnPoint.position.y; 

                    enemy.transform.position = data.spawnPoint.position + randomOffset;
                    enemy.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"¡El Pool se ha quedado sin enemigos del tipo {data.enemyType}!");
                }
            }
        }
    }
}
