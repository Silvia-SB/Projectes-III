using UnityEngine;

public class BloodArrow : Arrow 
{
    public override ArrowType type => ArrowType.Blood;
    [SerializeField] private float baseDamage = 15f;
    [SerializeField] private float aoeRadius = 5f;

    protected override void OnHit(Collider other) 
    {
        float finalDamage = baseDamage * damageMultiplier;

        if (isFullyCharged) 
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, aoeRadius);
            
            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Player")) continue; 

                IDamageable target = col.GetComponentInParent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(finalDamage);
                }
            }
        }
        else 
        {
            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null) 
            {
                damageable.TakeDamage(finalDamage);
            }
        }
    }
}