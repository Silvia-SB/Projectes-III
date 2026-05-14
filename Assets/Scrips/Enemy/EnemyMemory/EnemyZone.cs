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

    [Header("Spawn Settings")]
    [SerializeField] private float distanceToSpawn = 10f;

    [Header("Visibility Settings")]
    [SerializeField] private LayerMask obstacleMask;

    private bool hasSpawned = false;
    private Transform playerTransform;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

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
        if (playerTransform == null) return;

        foreach (var data in zoneData)
        {
            if (data.totalSpawnedEnemies >= data.quantity)
                continue;

            data.currentTimer -= Time.deltaTime;

            if (data.currentTimer <= 0f)
            {
                bool spawned = SpawnEnemies(data);

                if (spawned)
                {
                    data.currentTimer = data.spawnInterval;
                }
            }
        }
    }

    private bool SpawnEnemies(ZoneData data)
    {
        if (data.spawnPoint == null) return false;
        if (data.spawnCount <= 0) return false;
        if (data.quantity <= 0) return false;

        bool isFarEnough =
            Vector3.Distance(playerTransform.position, data.spawnPoint.position) >= distanceToSpawn;

        bool isVisibleToCamera =
            IsPointVisibleToCamera(data.spawnPoint.position);

        bool canSpawn =
            isFarEnough && !isVisibleToCamera;

        if (!canSpawn)
            return false;

        bool spawnedAny = false;

        for (int i = 0; i < data.spawnCount; i++)
        {
            if (data.totalSpawnedEnemies >= data.quantity)
                break;

            GameObject enemy = EnemyPool.Instance.GetEnemy(data.enemyType);

            if (enemy != null)
            {
                Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * 0.5f;
                randomOffset.y = 0f;

                enemy.transform.position = data.spawnPoint.position + randomOffset;
                enemy.SetActive(true);

                data.totalSpawnedEnemies++;
                spawnedAny = true;
            }
        }

        return spawnedAny;
    }

    private bool IsPointVisibleToCamera(Vector3 point)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
                return false;
        }

        Vector3 viewPos = mainCamera.WorldToViewportPoint(point);

        bool isInFrontOfCamera = viewPos.z > 0f;
        bool isInsideScreen =
            viewPos.x >= 0f && viewPos.x <= 1f &&
            viewPos.y >= 0f && viewPos.y <= 1f;

        if (!isInFrontOfCamera || !isInsideScreen)
            return false;

        Vector3 direction = point - mainCamera.transform.position;
        float distance = direction.magnitude;

        if (Physics.Raycast(mainCamera.transform.position, direction.normalized, distance, obstacleMask))
        {
            return false;
        }

        return true;
    }
}