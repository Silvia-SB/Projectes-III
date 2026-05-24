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
    [SerializeField] private Transform bowTopPassPoint;
    [SerializeField] private float bowAlignmentDuration = 0.5f;
    
    [Header("Block System")]
    [SerializeField] private float weaponBlockRadius = 0.05f;
    [SerializeField] private float enemyPointBlankDistance = 1.0f;

    [Header("Weapon Retraction")]
    [Tooltip("El objeto padre de los brazos y el arco. Se moverá para evitar atravesar paredes.")]
    [SerializeField] private Transform weaponRoot;
    [SerializeField] private Vector3 retractedPositionOffset = new Vector3(0f, -0.2f, -0.1f);
    [SerializeField] private Vector3 retractedRotationOffset = new Vector3(25f, -15f, 0f);
    [SerializeField] private float retractionSpeed = 12f;

    [Header("Animators")]
    [SerializeField] private Animator armsAnimator;
    [SerializeField] private Animator bowAnimator;

    // --- State Variables ---
    private Arrow currentArrowInstance;
    private float nextFireTime;
    private bool isWaitingForReload;
    private bool isCharging;
    private float chargeStartTime;
    private bool isFireButtonHeld;
    private bool hasReachedMinCharge;
    private bool isAimMarkerActive; 
    private float bowAlignmentWeight = 1f;
    private bool isAligningBow;
    private float alignmentStartTime;
    private Vector3 initialWeaponPos;
    private Quaternion initialWeaponRot;
    private float currentRetractionWeight;

    private static readonly int isChargingHash = Animator.StringToHash("isCharging");
    private static readonly int cancelChargeHash = Animator.StringToHash("cancelCharge");
    private static readonly int shootHash = Animator.StringToHash("Shoot");
    private static readonly int changeArrowHash = Animator.StringToHash("changeArrow");

    // --- Caches & Helpers ---
    private RaycastHit[] aimHits = new RaycastHit[20];     
    private readonly Vector3 viewportCenter = new Vector3(0.5f, 0.5f, 0f); 
    private Transform camTransform;
    private Collider[] blockCheckColliders = new Collider[5];

    public event Action OnChargeStart;
    public event Action OnChargeEnd;
    public event Action<float, float> OnChargeUpdate;
    public event Action OnMinChargeReached;
    public event Action<Vector2> OnAimPointUpdated;
    public event Action OnAimPointLost;

    public float MinChargePercentage => minChargeTime / fullChargeTime;

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (weaponRoot != null)
        {
            initialWeaponPos = weaponRoot.localPosition;
            initialWeaponRot = weaponRoot.localRotation;
        }
        PrepareArrow();
    }

    private void Update()
    {
        if (isWaitingForReload && Time.time >= nextFireTime)
        {
            if (!CanAffordArrow(currentArrowType))
            {
                ChangeArrowType(ArrowType.Base);
                return;
            }

            isWaitingForReload = false;
            PrepareArrow();
        }

        if (isAligningBow)
        {
            float elapsed = Time.time - alignmentStartTime;
            bowAlignmentWeight = bowAlignmentDuration > 0f ? Mathf.Clamp01(elapsed / bowAlignmentDuration) : 1f;
            if (bowAlignmentWeight >= 1f)
            {
                isAligningBow = false;

                // Si el jugador mantuvo el botón pulsado durante la recarga, iniciamos la carga justo ahora
                if (isFireButtonHeld && !isCharging && currentArrowInstance != null)
                {
                    StartCharging();
                }
            }
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

    private void LateUpdate()
    {
        // Alineación procedural: El FirePoint sigue a la cuerda pero apunta obligatoriamente al arco
        if (stringNockPoint != null && bowRestPoint != null && firePoint != null)
        {
            firePoint.position = stringNockPoint.position;

            Vector3 targetPoint = bowRestPoint.position;

            if (bowTopPassPoint != null && bowAlignmentWeight < 1f)
            {
                targetPoint = Vector3.Lerp(bowTopPassPoint.position, bowRestPoint.position, bowAlignmentWeight);
            }

            Vector3 forwardDirection = targetPoint - stringNockPoint.position;
            if (forwardDirection.sqrMagnitude > 0.001f)
            {
                firePoint.rotation = Quaternion.LookRotation(forwardDirection);
            }
            else
            {
                firePoint.rotation = stringNockPoint.rotation;
            }
        }

        // Retracción procedural del arma al acercarse a una pared
        if (weaponRoot != null)
        {
            // 1. Movemos temporalmente el arma a su posición original para que el detector no tiemble
            weaponRoot.localPosition = initialWeaponPos;
            weaponRoot.localRotation = initialWeaponRot;

            // 2. Usamos tu detector original indicándole que ignore el peso de retracción visual
            bool shouldRetract = IsShotBlocked(true);

            float targetWeight = shouldRetract ? 1f : 0f;
            currentRetractionWeight = Mathf.Lerp(currentRetractionWeight, targetWeight, Time.deltaTime * retractionSpeed);

            weaponRoot.localPosition = Vector3.Lerp(initialWeaponPos, initialWeaponPos + retractedPositionOffset, currentRetractionWeight);
            weaponRoot.localRotation = Quaternion.Slerp(initialWeaponRot, initialWeaponRot * Quaternion.Euler(retractedRotationOffset), currentRetractionWeight);
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isFireButtonHeld = true;
            if (!isWaitingForReload && !isAligningBow && Time.time >= nextFireTime && currentArrowInstance != null && !isCharging)
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

        currentArrowInstance.transform.SetParent(null);

        //obtenemos los datos calculados
        var aimData = CalculateAimData();
        Vector3 shootDirection = aimData.direction;

        //Retrasamos el origen hacia atrás para detectar la superficie real 
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
                // Hardcodeamos la posición de la flecha restando su longitud exacta
                startPos = aimHits[i].point - shootDirection * currentArrowInstance.ArrowLength;
            }
        }

        // 3. Lanzamiento Hardcodeado en la dirección exacta calculada
        currentArrowInstance.transform.position = startPos;
        currentArrowInstance.transform.rotation = Quaternion.LookRotation(shootDirection);

        float shootVelocity = Mathf.Lerp(minShootVelocity, maxShootVelocity, chargePercent);
        currentArrowInstance.Launch(shootVelocity);
        currentArrowInstance = null;

        isWaitingForReload = true;

        SetAnimatorTrigger(shootHash);
    }

    private (Vector3 point, Vector3 direction, bool wasIntercepted) CalculateAimData()
    {
        if (playerCamera == null) return (firePoint.position + firePoint.forward * 100f, firePoint.forward, false);

        // 1. Cálculo del objetivo de la cámara
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

        // 2. Cálculo de la dirección de disparo perfecta
        Vector3 shootDirection;
        
        if (IsPointBlankWithEnemy() || (camHit && closestDistance < minConvergenceDistance))
        {
            // HARDCODE: Si disparamos a quemarropa, forzamos que vaya en línea recta paralela a la cámara
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

        // 3. Cálculo de la trayectoria real de la flecha
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

        // 4. Lógica de UI (Evaluación de desvío significativo de la mira)
        bool isSignificantDeviation = false;
        
        if (IsShotBlocked() || IsPointBlankWithEnemy() || (camHit && closestDistance < minConvergenceDistance))
        {
            // Si estamos a quemarropa o el tiro está bloqueado, forzamos a que NO se muestre la mira secundaria
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

        // Si el arma está muy retraída visualmente por la pared, bloqueamos el disparo directamente
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
        }
    }

    private void ChangeArrowType(ArrowType newType)
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

        // Limpiamos los triggers anteriores para evitar que bloqueen el Animator
        ResetAnimatorTrigger(cancelChargeHash);
        ResetAnimatorTrigger(shootHash);
        SetAnimatorTrigger(changeArrowHash);

        currentArrowType = newType;
        nextFireTime = Time.time + fireRate;
        isWaitingForReload = true;
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