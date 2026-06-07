using UnityEngine;

public class PlayerAimController : MonoBehaviour
{
    [Header("Aiming Settings")]
    [SerializeField] private LayerMask aimLayerMask = ~0; 
    [SerializeField] private float aimMarkerThreshold = 1.0f;
    [SerializeField] private float bowMisalignmentThreshold = 2.0f;
    [SerializeField] private float obstacleDetectionDistance = 3.0f;
    [SerializeField] private float minConvergenceDistance = 2.0f;

    [Header("Block System")]
    [SerializeField] private float weaponBlockRadius = 0.05f;
    [SerializeField] private float enemyPointBlankDistance = 1.0f;

    private RaycastHit[] aimHits = new RaycastHit[20];     
    private readonly Vector3 viewportCenter = new Vector3(0.5f, 0.5f, 0f); 
    private Collider[] blockCheckColliders = new Collider[5];

    public (Vector3 point, Vector3 direction, bool wasIntercepted) CalculateAimData(Camera playerCamera, Transform firePoint, float arrowLength, bool isShotBlocked, bool hasReachedMinCharge)
    {
        if (playerCamera == null || firePoint == null) return (Vector3.zero, Vector3.forward, false);

        var camData = GetCameraTargetData(playerCamera);
        Vector3 shootDirection = GetShootDirection(playerCamera, firePoint, camData.targetPoint, camData.hit, camData.distance, arrowLength);

        float angleDifference = Vector3.Angle(playerCamera.transform.forward, firePoint.forward);
        if (angleDifference > bowMisalignmentThreshold)
        {
            shootDirection = firePoint.forward;
        }

        var arrowData = GetArrowHitData(firePoint, shootDirection, arrowLength);
        bool isSignificantDeviation = CalculateAimDeviation(playerCamera, firePoint, camData, arrowData, angleDifference, isShotBlocked, hasReachedMinCharge, arrowLength);

        return (arrowData.hitPoint, shootDirection, isSignificantDeviation);
    }

    private (Vector3 targetPoint, bool hit, float distance) GetCameraTargetData(Camera playerCamera)
    {
        Ray camRay = playerCamera.ViewportPointToRay(viewportCenter);
        Vector3 targetPoint = camRay.GetPoint(100f);
        int hitCount = Physics.RaycastNonAlloc(camRay, aimHits, 100f, aimLayerMask, QueryTriggerInteraction.Ignore);
        
        float closestDistance = float.MaxValue;
        bool camHit = false;

        for (int i = 0; i < hitCount; i++)
        {
            if (aimHits[i].collider.CompareTag("Player")) continue;
            if (aimHits[i].distance < closestDistance)
            {
                closestDistance = aimHits[i].distance;
                targetPoint = aimHits[i].point;
                camHit = true;
            }
        }
        return (targetPoint, camHit, closestDistance);
    }

    private Vector3 GetShootDirection(Camera playerCamera, Transform firePoint, Vector3 targetPoint, bool camHit, float closestDistance, float arrowLength)
    {
        if (IsPointBlankWithEnemy(firePoint, arrowLength) || (camHit && closestDistance < minConvergenceDistance))
        {
            return playerCamera.transform.forward;
        }
        return (targetPoint - firePoint.position).normalized;
    }

    private (Vector3 hitPoint, bool hit) GetArrowHitData(Transform firePoint, Vector3 shootDirection, float arrowLength)
    {
        float lengthOffset = arrowLength > 0 ? arrowLength : 1f;
        Vector3 rayStart = firePoint.position + shootDirection * lengthOffset;
        Vector3 actualHitPoint = rayStart + shootDirection * 100f;
        
        int arrowHitsCount = Physics.RaycastNonAlloc(rayStart, shootDirection, aimHits, 100f, aimLayerMask, QueryTriggerInteraction.Ignore);
        float closestArrowDist = float.MaxValue;
        bool arrowHit = false;
        
        for (int i = 0; i < arrowHitsCount; i++)
        {
            if (aimHits[i].collider.CompareTag("Player")) continue;
            if (aimHits[i].distance < closestArrowDist)
            {
                closestArrowDist = aimHits[i].distance;
                actualHitPoint = aimHits[i].point;
                arrowHit = true;
            }
        }
        return (actualHitPoint, arrowHit);
    }

    private bool CalculateAimDeviation(Camera playerCamera, Transform firePoint, (Vector3 targetPoint, bool hit, float distance) camData, (Vector3 hitPoint, bool hit) arrowData, float angleDifference, bool isShotBlocked, bool hasReachedMinCharge, float arrowLength)
    {
        if (isShotBlocked || IsPointBlankWithEnemy(firePoint, arrowLength) || (camData.hit && camData.distance < minConvergenceDistance) || !hasReachedMinCharge)
        {
            return false;
        }

        bool isSignificantDeviation = (camData.targetPoint - arrowData.hitPoint).sqrMagnitude > (aimMarkerThreshold * aimMarkerThreshold);

        if (!camData.hit && !arrowData.hit)
        {
            return false;
        }
        else if (angleDifference > bowMisalignmentThreshold)
        {
            float camPitch = playerCamera.transform.eulerAngles.x;
            if (camPitch > 180f) camPitch -= 360f;

            if (camPitch > 0f) return true;

            float distToTarget = Vector3.Distance(firePoint.position, camData.targetPoint);
            float distToActual = Vector3.Distance(firePoint.position, arrowData.hitPoint);

            if (distToActual >= distToTarget - obstacleDetectionDistance)
            {
                return false;
            }
        }

        return isSignificantDeviation;
    }

    public Vector3 CalculateArrowStartPos(Transform firePoint, Vector3 shootDirection, float arrowLength)
    {
        Vector3 startPos = firePoint.position;
        float backOffset = 2.0f;
        Vector3 rayOrigin = firePoint.position - shootDirection * backOffset;
        float rayDistance = backOffset + arrowLength;

        int hitCount = Physics.RaycastNonAlloc(rayOrigin, shootDirection, aimHits, rayDistance, aimLayerMask, QueryTriggerInteraction.Ignore);
        float closestValidDist = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            if (aimHits[i].collider.CompareTag("Player")) continue;
            
            if (aimHits[i].distance < closestValidDist)
            {
                closestValidDist = aimHits[i].distance;
                startPos = aimHits[i].point - shootDirection * arrowLength;
            }
        }
        
        return startPos;
    }

    public bool IsShotBlocked(Transform firePoint, float arrowLength, float currentRetractionWeight, bool ignoreRetraction = false)
    {
        if (arrowLength <= 0 || firePoint == null) return false;

        if (!ignoreRetraction && currentRetractionWeight > 0.3f)
        {
            return true;
        }

        Vector3 startPos = firePoint.position;
        Vector3 endPos = firePoint.position + firePoint.forward * arrowLength;

        int count = Physics.OverlapCapsuleNonAlloc(startPos, endPos, weaponBlockRadius, blockCheckColliders, aimLayerMask, QueryTriggerInteraction.Ignore);
        
        for (int i = 0; i < count; i++)
        {
            if (blockCheckColliders[i].CompareTag("Player") || blockCheckColliders[i].CompareTag("Enemy")) continue;
            return true;
        }
        return false;
    }

    public bool IsPointBlankWithEnemy(Transform firePoint, float arrowLength)
    {
        if (arrowLength <= 0 || firePoint == null) return false;

        Vector3 startPos = firePoint.position;
        Vector3 endPos = firePoint.position + firePoint.forward * (arrowLength + enemyPointBlankDistance);

        int count = Physics.OverlapCapsuleNonAlloc(startPos, endPos, weaponBlockRadius, blockCheckColliders, aimLayerMask, QueryTriggerInteraction.Collide);
        
        for (int i = 0; i < count; i++)
        {
            if (blockCheckColliders[i].CompareTag("Enemy")) return true;
        }
        return false;
    }
}
