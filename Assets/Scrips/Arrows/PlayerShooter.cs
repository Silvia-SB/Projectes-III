using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private ArrowPool arrowPool;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float reloadVisualTime = 0.2f;
    [SerializeField] private float maxChargeTime = 2f;
    [SerializeField] private ArrowType currentArrowType = ArrowType.Base;

    private Arrow currentArrowInstance;
    private float nextFireTime;
    private float reloadTimer;
    private bool isWaitingForReload;
    private float chargeStartTime;

    private void Start() => PrepareArrow();

    private void Update()
    {
        if (isWaitingForReload && Time.time >= reloadTimer)
        {
            isWaitingForReload = false;
            PrepareArrow();
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started && !isWaitingForReload && Time.time >= nextFireTime && currentArrowInstance != null)
        {
            chargeStartTime = Time.time;
        }
        else if (context.canceled && currentArrowInstance != null)
        {
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
        if (context.performed && currentArrowType != ArrowType.Blood) ChangeArrowType(ArrowType.Blood);
    }

    private void Shoot(float chargePercent)
    {
        nextFireTime = Time.time + fireRate;

        currentArrowInstance.transform.SetParent(null);
        Rigidbody rb = currentArrowInstance.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        currentArrowInstance.Launch(chargePercent);
        currentArrowInstance = null;

        reloadTimer = Time.time + reloadVisualTime;
        isWaitingForReload = true;
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