using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private ShootCrow shootCrow;
    
    public void PlagueDoctorAttack()
    {
        shootCrow.ShootingCrow();
    }
    
    public void MeleeAttack(Transform target, DamageType damageType, float damage)
    {
        if (target != null && target.CompareTag("Player"))
        {
            IDamageable damageable = target.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, damageType);
            }
        }
    }
    
}
