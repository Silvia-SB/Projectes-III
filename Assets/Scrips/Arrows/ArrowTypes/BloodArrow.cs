using UnityEngine;
using System.Collections.Generic;

public class BloodArrow : Arrow 
{
    public override ArrowType type => ArrowType.Blood;
    public override DamageType damageType => DamageType.Blood;
    
    [Header("Hit damage")]
    [SerializeField] private float baseDamage = 5f;
    [SerializeField] private float maxDamage = 30f;
    [SerializeField] private float innerAoeRadius = 2.5f;
    [SerializeField] private float outerAoeRadius = 5f;

    [Header("Damage over time (DoT)")]
    [SerializeField] private int dotTicks = 5;
    [SerializeField] private float dotInterval = 0.4f; 

    protected override void OnHit(Collider other) 
    {
        if (other != null && other.CompareTag("Liquid"))
        {
            CorruptTarget(other);
        }

        if (isFullyCharged) 
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, outerAoeRadius);
            HashSet<IDamageable> processedTargets = new HashSet<IDamageable>();
                
            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Player")) continue; 
                if (col.CompareTag("Liquid"))
                {
                    CorruptTarget(col);
                    continue; 
                }

                IDamageable target = col.GetComponentInParent<IDamageable>();
                if (target != null && !processedTargets.Contains(target))
                {
                    float distance = Vector3.Distance(transform.position, col.ClosestPoint(transform.position));
                    float damageToApply = (distance <= innerAoeRadius) ? maxDamage : baseDamage;
                    
                    target.TakeDamage(damageToApply, damageType);
                    target.TakeRecurrentDamage(baseDamage, dotInterval, dotTicks, damageType);
                    processedTargets.Add(target);
                }
            }
        }
        else if (other != null && !other.CompareTag("Liquid")) 
        {
            IDamageable directTarget = other.GetComponentInParent<IDamageable>();
            if (directTarget != null) 
            {
                directTarget.TakeDamage(baseDamage, damageType);
                directTarget.TakeRecurrentDamage(baseDamage, dotInterval, dotTicks, damageType);
            }
        }
    }

    private void CorruptTarget(Collider col)
    {
        if (!col.TryGetComponent(out CorruptedLiquid liquid))
            liquid = col.gameObject.AddComponent<CorruptedLiquid>();
        
        liquid.Activate();
    }
}