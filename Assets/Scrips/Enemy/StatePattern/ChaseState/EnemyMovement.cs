using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    private EnemyConfig config;
    private float nextRefreshTime;
    private float nextTeleportTime;
    private Transform debugTarget;


    public void Configure(EnemyConfig config)
    {
        this.config = config;
        nextRefreshTime = 0f;
        nextTeleportTime = 0f;
    }

    public void MoveTo(EnemyController enemyController)
    {
        if (config == null) return;

        NavMeshAgent agent = enemyController.GetNavMeshAgent();
        Transform target = enemyController.GetTarget();
        Transform enemyTransform = enemyController.transform;

        if (agent == null || target == null) return;
        if (!agent.isActiveAndEnabled) return;

        if (!config.isRanged && !agent.isOnNavMesh) return;

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
        debugTarget = target;

        StopAgent(agent);
        LookAtTarget(enemyTransform, target.position);

        float distanceToTarget = FlatDistance(enemyTransform.position, target.position);

        bool tooClose = distanceToTarget < config.rangedMinDistance;
        bool tooFar = distanceToTarget > config.rangedMaxDistance;

        if (!tooClose && !tooFar) return;

        TryTeleportInsidePlayerSphere(enemyTransform, agent, target);
    }

    public bool TryTeleportAroundTarget(EnemyController enemyController)
    {
        if (config == null) return false;

        NavMeshAgent agent = enemyController.GetNavMeshAgent();
        Transform target = enemyController.GetTarget();
        Transform enemyTransform = enemyController.transform;

        if (agent == null || target == null) return false;
        if (!agent.isActiveAndEnabled) return false;

        return TryTeleportInsidePlayerSphere(enemyTransform, agent, target);
    }

    private bool TryTeleportInsidePlayerSphere(Transform enemyTransform, NavMeshAgent agent, Transform target)
    {
        if (Time.time < nextTeleportTime) return false;

        if (!TryFindPointInsidePlayerSphere(target.position, out Vector3 teleportPoint))
        {
            Debug.LogWarning("Medico no encuentra punto dentro de la esfera del player sobre el NavMesh.", this);
            return false;
        }

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

    private bool TryFindPointInsidePlayerSphere(Vector3 targetPosition, out Vector3 result)
    {
        float minDistance = Mathf.Max(0.1f, config.rangedMinDistance);
        float maxDistance = Mathf.Max(minDistance, config.rangedMaxDistance);
        float navMeshSampleRadius = Mathf.Max(1f, config.rangedTeleportNavMesh);
        int attempts = Mathf.Max(config.rangedTeleportAttempts, 64);

        for (int i = 0; i < attempts; i++)
        {
            Vector2 direction = Random.insideUnitCircle;
            if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;
            direction.Normalize();

            float randomDistance = Random.Range(minDistance, maxDistance);

            Vector3 candidate = targetPosition + new Vector3(
                direction.x * randomDistance,
                0f,
                direction.y * randomDistance
            );

            if (!NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    navMeshSampleRadius,
                    NavMesh.AllAreas))
            {
                continue;
            }

            float distanceToTarget = FlatDistance(hit.position, targetPosition);

            if (distanceToTarget < minDistance)
                continue;

            if (distanceToTarget > maxDistance)
                continue;

            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
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

    private void OnDrawGizmosSelected()
    {
        if (config == null || !config.isRanged) return;

        Transform sphereTarget = debugTarget;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            sphereTarget = player.transform;
        }

        if (sphereTarget == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(sphereTarget.position, config.rangedMaxDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(sphereTarget.position, config.rangedMinDistance);
    }
    
}
