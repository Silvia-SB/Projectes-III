using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private ArrowPool arrowPool;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float maxChargeTime = 4f;
    [SerializeField] private ArrowType currentArrowType = ArrowType.Base;

    private Arrow currentArrowInstance;
    private float nextFireTime;
    private bool isWaitingForReload;
    private bool isCharging;
    private float chargeStartTime;

    private void Start() => PrepareArrow();

    private void Update()
    {
        if (isWaitingForReload && Time.time >= nextFireTime)
        {
            isWaitingForReload = false;
            PrepareArrow();
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started && !isWaitingForReload && Time.time >= nextFireTime && currentArrowInstance != null)
        {
            if (!CanAffordArrow(currentArrowType))
            {
                ChangeArrowType(ArrowType.Base);
                return;
            }
            chargeStartTime = Time.time;
            isCharging = true; 
        }
        else if (context.canceled && isCharging && currentArrowInstance != null)
        {
            isCharging = false; 
            float chargePercent = Mathf.Clamp01((Time.time - chargeStartTime) / maxChargeTime);
            Shoot(chargePercent);
        }
    }

    public void OnSelectBase(InputAction.CallbackContext context)
    {
        if (context.performed && currentArrowType != ArrowType.Base) ChangeArrowType(ArrowType.Base);
    }

    public void OnSelectBlood(InputAction.CallbackContext context)
    {
        if (context.performed && currentArrowType != ArrowType.Blood) 
        {
            if (CanAffordArrow(ArrowType.Blood))
                ChangeArrowType(ArrowType.Blood);
            else
                Debug.Log("¡No tienes suficientes almas para la Flecha de Sangre!");
        }
    }

    public void OnSelectPiercing(InputAction.CallbackContext context)
    {
        if (context.performed && currentArrowType != ArrowType.Piercing) 
        {
            if (CanAffordArrow(ArrowType.Piercing))
                ChangeArrowType(ArrowType.Piercing);
            else
                Debug.Log("¡No tienes suficientes almas para la Flecha Perforante!");
        }
    }

    private void Shoot(float chargePercent)
    {
        if (SoulManager.Instance != null && !SoulManager.Instance.TryConsumeSouls(currentArrowType)) return;

        nextFireTime = Time.time + fireRate;

        currentArrowInstance.isFullyCharged = chargePercent >= 1f;

        currentArrowInstance.transform.SetParent(null);
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

        currentArrowType = newType;
        if (!isWaitingForReload) PrepareArrow();
    }
}