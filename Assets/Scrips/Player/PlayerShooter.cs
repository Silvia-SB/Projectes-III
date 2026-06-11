using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(PlayerAimController))]
[RequireComponent(typeof(ProceduralBowAnimation))]
public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArrowPool arrowPool;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Camera playerCamera; 

    [Header("Controllers")]
    [SerializeField] private PlayerAimController aimController;
    [SerializeField] private ProceduralBowAnimation bowAnimation;

    [Header("Shooting Settings")]
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float minChargeTime = 1f;
    [SerializeField] private float fullChargeTime = 4f;
    [SerializeField] private float minShootVelocity = 25f;
    [SerializeField] private float maxShootVelocity = 60f;
    [SerializeField] private ArrowType currentArrowType = ArrowType.Base;

    private Arrow currentArrowInstance;
    private float nextFireTime;
    private float emergencySpawnTime;
    private bool isWaitingForReload;
    private bool isCharging;
    private float chargeStartTime;
    private bool isFireButtonHeld;
    private bool hasReachedMinCharge;
    private bool isAimMarkerActive; 

    public event Action OnChargeStart;
    public event Action OnChargeEnd;
    public event Action<float, float> OnChargeUpdate;
    public event Action OnMinChargeReached;
    public event Action<Vector2> OnAimPointUpdated;
    public event Action OnAimPointLost;
    public event Action<ArrowType> OnArrowChanged;
    public event Action OnShootTriggered;
    public event Action OnChargeCanceled;

    public float MinChargePercentage => minChargeTime / fullChargeTime;

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (aimController == null) aimController = GetComponent<PlayerAimController>();
        if (bowAnimation == null) bowAnimation = GetComponent<ProceduralBowAnimation>();
        PrepareArrow();
        OnArrowChanged?.Invoke(currentArrowType);
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        HandleEmergencyReload();
        CheckAutoCharge();
        UpdateChargingState();
    }

    private void HandleEmergencyReload()
    {
        if (isWaitingForReload && Time.time >= emergencySpawnTime)
        {
            AnimationEvent_SpawnArrow();
        }
    }

    private void CheckAutoCharge()
    {
        bool isAligning = bowAnimation != null && bowAnimation.IsAligningBow;
        if (isFireButtonHeld && !isCharging && currentArrowInstance != null && !isWaitingForReload && !isAligning && Time.time >= nextFireTime)
        {
            StartCharging();
        }
    }

    private void UpdateChargingState()
    {
        if (isCharging)
        {
            float currentCharge = Time.time - chargeStartTime;
            if (!hasReachedMinCharge && currentCharge >= minChargeTime)
            {
                hasReachedMinCharge = true;
                OnMinChargeReached?.Invoke();
            }
            
            OnChargeUpdate?.Invoke(currentCharge, fullChargeTime);

            if (playerCamera != null && aimController != null)
            {
                var aimData = aimController.CalculateAimData(playerCamera, firePoint, currentArrowInstance != null ? currentArrowInstance.ArrowLength : 0f, IsShotBlocked(), hasReachedMinCharge);
                
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
        if (currentArrowInstance != null)
        {
            currentArrowInstance.ReturnToPool();
            currentArrowInstance = null;
        }

        if (!CanAffordArrow(currentArrowType))
        {
        currentArrowType = ArrowType.Base;
        OnArrowChanged?.Invoke(currentArrowType);
        }

        isWaitingForReload = false;
        PrepareArrow();
    }

    private bool IsShotBlocked(bool ignoreRetraction = false)
    {
        if (aimController == null || currentArrowInstance == null) return false;
        float retractionWeight = bowAnimation != null ? bowAnimation.CurrentRetractionWeight : 0f;
        return aimController.IsShotBlocked(firePoint, currentArrowInstance.ArrowLength, retractionWeight, ignoreRetraction);
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f)
        {
            if (context.canceled)
            {
                isFireButtonHeld = false;
                isCharging = false;
                isAimMarkerActive = false;
                OnChargeEnd?.Invoke();
                OnChargeCanceled?.Invoke();
            }
            return;
        }

        if (context.started)
        {
            isFireButtonHeld = true;
            bool isAligning = bowAnimation != null && bowAnimation.IsAligningBow;
            if (!isWaitingForReload && !isAligning && currentArrowInstance != null && !isCharging && Time.time >= nextFireTime)
            {
                StartCharging();
            }
        }
        else if (context.canceled)
        {
            isFireButtonHeld = false;
            if (isCharging) ReleaseCharge();
        }
    }

    private void ReleaseCharge()
    {
        isCharging = false; 
        isAimMarkerActive = false;
        OnChargeEnd?.Invoke();
        
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
                OnChargeCanceled?.Invoke();
            }
        }
        else
        {
            OnChargeCanceled?.Invoke();
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
        OnChargeStart?.Invoke();
    }

    public void OnSelectBase(InputAction.CallbackContext context)
    {
        if (context.performed) TryChangeArrowType(ArrowType.Base);
    }

    public void OnSelectBlood(InputAction.CallbackContext context)
    {
        if (context.performed) TryChangeArrowType(ArrowType.Blood);
    }

    public void OnSelectPiercing(InputAction.CallbackContext context)
    {
        if (context.performed) TryChangeArrowType(ArrowType.Piercing);
    }

    public void OnSelectElectric(InputAction.CallbackContext context)
    {
        if (context.performed) TryChangeArrowType(ArrowType.Electric);
    }

    private void TryChangeArrowType(ArrowType targetType)
    {
        if (Time.timeScale == 0f) return;

        bool isAligning = bowAnimation != null && bowAnimation.IsAligningBow;
        if (currentArrowType == targetType || isCharging || isWaitingForReload || isAligning) return;
        
        if (targetType == ArrowType.Base || CanAffordArrow(targetType))
        {
            ChangeArrowType(targetType);
        }
    }

    private void Shoot(float chargePercent)
    {
        if (SoulManager.Instance != null && !SoulManager.Instance.TryConsumeSouls(currentArrowType))
        {
            OnChargeCanceled?.Invoke();
            return;
        }

        nextFireTime = Time.time + fireRate;

        currentArrowInstance.isFullyCharged = chargePercent >= 1f;

        if (chargePercent >= 1f)
        {
            AchievementManager.UnlockAchievement("fully_charged");
        }

        currentArrowInstance.transform.SetParent(null);

        Vector3 shootDirection = playerCamera.transform.forward;
        Vector3 startPos = firePoint.position;
        if (aimController != null)
        {
            var aimData = aimController.CalculateAimData(playerCamera, firePoint, currentArrowInstance.ArrowLength, IsShotBlocked(), hasReachedMinCharge);
            shootDirection = aimData.direction;
            startPos = aimController.CalculateArrowStartPos(firePoint, shootDirection, currentArrowInstance.ArrowLength);
        }

        currentArrowInstance.transform.position = startPos;
        currentArrowInstance.transform.rotation = Quaternion.LookRotation(shootDirection, firePoint.up);

        float shootVelocity = Mathf.Lerp(minShootVelocity, maxShootVelocity, chargePercent);
        currentArrowInstance.Launch(shootVelocity);
        currentArrowInstance = null;

        isWaitingForReload = true;
        emergencySpawnTime = Time.time + 2.0f;

        OnShootTriggered?.Invoke();
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

    if (currentArrowInstance == null && currentArrowType != ArrowType.Base)
    {
        currentArrowType = ArrowType.Base;
        OnArrowChanged?.Invoke(currentArrowType);
        currentArrowInstance = arrowPool.GetArrow(currentArrowType);
    }

        if (currentArrowInstance == null) return;

        if (bowAnimation != null)
            bowAnimation.CurrentArrowLength = currentArrowInstance.ArrowLength;

        InitializeArrowTransformAndPhysics();
        
        if (bowAnimation != null)
            bowAnimation.StartBowAlignment();
    }

    private void InitializeArrowTransformAndPhysics()
    {
        currentArrowInstance.transform.SetParent(firePoint);
        currentArrowInstance.transform.localPosition = Vector3.zero;
        currentArrowInstance.transform.localRotation = Quaternion.identity;
        currentArrowInstance.gameObject.layer = LayerMask.NameToLayer("Weapon");

        if (currentArrowInstance.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;
        }

        if (currentArrowInstance.TryGetComponent<Collider>(out var col)) col.enabled = false; 
    }

    public void ChangeArrowType(ArrowType newType)
    {
        if (isCharging)
        {
            isCharging = false;
            OnChargeEnd?.Invoke();
        }

        currentArrowType = newType;
        OnArrowChanged?.Invoke(currentArrowType);
        nextFireTime = Time.time + fireRate;
        isWaitingForReload = true;
        emergencySpawnTime = Time.time + 2.0f;
    }
    
    public void ResetState()
    {
        TryChangeArrowType(ArrowType.Base);
        PrepareArrow();
        OnArrowChanged?.Invoke(ArrowType.Base);
    }
}