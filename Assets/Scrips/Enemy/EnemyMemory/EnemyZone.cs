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
        public float spawnInterval;
        public int spawnCount;
        [HideInInspector] public float currentTimer; 
        [HideInInspector] public int totalSpawnedEnemies;

    }

    [SerializeField] private List<ZoneData> zoneData = new();
    [SerializeField] private float distanceToSpawn = 10f;
    private bool hasSpawned = false;
    private float spawnTimer;
    private Transform playerTransform;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = other.transform;
            hasSpawned = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hasSpawned = false;
        }
    }

    private void Update()
    {
        if (!hasSpawned) return;
        foreach (var data in zoneData)
        {
            data.currentTimer -= Time.deltaTime; 

            if (data.currentTimer <= 0f && data.totalSpawnedEnemies < data.quantity)
            {
                SpawnEnemies(data);
                data.currentTimer = data.spawnInterval; 
            }
        }
    }

    private void SpawnEnemies(ZoneData data)
    {
        if (Vector3.Distance(playerTransform.position, data.spawnPoint.position) >= distanceToSpawn && 
            IsPointOffScreen(data.spawnPoint.position))
        {
            for (int i = 0; i < data.spawnCount; i++)
            {
               
                if (data.totalSpawnedEnemies >= data.quantity) 
                {
                    break; 
                }

                GameObject enemy = EnemyPool.Instance.GetEnemy(data.enemyType);
    
                if (enemy != null)
                {
                    Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * 1f;
                    randomOffset.y = data.spawnPoint.position.y; 

                    enemy.transform.position = data.spawnPoint.position + randomOffset;
                    enemy.SetActive(true);

                    data.totalSpawnedEnemies++;
                }
            }
        }
    }
    
    private bool IsPointOffScreen(Vector3 point)
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(point);
        return viewPos.z < 0 || viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1;
    }
}
