using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private ArrowPool arrowPool;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float minChargeTime = 1f;
    [SerializeField] private float fullChargeTime = 4f;
    [SerializeField] private float minShootVelocity = 25f;
    [SerializeField] private float maxShootVelocity = 60f;
    [SerializeField] private ArrowType currentArrowType = ArrowType.Base;
    [SerializeField] private Camera playerCamera; 
    [SerializeField] private LayerMask aimLayerMask = ~0; 
    [SerializeField] private float aimMarkerThreshold = 1.0f;
    [SerializeField] private float bowMisalignmentThreshold = 2.0f;
    [SerializeField] private float obstacleDetectionDistance = 3.0f;
    
    [SerializeField] private float minConvergenceDistance = 2.0f;
    
    [Header("Block System")]
    [SerializeField] private float weaponBlockRadius = 0.05f;
    [SerializeField] private float enemyPointBlankDistance = 1.0f;

    [Header("Animator")]
    [SerializeField] private Animator animator;
    
    private Arrow currentArrowInstance;
    private float nextFireTime;
    private bool isWaitingForReload;
    private bool isCharging;
    private float chargeStartTime;
    private bool isFireButtonHeld;
    private bool hasReachedMinCharge;
    private RaycastHit[] aimHits = new RaycastHit[20];     
    private readonly Vector3 viewportCenter = new Vector3(0.5f, 0.5f, 0f); 
    private bool isAimMarkerActive; 
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
        PrepareArrow();
    }

    private void Update()
    {
        if (isWaitingForReload && Time.time >= nextFireTime)
        {
            isWaitingForReload = false;
            PrepareArrow();

            if (isFireButtonHeld && !isCharging)
            {
                StartCharging();
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

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isFireButtonHeld = true;
            if (!isWaitingForReload && Time.time >= nextFireTime && currentArrowInstance != null && !isCharging)
            {
                animator.SetBool("isCharging",true);
                StartCharging();
            }
        }
        else if (context.canceled)
        {
            isFireButtonHeld = false;
            if (isCharging && currentArrowInstance != null)
            {
                isCharging = false; 
                isAimMarkerActive = false;
                OnChargeEnd?.Invoke();
                
                float chargeDuration = Time.time - chargeStartTime;
                if (chargeDuration >= minChargeTime)
                {
                    if (!IsShotBlocked())
                    {
                        animator.SetBool("isCharging",false);
                        float chargePercent = Mathf.Clamp01((chargeDuration - minChargeTime) / (fullChargeTime - minChargeTime));
                        Shoot(chargePercent);
                    }
                    else
                    {
                        
                    }
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
        chargeStartTime = Time.time;
        isCharging = true; 
        hasReachedMinCharge = false;
        isAimMarkerActive = false;
        OnChargeStart?.Invoke();
    }

    public void OnSelectBase(InputAction.CallbackContext context)
    {
        if (context.performed && currentArrowType != ArrowType.Base) 
        {
            ChangeArrowType(ArrowType.Base);
        }
    }

    public void OnSelectBlood(InputAction.CallbackContext context)
    {
        if (context.performed && currentArrowType != ArrowType.Blood) 
        {
            if (CanAffordArrow(ArrowType.Blood)){
                ChangeArrowType(ArrowType.Blood);
            }
        }
    }

    public void OnSelectPiercing(InputAction.CallbackContext context)
    {
        if (context.performed && currentArrowType != ArrowType.Piercing) 
        {
            if (CanAffordArrow(ArrowType.Piercing)){
                ChangeArrowType(ArrowType.Piercing);
            }
        }
    }

    public void OnSelectElectric(InputAction.CallbackContext context)
    {
        if (context.performed && currentArrowType != ArrowType.Electric) 
        {
            if (CanAffordArrow(ArrowType.Electric)){
                ChangeArrowType(ArrowType.Electric);
            }
        }
    }

    private void Shoot(float chargePercent)
    {
        if (SoulManager.Instance != null && !SoulManager.Instance.TryConsumeSouls(currentArrowType)) return;

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

        if (!CanAffordArrow(currentArrowType))
        {
            currentArrowType = ArrowType.Base;
        }
        animator.SetTrigger("Shoot");
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

    private bool IsShotBlocked()
    {
        if (currentArrowInstance == null) return false;

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
            if (rb != null) rb.isKinematic = true;

            Collider col = currentArrowInstance.GetComponent<Collider>();
            if (col != null) col.enabled = false; 
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
        }

        currentArrowType = newType;
        nextFireTime = Time.time + fireRate;
        isWaitingForReload = true;
    }
}