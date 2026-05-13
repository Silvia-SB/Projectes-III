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

        NavMeshAgent agent = enemyController.GetNavMeshAgent();
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
            desiredPosition = GetMeleePosition(enemyController.transform, target.position);
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

    private Vector3 GetMeleePosition(Transform enemyTransform, Vector3 targetPosition)
    {
        float distanceToTarget = Vector3.Distance(enemyTransform.position, targetPosition);
        
        if (distanceToTarget > 4f)
        {
            Vector3 randomOffset = Random.insideUnitSphere * config.targetOffsetRadius;
            randomOffset.y = 0f;
            return targetPosition + randomOffset;
        }

        return targetPosition;
    }

    private Vector3 GetRangedPosition(EnemyController enemyController)
    {
        Transform target = enemyController.GetTarget();
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (Mathf.Abs(distanceToTarget - config.preferredDistance) <= 1f)
        {
            return transform.position; 
        }

        Vector3 directionAwayFromPlayer = (transform.position - target.position).normalized;
        
        Vector3 sideOffset = Vector3.zero;
        if (distanceToTarget > config.preferredDistance)
        {
            sideOffset = transform.right * Random.Range(-2f, 2f);
        }

        return target.position 
               + directionAwayFromPlayer * config.preferredDistance 
               + sideOffset;
    }
}