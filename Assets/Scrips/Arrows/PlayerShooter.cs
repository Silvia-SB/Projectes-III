using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private ArrowPool arrowPool;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float maxChargeTime = 4f;
    [SerializeField] private ArrowType currentArrowType = ArrowType.Base;
    [SerializeField] private Camera playerCamera; 
    [SerializeField] private LayerMask aimLayerMask = ~0; 
    [SerializeField] private float minAimDistance = 2f;
    [SerializeField] private GameObject crosshair;

    private Arrow currentArrowInstance;
    private float nextFireTime;
    private bool isWaitingForReload;
    private bool isCharging;
    private float chargeStartTime;
    private bool isFireButtonHeld;

    public event Action OnChargeStart;
    public event Action OnChargeEnd;
    public event Action<float, float> OnChargeUpdate;

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
            OnChargeUpdate?.Invoke(currentCharge, maxChargeTime);
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
                OnChargeEnd?.Invoke();
                float chargePercent = Mathf.Clamp01((Time.time - chargeStartTime) / maxChargeTime);
                Shoot(chargePercent);
            }
            
            if (crosshair != null) crosshair.SetActive(false);
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
        OnChargeStart?.Invoke();
        if (crosshair != null) crosshair.SetActive(true);
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

        if (playerCamera != null)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Vector3 targetPoint = ray.GetPoint(100f);

            RaycastHit[] hits = Physics.RaycastAll(ray, 1000f, aimLayerMask);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                if (hit.distance >= minAimDistance)
                {
                    targetPoint = hit.point;
                    break;
                }
            }

            Vector3 shootDirection = (targetPoint - firePoint.position).normalized;
            currentArrowInstance.transform.rotation = Quaternion.LookRotation(shootDirection);
        }
        
        Rigidbody rb = currentArrowInstance.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        currentArrowInstance.Launch();
        currentArrowInstance = null;

        isWaitingForReload = true;

        if (!CanAffordArrow(currentArrowType))
        {
            currentArrowType = ArrowType.Base;
        }
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
            if (crosshair != null && !isFireButtonHeld) crosshair.SetActive(false);
        }

        currentArrowType = newType;
        nextFireTime = Time.time + fireRate;
        isWaitingForReload = true;
    }
}