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

    private Arrow currentArrowInstance;
    private float nextFireTime;
    private bool isWaitingForReload;
    private bool isCharging;
    private float chargeStartTime;
    private bool isFireButtonHeld;
    private bool hasReachedMinCharge;
    private RaycastHit[] aimHits = new RaycastHit[20];     private readonly Vector3 viewportCenter = new Vector3(0.5f, 0.5f, 0f); // Evita crear un new Vector3 cada frame
    private bool isAimMarkerActive; 

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
                var (impactPoint, wasIntercepted) = CalculateActualImpactPoint();
                
                if (wasIntercepted)
                {
                    Vector2 screenPos = playerCamera.WorldToScreenPoint(impactPoint);
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
                    float chargePercent = Mathf.Clamp01((chargeDuration - minChargeTime) / (fullChargeTime - minChargeTime));
                    Shoot(chargePercent);
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

        Vector3 targetPoint = CalculateActualImpactPoint().point;
        Vector3 shootDirection = (targetPoint - firePoint.position).normalized;
        currentArrowInstance.transform.rotation = Quaternion.LookRotation(shootDirection);

        float shootVelocity = Mathf.Lerp(minShootVelocity, maxShootVelocity, chargePercent);
        currentArrowInstance.Launch(shootVelocity);
        currentArrowInstance = null;

        isWaitingForReload = true;

        if (!CanAffordArrow(currentArrowType))
        {
            currentArrowType = ArrowType.Base;
        }
    }

    private (Vector3 point, bool wasIntercepted) CalculateActualImpactPoint()
    {
        if (playerCamera == null) return (firePoint.position + firePoint.forward * 100f, false);

        Ray camRay = playerCamera.ViewportPointToRay(viewportCenter);
        Vector3 targetPoint = camRay.GetPoint(100f);

        int hitCount = Physics.RaycastNonAlloc(camRay, aimHits, 100f, aimLayerMask);
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            if (aimHits[i].distance < closestDistance)
            {
                closestDistance = aimHits[i].distance;
                targetPoint = aimHits[i].point;
            }
        }

        Vector3 shootDirection = (targetPoint - firePoint.position).normalized;
        
        float lengthOffset = currentArrowInstance != null ? currentArrowInstance.ArrowLength : 1f;
        Vector3 rayStart = firePoint.position + shootDirection * lengthOffset;

        float distToTarget = Vector3.Distance(rayStart, targetPoint);
        if (distToTarget > 0 && Physics.Raycast(rayStart, shootDirection, out RaycastHit arrowHit, distToTarget, aimLayerMask))
        {
            bool isSignificantDeviation = (targetPoint - arrowHit.point).sqrMagnitude > (aimMarkerThreshold * aimMarkerThreshold);
            return (arrowHit.point, isSignificantDeviation);
        }

        return (targetPoint, false);
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