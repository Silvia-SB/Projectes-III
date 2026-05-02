using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    private EnemyConfig config;
    private float nextRefreshTime;

    public void Configure(EnemyConfig config)
    {
        this.config = config;
    }

    public void MoveTo(EnemyController enemyController)
    {
        if (config == null) return;

        NavMeshAgent agent = enemyController.navMeshAgent;
        Transform target = enemyController.GetTarget();

        if (agent == null || target == null) return;
        if (!agent.isOnNavMesh) return;

        if (Time.time < nextRefreshTime) return;

        Vector3 desiredPosition;

        if (config.isRanged)
        {
            desiredPosition = GetRangedPosition(enemyController);
        }
        else
        {
            desiredPosition = GetMeleePosition(target.position);
        }

        if (NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        nextRefreshTime = Time.time + Random.Range(
            config.destinationRefreshMin,
            config.destinationRefreshMax
        );
    }

    private Vector3 GetMeleePosition(Vector3 targetPosition)
    {
        Vector3 randomOffset = Random.insideUnitSphere * config.targetOffsetRadius;
        randomOffset.y = 0f;

        return targetPosition + randomOffset;
    }

    private Vector3 GetRangedPosition(EnemyController enemyController)
    {
        Transform target = enemyController.GetTarget();

        Vector3 directionAwayFromPlayer = (transform.position - target.position).normalized;
        Vector3 sideOffset = transform.right * Random.Range(-2f, 2f);

        return target.position 
               + directionAwayFromPlayer * config.preferredDistance 
               + sideOffset;
    }

    public void Stop(EnemyController enemyController)
    {
        NavMeshAgent agent = enemyController.navMeshAgent;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }
}