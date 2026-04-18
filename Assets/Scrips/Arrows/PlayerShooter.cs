using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private ArrowPool arrowPool;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.5f;
    
    [SerializeField] private ArrowType currentArrowType = ArrowType.Base; 

    private float nextFireTime;

    public void OnShoot(InputAction.CallbackContext context) {
        if (context.performed) Shoot();
    }

    public void OnSelectBase(InputAction.CallbackContext context) {
        if (context.performed) currentArrowType = ArrowType.Base;
    }

    public void OnSelectBlood(InputAction.CallbackContext context) {
        if (context.performed) currentArrowType = ArrowType.Blood;
    }

    private void Shoot() {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireRate;

        Arrow arrow = arrowPool.GetArrow(currentArrowType);
        if (arrow != null) {
            arrow.transform.position = firePoint.position;
            arrow.transform.rotation = firePoint.rotation;
            arrow.Launch();
        }
    }
}