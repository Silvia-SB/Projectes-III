using UnityEngine;
using UnityEngine.AI;

public static class EnemyNavMeshUtility
{
    public static bool TrySamplePosition(Vector3 desiredPosition, float sampleRadius, int areaMask, out Vector3 result)
    {
        if (NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, Mathf.Max(0.1f, sampleRadius), areaMask))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    public static bool HasCompletePath(Vector3 from, Vector3 to, int areaMask, NavMeshPath path)
    {
        if (path == null)
        {
            path = new NavMeshPath();
        }

        path.ClearCorners();
        return NavMesh.CalculatePath(from, to, areaMask, path) &&
               path.status == NavMeshPathStatus.PathComplete;
    }

    public static bool TrySampleReachablePosition(
        Vector3 desiredPosition,
        Vector3 targetPosition,
        float desiredSampleRadius,
        float targetSampleRadius,
        int areaMask,
        NavMeshPath path,
        out Vector3 reachablePosition)
    {
        reachablePosition = Vector3.zero;

        if (!TrySamplePosition(desiredPosition, desiredSampleRadius, areaMask, out Vector3 sourcePosition))
        {
            return false;
        }

        if (!TrySamplePosition(targetPosition, targetSampleRadius, areaMask, out Vector3 targetNavMeshPosition))
        {
            return false;
        }

        if (!HasCompletePath(sourcePosition, targetNavMeshPosition, areaMask, path))
        {
            return false;
        }

        reachablePosition = sourcePosition;
        return true;
    }
}
