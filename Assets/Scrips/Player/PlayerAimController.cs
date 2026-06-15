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

    public Transform AimAssistTarget { get; private set; }
    public Vector3 AimAssistPoint { get; private set; }
    public float LastAimAssistUpdateTime { get; private set; }

    public (Vector3 point, Vector3 direction, bool wasIntercepted) CalculateAimData(Camera playerCamera, Transform firePoint, float arrowLength, bool isShotBlocked, bool hasReachedMinCharge, float initialVelocity = 60f)
    {
        if (playerCamera == null || firePoint == null) return (Vector3.zero, Vector3.forward, false);

        var camData = GetCameraTargetData(playerCamera);
        Vector3 shootDirection = GetShootDirection(playerCamera, firePoint, camData.targetPoint, camData.hit, camData.distance, arrowLength);

        float angleDifference = Vector3.Angle(playerCamera.transform.forward, firePoint.forward);
        if (angleDifference > bowMisalignmentThreshold)
        {
            shootDirection = firePoint.forward;
        }

        var arrowData = GetArrowHitData(firePoint, shootDirection, arrowLength, initialVelocity);
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

    private (Vector3 hitPoint, bool hit) GetArrowHitData(Transform firePoint, Vector3 shootDirection, float arrowLength, float initialVelocity)
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
        
        Vector3 currentPos = firePoint.position;
        Vector3 currentVel = shootDirection * initialVelocity;
        Transform currentTarget = null;
        Vector3 targetPoint = Vector3.zero;

        float timeStep = 0.05f; 
        float maxTime = 3f; 

        for (float t = 0; t < maxTime; t += timeStep)
        {
            Vector3 nextPos = currentPos + currentVel * timeStep;
            Vector3 segmentDir = nextPos - currentPos;
            float segmentDist = segmentDir.magnitude;

            int gravityHitsCount = Physics.RaycastNonAlloc(currentPos, segmentDir.normalized, aimHits, segmentDist, aimLayerMask, QueryTriggerInteraction.Ignore);
            float closestGravityDist = float.MaxValue;
            int closestGravityIndex = -1;

            for (int i = 0; i < gravityHitsCount; i++)
            {
                if (aimHits[i].collider.CompareTag("Player")) continue;
                if (aimHits[i].distance < closestGravityDist)
                {
                    closestGravityDist = aimHits[i].distance;
                    closestGravityIndex = i;
                }
            }

            if (closestGravityIndex != -1)
            {
                if (aimHits[closestGravityIndex].collider.CompareTag("Enemy"))
                {
                    currentTarget = aimHits[closestGravityIndex].collider.transform;
                    
                    HitboxManager hitboxManager = null;
                    if (aimHits[closestGravityIndex].collider.attachedRigidbody != null)
                        aimHits[closestGravityIndex].collider.attachedRigidbody.TryGetComponent(out hitboxManager);
                    else
                        aimHits[closestGravityIndex].collider.TryGetComponent(out hitboxManager);

                    if (hitboxManager != null)
                    {
                        Vector3? bestPoint = hitboxManager.GetAimAssistTargetPoint();
                        targetPoint = bestPoint.HasValue ? bestPoint.Value : aimHits[closestGravityIndex].collider.bounds.center;
                    }
                    else
                    {
                        targetPoint = aimHits[closestGravityIndex].collider.bounds.center;
                    }
                }
                break;
            }

            currentPos = nextPos;
            currentVel += Physics.gravity * timeStep; 
        }

        if (currentTarget != null)
        {
            AimAssistTarget = currentTarget;
            AimAssistPoint = targetPoint;
            LastAimAssistUpdateTime = Time.time;
        }
        else if (Time.time - LastAimAssistUpdateTime > 0.15f || (AimAssistTarget != null && !AimAssistTarget.gameObject.activeInHierarchy))
        {
            AimAssistTarget = null;
        }
        else if (AimAssistTarget != null)
        {
            if (AimAssistTarget.TryGetComponent(out HitboxManager hm))
            {
                Vector3? bestPoint = hm.GetAimAssistTargetPoint();
                if (bestPoint.HasValue) AimAssistPoint = bestPoint.Value;
            }
            else
            {
                AimAssistPoint = AimAssistTarget.position;
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
        float backOffset = 0.5f;
        Vector3 rayOrigin = firePoint.position - shootDirection * backOffset;
        float rayDistance = backOffset + arrowLength;

        int hitCount = Physics.RaycastNonAlloc(rayOrigin, shootDirection, aimHits, rayDistance, aimLayerMask, QueryTriggerInteraction.Ignore);
        float closestValidDist = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            if (aimHits[i].collider.CompareTag("Player")) continue;
            
            if (aimHits[i].distance < backOffset - 0.1f) continue;
            
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
