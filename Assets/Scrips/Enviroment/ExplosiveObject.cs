using UnityEngine;
using System.Collections.Generic;

public class ExplosiveObject : Health
{
    private const int InfiniteTicks = 9999;
    private const float DefaultDotAmount = 5f;

    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float instantDamage = 50f;
    [SerializeField] private float dotAmount = 5f;
    [SerializeField] private float dotInterval = 0.4f;
    [SerializeField] private int dotTicks = 5;
    [SerializeField] private DamageType damageType = DamageType.Blood;

    private bool hasExploded;

    public override void TakeDamage(float amount, DamageType incomingDamageType)
    {
        if (incomingDamageType != damageType) return;
        
        base.TakeDamage(amount, incomingDamageType);

        if (statusManager?.HasStatus(damageType) == false)
        {
            float initialDotAmount = amount > 0 ? amount : DefaultDotAmount;
            base.TakeRecurrentDamage(initialDotAmount, dotInterval, InfiniteTicks, incomingDamageType);
        }
    }

    public override void TakeRecurrentDamage(float amount, float interval, int ticks, DamageType incomingDamageType)
    {
        if (incomingDamageType != damageType) return;
        
        base.TakeRecurrentDamage(amount, interval, InfiniteTicks, incomingDamageType);
    }

    protected override void Die()
    {
        if (!hasExploded)
        {
            hasExploded = true;
            Explode();
        }
        
        base.Die();
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        var damagedTargets = new HashSet<IDamageable>();

        foreach (Collider col in colliders)
        {
            if (col.gameObject == gameObject) continue;

            IDamageable target = col.GetComponentInParent<IDamageable>();
            
            if (target != null && damagedTargets.Add(target))
            {
                target.TakeDamage(instantDamage, damageType);
                target.TakeRecurrentDamage(dotAmount, dotInterval, dotTicks, damageType);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}