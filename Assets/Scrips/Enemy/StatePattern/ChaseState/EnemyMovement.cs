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
        Transform enemyTransform = enemyController.transform;

        if (agent == null || target == null) return;
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        if (config.isRanged)
        {
            MoveRanged(enemyTransform, agent, target);
        }
        else
        {
            MoveMelee(enemyTransform, agent, target);
        }
    }

    private void MoveMelee(Transform enemyTransform, NavMeshAgent agent, Transform target)
    {
        if (Time.time < nextRefreshTime) return;

        Vector3 desiredPosition = GetMeleePosition(enemyTransform, target.position);

        if (NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }

        nextRefreshTime = Time.time + Random.Range(
            config.destinationRefreshMin,
            config.destinationRefreshMax
        );
    }

    private void MoveRanged(Transform enemyTransform, NavMeshAgent agent, Transform target)
    {
        float preferredDistance = config.preferredDistance;

        if (preferredDistance <= 0f)
        {
            preferredDistance = config.attackRange * 0.75f;
        }

        float distanceToTarget = FlatDistance(enemyTransform.position, target.position);

        float tolerance = 1.5f;
        float minDistance = preferredDistance - tolerance;
        float maxDistance = preferredDistance + tolerance;

        bool isComfortable =
            distanceToTarget >= minDistance &&
            distanceToTarget <= maxDistance;

        if (isComfortable)
        {
            StopAgent(agent);
            LookAtTarget(enemyTransform, target.position);
            return;
        }

        if (agent.hasPath && !agent.pathPending)
        {
            if (agent.remainingDistance > agent.stoppingDistance + 0.2f)
            {
                LookAtTarget(enemyTransform, target.position);
                return;
            }
        }

        if (Time.time < nextRefreshTime)
        {
            LookAtTarget(enemyTransform, target.position);
            return;
        }

        Vector3 desiredPosition = GetRangedPosition(enemyTransform, target.position, preferredDistance);

        if (NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }

        LookAtTarget(enemyTransform, target.position);

        nextRefreshTime = Time.time + Random.Range(
            config.destinationRefreshMin,
            config.destinationRefreshMax
        );
    }

    private Vector3 GetMeleePosition(Transform enemyTransform, Vector3 targetPosition)
    {
        float distanceToTarget = FlatDistance(enemyTransform.position, targetPosition);

        if (distanceToTarget > 4f)
        {
            Vector3 randomOffset = Random.insideUnitSphere * config.targetOffsetRadius;
            randomOffset.y = 0f;
            return targetPosition + randomOffset;
        }

        return targetPosition;
    }

    private Vector3 GetRangedPosition(Transform enemyTransform, Vector3 targetPosition, float preferredDistance)
    {
        Vector3 directionFromPlayer = enemyTransform.position - targetPosition;
        directionFromPlayer.y = 0f;

        if (directionFromPlayer.sqrMagnitude < 0.001f)
        {
            directionFromPlayer = -enemyTransform.forward;
        }

        directionFromPlayer.Normalize();

        Vector3 desiredPosition = targetPosition + directionFromPlayer * preferredDistance;
        desiredPosition.y = enemyTransform.position.y;

        return desiredPosition;
    }

    private void StopAgent(NavMeshAgent agent)
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();
    }

    private float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void LookAtTarget(Transform enemyTransform, Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - enemyTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        enemyTransform.rotation = Quaternion.Slerp(
            enemyTransform.rotation,
            Quaternion.LookRotation(direction),
            Time.deltaTime * 8f
        );
    }
}