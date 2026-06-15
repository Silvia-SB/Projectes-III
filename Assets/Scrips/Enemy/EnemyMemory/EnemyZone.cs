using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyZone : MonoBehaviour, IResettable
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

    [Header("Zone Behavior")]
    [SerializeField] private bool ignoreTriggerExit = true;

    [Header("Visibility Settings")]
    [SerializeField] private LayerMask obstacleMask;
    
    [Header("Audio")]
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private float volume = 1f;
    [SerializeField] private float pitch = 1f;

    private bool hasSpawned;
    private Transform playerTransform;
    private Camera mainCamera;
    private int initialQuantity;
    private float initialSpawnInterval;
    private int initialSpawnCount;
    private AudioManagerEnemyZone audioManager;
    
    private static List<EnemyZone> triggeredZones = new List<EnemyZone>();
    [HideInInspector] public List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Awake()
    {
        mainCamera = Camera.main;
        audioManager = AudioManagerEnemyZone.AudioManager;
        AudioManagerEnemyZone.PreloadClip(spawnSound);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (hasSpawned) return;

            playerTransform = other.transform;
            hasSpawned = true;
            if(spawnSound != null && audioManager != null) audioManager.PlayClip(spawnSound, volume, pitch);
            
            if (!triggeredZones.Contains(this))
            {
                triggeredZones.RemoveAll(z => z == null); 
                triggeredZones.Add(this);
                
                if (triggeredZones.Count > 3)
                {
                    for (int i = 0; i < triggeredZones.Count - 3; i++)
                    {
                        triggeredZones[i].DeactivateDistantEnemies(40f, playerTransform.position);
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!ignoreTriggerExit && other.CompareTag("Player"))
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

        if (!canSpawn){
            return false;
        }

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

                spawnedEnemies.Add(enemy);

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

    public void DeactivateDistantEnemies(float distance, Vector3 playerPos)
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemyObj = spawnedEnemies[i];
            if (enemyObj == null || !enemyObj.activeInHierarchy)
            {
                spawnedEnemies.RemoveAt(i);
                continue;
            }

            if (Vector3.Distance(playerPos, enemyObj.transform.position) > distance)
            {
                if (enemyObj.TryGetComponent<EnemyController>(out var controller))
                {
                    controller.Despawn();
                }
                else
                {
                    enemyObj.SetActive(false);
                }
                spawnedEnemies.RemoveAt(i);
            }
        }
    }

    public void CaptureInitialState()
    {
        //Dont need to capture initial state
    }

    public void ResetState()
    {
        hasSpawned = false;
        playerTransform = null;

        foreach (var data in zoneData)
        {
            data.currentTimer = 0f;
            data.totalSpawnedEnemies = 0;
        }
        
        spawnedEnemies.Clear();
        if (triggeredZones.Contains(this))
        {
            triggeredZones.Remove(this);
        }
    }
}
