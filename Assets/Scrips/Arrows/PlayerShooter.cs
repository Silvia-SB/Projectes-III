using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private ArrowPool arrowPool;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1; 
    
    private float nextFireTime;

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started) 
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;
        
        Arrow arrow = arrowPool.GetArrow();
        arrow.transform.position = firePoint.position;
        arrow.transform.rotation = firePoint.rotation;
        arrow.Launch();
    }
}