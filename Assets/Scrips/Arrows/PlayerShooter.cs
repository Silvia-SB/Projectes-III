using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(100)]
public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArrowPool arrowPool;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Camera playerCamera; 

    [Header("Shooting Settings")]
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float minChargeTime = 1f;
    [SerializeField] private float fullChargeTime = 4f;
    [SerializeField] private float minShootVelocity = 25f;
    [SerializeField] private float maxShootVelocity = 60f;
    [SerializeField] private ArrowType currentArrowType = ArrowType.Base;

    [Header("Aiming Settings")]
    [SerializeField] private LayerMask aimLayerMask = ~0; 
    [SerializeField] private float aimMarkerThreshold = 1.0f;
    [SerializeField] private float bowMisalignmentThreshold = 2.0f;
    [SerializeField] private float obstacleDetectionDistance = 3.0f;
    [SerializeField] private float minConvergenceDistance = 2.0f;

    [Header("Bow Alignment (Procedural IK)")]
    [SerializeField] private Transform stringNockPoint;
    [SerializeField] private Transform bowRestPoint;
    [SerializeField] private Transform reloadStartPoint;
    [SerializeField] private float bowAlignmentDuration = 0.2f;
    
    [Header("Block System")]
    [SerializeField] private float weaponBlockRadius = 0.05f;
    [SerializeField] private float enemyPointBlankDistance = 1.0f;

    [Header("Weapon Retraction")]
    [SerializeField] private Transform weaponRoot;
    [SerializeField] private Vector3 retractedPositionOffset = new Vector3(0f, -0.2f, -0.1f);
    [SerializeField] private Vector3 retractedRotationOffset = new Vector3(25f, -15f, 0f);
    [SerializeField] private float retractionSpeed = 12f;

    [Header("Weapon Sway (Procedural)")]
    [SerializeField] private float swayPosMultiplier = 0.0005f;
    [SerializeField] private float maxSwayPos = 0.05f;
    [SerializeField] private float swayRotMultiplier = 0.04f;
    [SerializeField] private float maxSwayRot = 4.0f;
    [SerializeField] private float swaySmooth = 12f;

    [Header("Bow Movement Limits")]
    [SerializeField] private float maxPitchUp = -35f;
    [SerializeField] private float maxPitchDown = 35f;

    [Header("Animators")]
    [SerializeField] private Animator armsAnimator;
    [SerializeField] private Animator bowAnimator;

    private Arrow currentArrowInstance;
    private float nextFireTime;
    private float emergencySpawnTime;
    private bool isWaitingForReload;
    private bool isCharging;
    private float chargeStartTime;
    private bool isFireButtonHeld;
    private bool hasReachedMinCharge;
    private bool isAimMarkerActive; 
    private Vector3 initialWeaponPos;
    private Quaternion initialWeaponRot;
    private float currentRetractionWeight;
    private float bowAlignmentWeight = 1f;
    private bool isAligningBow;
    private float alignmentStartTime;
    private Vector3 initialLocalAlignDir;
    private Vector3 currentSwayPos;
    private Quaternion currentSwayRot = Quaternion.identity;
    private float lastCamYaw;
    private float lastCamPitch;
    private float smoothedPitchVelocity;
    private float smoothedYawVelocity;

    private static readonly int isChargingHash = Animator.StringToHash("isCharging");
    private static readonly int cancelChargeHash = Animator.StringToHash("cancelCharge");
    private static readonly int shootHash = Animator.StringToHash("Shoot");
    private static readonly int changeArrowHash = Animator.StringToHash("changeArrow");

    private RaycastHit[] aimHits = new RaycastHit[20];     
    private readonly Vector3 viewportCenter = new Vector3(0.5f, 0.5f, 0f); 
    private Collider[] blockCheckColliders = new Collider[5];

    public event Action OnChargeStart;
    public event Action OnChargeEnd;
    public event Action<float, float> OnChargeUpdate;
    public event Action OnMinChargeReached;
    public event Action<Vector2> OnAimPointUpdated;
    public event Action OnAimPointLost;
    public event Action<ArrowType> OnArrowChanged;

    public float MinChargePercentage => minChargeTime / fullChargeTime;

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (weaponRoot != null)
        {
            initialWeaponPos = weaponRoot.localPosition;
            initialWeaponRot = weaponRoot.localRotation;
        }
        if (playerCamera != null) 
        {
            lastCamYaw = playerCamera.transform.eulerAngles.y;
            lastCamPitch = playerCamera.transform.eulerAngles.x;
        }
        PrepareArrow();
        OnArrowChanged?.Invoke(currentArrowType);
    }

    private void Update()
    {
        if (isWaitingForReload && Time.time >= emergencySpawnTime)
        {
            AnimationEvent_SpawnArrow();
        }

        if (isAligningBow)
        {
            float elapsed = Time.time - alignmentStartTime;
            bowAlignmentWeight = bowAlignmentDuration > 0f ? Mathf.Clamp01(elapsed / bowAlignmentDuration) : 1f;
            if (bowAlignmentWeight >= 1f)
            {
                isAligningBow = false;
            }
        }

        if (isFireButtonHeld && !isCharging && currentArrowInstance != null && !isWaitingForReload && !isAligningBow && Time.time >= nextFireTime)
        {
            StartCharging();
        }

        if (isCharging)
        {
            float currentCharge = Time.time - chargeStartTime;
            if (!hasReachedMinCharge && currentCharge >= minChargeTime)
            {
                hasReachedMinCharge = true;
                OnMinChargeReached?.Invoke();
            }
            
            OnChargeUpdate?.Invoke(currentCharge, fullChargeTime);

            if (playerCamera != null)
            {
                var aimData = CalculateAimData();
                
                if (aimData.wasIntercepted)
                {
                    Vector2 screenPos = playerCamera.WorldToScreenPoint(aimData.point);
                    OnAimPointUpdated?.Invoke(screenPos);
                    isAimMarkerActive = true;
                }
                else if (isAimMarkerActive)
                {
                    OnAimPointLost?.Invoke();
                    isAimMarkerActive = false;
                }
            }
        }

    }

    public void AnimationEvent_SpawnArrow()
    {

        if (!isWaitingForReload) return;

        if (!CanAffordArrow(currentArrowType))
        {
            ChangeArrowType(ArrowType.Base);
            return;
        }

        isWaitingForReload = false;
        PrepareArrow();
    }

    private void LateUpdate()
    {
        UpdateWeaponSway();

        Vector3 targetBasePos = initialWeaponPos + currentSwayPos;
        Quaternion targetBaseRot = initialWeaponRot * currentSwayRot;

        if (weaponRoot != null)
        {
            weaponRoot.localPosition = targetBasePos;
            weaponRoot.localRotation = targetBaseRot;
        }

        AlignFirePoint();

        if (weaponRoot != null)
        {
            bool shouldRetract = IsShotBlocked(true);

            float targetWeight = shouldRetract ? 1f : 0f;
            currentRetractionWeight = Mathf.Lerp(currentRetractionWeight, targetWeight, Time.deltaTime * retractionSpeed);

            weaponRoot.localPosition = Vector3.Lerp(targetBasePos, initialWeaponPos + retractedPositionOffset, currentRetractionWeight);
            weaponRoot.localRotation = Quaternion.Slerp(targetBaseRot, initialWeaponRot * Quaternion.Euler(retractedRotationOffset), currentRetractionWeight);
        }

        AlignFirePoint();
    }

    private void UpdateWeaponSway()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        float targetSwayPosX = 0f;
        float targetSwayPosY = 0f;
        float targetSwayRotX = 0f;
        float targetSwayRotY = 0f;
        float targetSwayRotZ = 0f;

        if (playerCamera != null)
        {
            Vector3 camEuler = playerCamera.transform.eulerAngles;
            float currentPitch = camEuler.x;
            float currentYaw = camEuler.y;
            float rawPitchVelocity = Mathf.DeltaAngle(lastCamPitch, currentPitch) / dt;
            float rawYawVelocity = Mathf.DeltaAngle(lastCamYaw, currentYaw) / dt;
            smoothedPitchVelocity = Mathf.Lerp(smoothedPitchVelocity, rawPitchVelocity, dt * 15f);
            smoothedYawVelocity = Mathf.Lerp(smoothedYawVelocity, rawYawVelocity, dt * 15f);
            targetSwayPosX = Mathf.Clamp(-smoothedYawVelocity * swayPosMultiplier, -maxSwayPos, maxSwayPos);
            targetSwayPosY = Mathf.Clamp(smoothedPitchVelocity * swayPosMultiplier, -maxSwayPos, maxSwayPos);
            float normalizedPitch = currentPitch;
            if (normalizedPitch > 180f) normalizedPitch -= 360f;
            
            float excessPitch = 0f;
            if (normalizedPitch < maxPitchUp) excessPitch = normalizedPitch - maxPitchUp;
            else if (normalizedPitch > maxPitchDown) excessPitch = normalizedPitch - maxPitchDown;

            targetSwayRotX = Mathf.Clamp(smoothedPitchVelocity * swayRotMultiplier, -maxSwayRot, maxSwayRot) - excessPitch;
            targetSwayRotY = Mathf.Clamp(-smoothedYawVelocity * swayRotMultiplier, -maxSwayRot, maxSwayRot);
            targetSwayRotZ = Mathf.Clamp(smoothedYawVelocity * swayRotMultiplier * 0.5f, -maxSwayRot, maxSwayRot); // Leve "Roll"

            lastCamPitch = currentPitch;
            lastCamYaw = currentYaw;
        }
        
        currentSwayPos.x = Mathf.Lerp(currentSwayPos.x, targetSwayPosX, dt * swaySmooth);
        currentSwayPos.y = Mathf.Lerp(currentSwayPos.y, targetSwayPosY, dt * swaySmooth);
        
        currentSwayRot = Quaternion.Slerp(currentSwayRot, Quaternion.Euler(targetSwayRotX, targetSwayRotY, targetSwayRotZ), dt * swaySmooth);
    }

    private void AlignFirePoint()
    {
        if (stringNockPoint != null && bowRestPoint != null && firePoint != null)
        {
            firePoint.position = stringNockPoint.position;

            Vector3 forwardDirection = bowRestPoint.position - stringNockPoint.position;
            Vector3 targetForward = forwardDirection.sqrMagnitude > 0.001f ? forwardDirection.normalized : stringNockPoint.forward;
            Quaternion targetRotation = Quaternion.LookRotation(targetForward, bowRestPoint.up);

            if (bowAlignmentWeight < 1f)
            {
                float smoothWeight = Mathf.SmoothStep(0f, 1f, bowAlignmentWeight);
                
                Vector3 startForward = bowRestPoint.TransformDirection(initialLocalAlignDir);
                
                Quaternion startRotation = Quaternion.FromToRotation(targetForward, startForward) * targetRotation;
                firePoint.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothWeight);
            }
            else
            {
                firePoint.rotation = targetRotation;
            }
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isFireButtonHeld = true;
            if (!isWaitingForReload && !isAligningBow && currentArrowInstance != null && !isCharging && Time.time >= nextFireTime)
            {
                StartCharging();
            }
        }
        else if (context.canceled)
        {
            isFireButtonHeld = false;
            if (isCharging)
            {
                isCharging = false; 
                isAimMarkerActive = false;
                OnChargeEnd?.Invoke();
                SetAnimatorBool(isChargingHash, false);
                
                float chargeDuration = Time.time - chargeStartTime;
                if (chargeDuration >= minChargeTime)
                {
                    if (currentArrowInstance != null && !IsShotBlocked())
                    {
                        float chargePercent = Mathf.Clamp01((chargeDuration - minChargeTime) / (fullChargeTime - minChargeTime));
                        Shoot(chargePercent);
                
                    }
                    else
                    {
                        SetAnimatorTrigger(cancelChargeHash);
                    }
                }
                else
                {
                    SetAnimatorTrigger(cancelChargeHash);
                }
            }
            
        }
    }

    private void StartCharging()
    {
        if (!CanAffordArrow(currentArrowType))
        {
            ChangeArrowType(ArrowType.Base);
            return;
        }

        if (currentArrowInstance == null) return;

        chargeStartTime = Time.time;
        isCharging = true; 
        hasReachedMinCharge = false;
        isAimMarkerActive = false;
        SetAnimatorBool(isChargingHash, true);
        OnChargeStart?.Invoke();
    }

    public void OnSelectBase(InputAction.CallbackContext context)
    {
        if (context.performed && currentArrowType != ArrowType.Base) 
        {
            if (isCharging || isWaitingForReload || isAligningBow) return;
            ChangeArrowType(ArrowType.Base);
        }
    }

    public void OnSelectBlood(InputAction.CallbackContext context)
    {
        if (context.performed && currentArrowType != ArrowType.Blood) 
        {
            if (isCharging || isWaitingForReload || isAligningBow) return;
            if (CanAffordArrow(ArrowType.Blood)){
                ChangeArrowType(ArrowType.Blood);
            }
        }
    }

    public void OnSelectPiercing(InputAction.CallbackContext context)
    {
        if (context.performed && currentArrowType != ArrowType.Piercing) 
        {
            if (isCharging || isWaitingForReload || isAligningBow) return;
            if (CanAffordArrow(ArrowType.Piercing)){
                ChangeArrowType(ArrowType.Piercing);
            }
        }
    }

    public void OnSelectElectric(InputAction.CallbackContext context)
    {
        if (context.performed && currentArrowType != ArrowType.Electric) 
        {
            if (isCharging || isWaitingForReload || isAligningBow) return;
            if (CanAffordArrow(ArrowType.Electric)){
                ChangeArrowType(ArrowType.Electric);
            }
        }
    }

    private void Shoot(float chargePercent)
    {
        if (SoulManager.Instance != null && !SoulManager.Instance.TryConsumeSouls(currentArrowType))
        {
            SetAnimatorTrigger(cancelChargeHash);
            return;
        }

        nextFireTime = Time.time + fireRate;

        currentArrowInstance.isFullyCharged = chargePercent >= 1f;

        if (chargePercent >= 1f)
        {
            AchievementManager.UnlockAchievement("fully_charged");
        }

        currentArrowInstance.transform.SetParent(null);

        var aimData = CalculateAimData();
        Vector3 shootDirection = aimData.direction;

        Vector3 startPos = firePoint.position;
        float backOffset = 2.0f;
        Vector3 rayOrigin = firePoint.position - shootDirection * backOffset;
        float rayDistance = backOffset + currentArrowInstance.ArrowLength;

        int hitCount = Physics.RaycastNonAlloc(rayOrigin, shootDirection, aimHits, rayDistance, aimLayerMask, QueryTriggerInteraction.Ignore);
        float closestValidDist = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            if (aimHits[i].collider.CompareTag("Player")) continue;
            
            if (aimHits[i].distance < closestValidDist)
            {
                closestValidDist = aimHits[i].distance;
                startPos = aimHits[i].point - shootDirection * currentArrowInstance.ArrowLength;
            }
        }

        currentArrowInstance.transform.position = startPos;
        currentArrowInstance.transform.rotation = Quaternion.LookRotation(shootDirection, firePoint.up);

        float shootVelocity = Mathf.Lerp(minShootVelocity, maxShootVelocity, chargePercent);
        currentArrowInstance.Launch(shootVelocity);
        currentArrowInstance = null;

        isWaitingForReload = true;
        emergencySpawnTime = Time.time + 2.0f;

        SetAnimatorTrigger(shootHash);
    }

    private (Vector3 point, Vector3 direction, bool wasIntercepted) CalculateAimData()
    {
        if (playerCamera == null) return (firePoint.position + firePoint.forward * 100f, firePoint.forward, false);

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

        Vector3 shootDirection;
        
        if (IsPointBlankWithEnemy() || (camHit && closestDistance < minConvergenceDistance))
        {
            shootDirection = playerCamera.transform.forward;
        }
        else
        {
            shootDirection = (targetPoint - firePoint.position).normalized;
        }
        
        float angleDifference = Vector3.Angle(playerCamera.transform.forward, firePoint.forward);
        if (angleDifference > bowMisalignmentThreshold)
        {
            shootDirection = firePoint.forward;
        }

        float lengthOffset = currentArrowInstance != null ? currentArrowInstance.ArrowLength : 1f;
        Vector3 rayStart = firePoint.position + shootDirection * lengthOffset;
        Vector3 actualHitPoint = rayStart + shootDirection * 100f;
        bool arrowHit = false;

        int arrowHitsCount = Physics.RaycastNonAlloc(rayStart, shootDirection, aimHits, 100f, aimLayerMask, QueryTriggerInteraction.Ignore);
        float closestArrowDist = float.MaxValue;
        
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

        bool isSignificantDeviation = false;
        
        if (IsShotBlocked() || IsPointBlankWithEnemy() || (camHit && closestDistance < minConvergenceDistance) || !hasReachedMinCharge)
        {
            isSignificantDeviation = false;
        }
        else
        {
            if ((targetPoint - actualHitPoint).sqrMagnitude > (aimMarkerThreshold * aimMarkerThreshold))
            {
                isSignificantDeviation = true;
            }

            if (!camHit && !arrowHit)
            {
                isSignificantDeviation = false;
            }
            else if (angleDifference > bowMisalignmentThreshold)
            {
                float camPitch = playerCamera.transform.eulerAngles.x;
                if (camPitch > 180f) camPitch -= 360f;

                if (camPitch > 0f)
                {
                    isSignificantDeviation = true;
                }
                else
                {
                    float distToTarget = Vector3.Distance(firePoint.position, targetPoint);
                    float distToActual = Vector3.Distance(firePoint.position, actualHitPoint);

                    if (distToActual >= distToTarget - obstacleDetectionDistance)
                    {
                        isSignificantDeviation = false;
                    }
                }
            }
        }

        return (actualHitPoint, shootDirection, isSignificantDeviation);
    }

    private bool IsShotBlocked(bool ignoreRetraction = false)
    {
        if (currentArrowInstance == null) return false;

        if (!ignoreRetraction && currentRetractionWeight > 0.3f)
        {
            return true;
        }

        Vector3 startPos = firePoint.position;
        Vector3 endPos = firePoint.position + firePoint.forward * currentArrowInstance.ArrowLength;

        int count = Physics.OverlapCapsuleNonAlloc(startPos, endPos, weaponBlockRadius, blockCheckColliders, aimLayerMask, QueryTriggerInteraction.Ignore);
        
        for (int i = 0; i < count; i++)
        {
            if (blockCheckColliders[i].CompareTag("Player") || blockCheckColliders[i].CompareTag("Enemy")) continue;
            return true;
        }
        return false;
    }

    private bool IsPointBlankWithEnemy()
    {
        if (currentArrowInstance == null) return false;

        Vector3 startPos = firePoint.position;
        Vector3 endPos = firePoint.position + firePoint.forward * (currentArrowInstance.ArrowLength + enemyPointBlankDistance);

        int count = Physics.OverlapCapsuleNonAlloc(startPos, endPos, weaponBlockRadius, blockCheckColliders, aimLayerMask, QueryTriggerInteraction.Collide);
        
        for (int i = 0; i < count; i++)
        {
            if (blockCheckColliders[i].CompareTag("Enemy")) return true;
        }
        return false;
    }

    private bool CanAffordArrow(ArrowType type)
    {
        if (SoulManager.Instance == null) return true;
        return SoulManager.Instance.CurrentSouls >= SoulManager.Instance.GetArrowCost(type);
    }

    private void PrepareArrow()
    {
        if (currentArrowInstance != null) return;

        currentArrowInstance = arrowPool.GetArrow(currentArrowType);
        if (currentArrowInstance != null)
        {
            currentArrowInstance.transform.SetParent(firePoint);
            currentArrowInstance.transform.localPosition = Vector3.zero;
            currentArrowInstance.transform.localRotation = Quaternion.identity;
            
            currentArrowInstance.gameObject.layer = LayerMask.NameToLayer("Weapon");

            Rigidbody rb = currentArrowInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.interpolation = RigidbodyInterpolation.None;
            }

            Collider col = currentArrowInstance.GetComponent<Collider>();
            if (col != null) col.enabled = false; 

            isAligningBow = true;
            alignmentStartTime = Time.time;
            bowAlignmentWeight = 0f;

            if (reloadStartPoint != null && bowRestPoint != null && stringNockPoint != null)
            {
                Vector3 startDir = reloadStartPoint.position - stringNockPoint.position;
                initialLocalAlignDir = startDir.sqrMagnitude > 0.001f ? bowRestPoint.InverseTransformDirection(startDir.normalized) : bowRestPoint.InverseTransformDirection(reloadStartPoint.forward);
            }
            else if (stringNockPoint != null && bowRestPoint != null)
            {
                initialLocalAlignDir = bowRestPoint.InverseTransformDirection(stringNockPoint.forward);
            }
        }
    }

    public void ChangeArrowType(ArrowType newType)
    {
        if (currentArrowInstance != null)
        {
            currentArrowInstance.ReturnToPool();
            currentArrowInstance = null;
        }

        if (isCharging)
        {
            isCharging = false;
            OnChargeEnd?.Invoke();
            SetAnimatorBool(isChargingHash, false);
        }

        ResetAnimatorTrigger(cancelChargeHash);
        ResetAnimatorTrigger(shootHash);
        SetAnimatorTrigger(changeArrowHash);

        currentArrowType = newType;
        OnArrowChanged?.Invoke(currentArrowType);
        nextFireTime = Time.time + fireRate;
        isWaitingForReload = true;
        emergencySpawnTime = Time.time + 2.0f;
    }

    private void SetAnimatorBool(int paramHash, bool value)
    {
        if (armsAnimator != null) armsAnimator.SetBool(paramHash, value);
        if (bowAnimator != null) bowAnimator.SetBool(paramHash, value);
    }

    private void SetAnimatorTrigger(int paramHash)
    {
        if (armsAnimator != null) armsAnimator.SetTrigger(paramHash);
        if (bowAnimator != null) bowAnimator.SetTrigger(paramHash);
    }

    private void ResetAnimatorTrigger(int paramHash)
    {
        if (armsAnimator != null) armsAnimator.ResetTrigger(paramHash);
        if (bowAnimator != null) bowAnimator.ResetTrigger(paramHash);
    }
}