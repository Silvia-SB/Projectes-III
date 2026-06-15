using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    private EnemyConfig config;
    private float nextRefreshTime;
    private float nextTeleportTime;
    private Transform debugTarget;
    private NavMeshPath navMeshPath;

    private void Awake()
    {
        navMeshPath = new NavMeshPath();
    }

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
            if (TryGetReachableTargetPosition(agent, target, out Vector3 destination))
            {
                agent.isStopped = false;
                agent.SetDestination(destination);
            }

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
        bool hasPathToTarget = agent.isOnNavMesh && HasCompletePathToTarget(agent, enemyTransform.position, target);

        if (!tooClose && !tooFar && hasPathToTarget) return;

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

        if (!TryFindPointInsidePlayerSphere(agent, target, out Vector3 teleportPoint))
        {
            return false;
        }

        StopAgent(agent);

        bool warped = agent.Warp(teleportPoint);

        if (!warped)
        {
            return false;
        }

        LookAtTarget(enemyTransform, target.position);

        StopAgent(agent);

        nextTeleportTime = Time.time + config.rangedTeleportCooldown;
        return true;
    }

    private bool TryFindPointInsidePlayerSphere(NavMeshAgent agent, Transform target, out Vector3 result)
    {
        Vector3 targetPosition = target.position;
        float minDistance = Mathf.Max(0.1f, config.rangedTeleportMinDistance);
        float maxDistance = Mathf.Max(minDistance, config.rangedTeleportMaxDistance);
        float navMeshSampleRadius = Mathf.Max(1f, config.rangedTeleportNavMesh);
        int attempts = Mathf.Max(config.rangedTeleportAttempts, 64);
        int areaMask = agent.areaMask;

        if (!EnemyNavMeshUtility.TrySamplePosition(
                targetPosition,
                navMeshSampleRadius,
                areaMask,
                out Vector3 targetNavMeshPosition))
        {
            result = Vector3.zero;
            return false;
        }

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
                    areaMask))
            {
                continue;
            }

            float distanceToTarget = FlatDistance(hit.position, targetPosition);

            if (distanceToTarget < minDistance)
                continue;

            if (distanceToTarget > maxDistance)
                continue;

            if (!IsInsidePlayerViewArea(target, hit.position))
                continue;

            if (!HasPlayerLineOfSight(target, hit.position))
                continue;

            if (!EnemyNavMeshUtility.HasCompletePath(hit.position, targetNavMeshPosition, areaMask, navMeshPath))
                continue;

            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    private bool IsInsidePlayerViewArea(Transform target, Vector3 candidate)
    {
        Vector3 playerForward = target.forward;
        playerForward.y = 0f;

        if (playerForward.sqrMagnitude < 0.001f) return true;
        playerForward.Normalize();

        Vector3 directionToCandidate = candidate - target.position;
        directionToCandidate.y = 0f;

        if (directionToCandidate.sqrMagnitude < 0.001f) return true;
        directionToCandidate.Normalize();

        float halfAngle = Mathf.Clamp(config.rangedTeleportViewAngle, 1f, 360f) * 0.5f;
        return Vector3.Angle(playerForward, directionToCandidate) <= halfAngle;
    }

    private bool HasPlayerLineOfSight(Transform target, Vector3 candidate)
    {
        if (!config.rangedTeleportNeedsLineOfSight) return true;
        if (config.obstacleMask.value == 0) return true;

        Vector3 origin = target.position + Vector3.up * 1.6f;
        Vector3 destination = candidate + Vector3.up;

        return !Physics.Linecast(origin, destination, config.obstacleMask);
    }

    private bool TryGetReachableTargetPosition(NavMeshAgent agent, Transform target, out Vector3 destination)
    {
        destination = Vector3.zero;

        if (agent == null || target == null) return false;
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return false;

        float sampleRadius = Mathf.Max(1f, config.rangedTeleportNavMesh);

        if (!EnemyNavMeshUtility.TrySamplePosition(target.position, sampleRadius, agent.areaMask, out Vector3 targetNavMeshPosition))
        {
            return false;
        }

        if (!EnemyNavMeshUtility.HasCompletePath(agent.nextPosition, targetNavMeshPosition, agent.areaMask, navMeshPath))
        {
            return false;
        }

        destination = targetNavMeshPosition;
        return true;
    }

    private bool HasCompletePathToTarget(NavMeshAgent agent, Vector3 sourcePosition, Transform target)
    {
        if (agent == null || target == null) return false;

        float sampleRadius = Mathf.Max(1f, config.rangedTeleportNavMesh);

        if (!EnemyNavMeshUtility.TrySampleReachablePosition(
                sourcePosition,
                target.position,
                sampleRadius,
                sampleRadius,
                agent.areaMask,
                navMeshPath,
                out _))
        {
            return false;
        }

        return true;
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
