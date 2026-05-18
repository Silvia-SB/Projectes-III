using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    private EnemyConfig config;
    private float nextRefreshTime;
    private float nextTeleportTime;


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

        if (EnemyType.Cuervo.Equals(enemyController.Config.type))
        {
            enemyController.GetNavMeshAgent().SetDestination(enemyController.GetTarget().position);
            return;
        }
        if (config.isRanged)
        {
            MoveRanged(enemyTransform, agent, target);
            return;
        }
       
        MoveMelee(enemyTransform, agent, target);
        
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
    
    private float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void MoveRanged(Transform enemyTransform, NavMeshAgent agent, Transform target)
    {
        StopAgent(agent);
        LookAtTarget(enemyTransform, target.position);

        float distanceToTarget = FlatDistance(enemyTransform.position, target.position);

        bool tooClose = distanceToTarget < config.rangedMinDistance;
        bool tooFar = distanceToTarget > config.rangedMaxDistance;

        if (!tooClose && !tooFar) return;

        TryTeleportAroundTarget(enemyTransform, agent, target);
    }
    
     public bool TryTeleportAroundTarget(EnemyController enemyController)
    {
        if (config == null) return false;

        NavMeshAgent agent = enemyController.GetNavMeshAgent();
        Transform target = enemyController.GetTarget();
        Transform enemyTransform = enemyController.transform;

        if (agent == null || target == null) return false;
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return false;

        return TryTeleportAroundTarget(enemyTransform, agent, target);
    }

    private bool TryTeleportAroundTarget(Transform enemyTransform, NavMeshAgent agent, Transform target)
    {
        if (Time.time < nextTeleportTime) return false;

        if (!TryFindTeleportPoint(target.position, agent.radius, out Vector3 teleportPoint))
            return false;

        StopAgent(agent);

        bool warped = agent.Warp(teleportPoint);

        if (!warped)
        {
            enemyTransform.position = teleportPoint;
        }

        LookAtTarget(enemyTransform, target.position);

        StopAgent(agent);

        nextTeleportTime = Time.time + config.rangedTeleportCooldown;
        return true;
    }

    private bool TryFindTeleportPoint(Vector3 targetPosition, float agentRadius, out Vector3 result)
    {
        for (int i = 0; i < config.rangedTeleportAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized;

            float randomDistance = Random.Range(
                config.rangedTeleportMinDistance,
                config.rangedTeleportMaxDistance
            );

            Vector3 candidate = targetPosition + new Vector3(
                randomCircle.x * randomDistance,
                0f,
                randomCircle.y * randomDistance
            );

            if (!NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    config.rangedTeleportNavMesh,
                    NavMesh.AllAreas))
            {
                continue;
            }

            float distanceToTarget = FlatDistance(hit.position, targetPosition);

            if (distanceToTarget < config.rangedTeleportMinDistance)
                continue;

            if (distanceToTarget > config.rangedTeleportMaxDistance)
                continue;

            if (!HasEnoughSpace(hit.position, agentRadius))
                continue;

            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    private bool HasEnoughSpace(Vector3 position, float agentRadius)
    {
        float checkRadius = agentRadius * 0.9f;

        Vector3 checkPosition = position + Vector3.up * 0.5f;

        return !Physics.CheckSphere(
            checkPosition,
            checkRadius,
            config.obstacleMask,
            QueryTriggerInteraction.Ignore
        );
    }
    private void LookAtTarget(Transform enemyTransform, Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - enemyTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        enemyTransform.rotation = Quaternion.Slerp(
            enemyTransform.rotation,
            Quaternion.LookRotation(direction),
            Time.deltaTime * 8f
        );
    }
    private void StopAgent(NavMeshAgent agent)
    {
        if (agent == null) return;
        if (!agent.isActiveAndEnabled) return;
        if (!agent.isOnNavMesh) return;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();
    }
    
}