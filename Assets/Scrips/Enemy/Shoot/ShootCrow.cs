using UnityEngine;
using UnityEngine.AI;

public class ShootCrow : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint; 
    [SerializeField] private int maxActiveCrows = 5;
    [SerializeField] private float recoverTime = 3f;
    [SerializeField] private float spawnNavMeshRadius = 3f;
    [SerializeField] private float targetNavMeshRadius = 3f;
    
    private float timer = 0f;
    private int activeCrows = 0;
    private NavMeshPath spawnPath;

    private void Awake()
    {
        spawnPath = new NavMeshPath();
    }

    public void ShootingCrow(Transform target)
    {
        if (activeCrows >= maxActiveCrows) return;
        if (spawnPoint == null || target == null) return;
        if (EnemyPool.Instance == null) return;
        
        GameObject cuervo = EnemyPool.Instance.GetEnemy(EnemyType.Cuervo);
        
        if (cuervo == null) return;

        cuervo.SetActive(false);

        if (!TryGetValidSpawnPosition(cuervo, target, out Vector3 spawnPosition))
        {
            EnemyPool.Instance.ReturnEnemyToPool(EnemyType.Cuervo, cuervo);
            return;
        }

        cuervo.transform.position = spawnPosition;
        cuervo.transform.rotation = spawnPoint.rotation;
        cuervo.SetActive(true);

        NavMeshAgent agent = cuervo.GetComponent<NavMeshAgent>();

        if (agent != null && agent.isActiveAndEnabled && !agent.Warp(spawnPosition))
        {
            EnemyPool.Instance.ReturnEnemyToPool(EnemyType.Cuervo, cuervo);
            return;
        }

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }

        activeCrows++;
    }

    private void Update()
    {
        if (activeCrows <= 0) return;

        timer += Time.deltaTime;

        if (timer >= recoverTime)
        {
            activeCrows--;
            timer = 0f;
        }
    }

    private bool TryGetValidSpawnPosition(GameObject cuervo, Transform target, out Vector3 spawnPosition)
    {
        spawnPosition = Vector3.zero;

        NavMeshAgent agent = cuervo.GetComponent<NavMeshAgent>();
        if (agent == null) return false;

        return EnemyNavMeshUtility.TrySampleReachablePosition(
            spawnPoint.position,
            target.position,
            spawnNavMeshRadius,
            targetNavMeshRadius,
            agent.areaMask,
            spawnPath,
            out spawnPosition);
    }
}
