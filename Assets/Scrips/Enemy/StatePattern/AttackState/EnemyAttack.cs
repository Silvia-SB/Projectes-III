using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public void PlagueDoctorAttack(Vector3 targetPosition, DamageType damageType, float damage)
    {
        Debug.Log($"Plague Doctor attacks towards {targetPosition} with {damage} damage of type {damageType}");
    }
    
    public void MeleeAttack(Transform target, DamageType damageType, float damage)
    {
        Debug.Log($"Melee attack on {target.name} with {damage} damage of type {damageType}");
        
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
