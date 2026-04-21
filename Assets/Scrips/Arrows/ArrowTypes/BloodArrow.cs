using UnityEngine;
using System.Collections.Generic;

public class BloodArrow : Arrow 
{
    public override ArrowType type => ArrowType.Blood;
    
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
        if (isFullyCharged) 
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, outerAoeRadius);
            HashSet<IDamageable> processedTargets = new HashSet<IDamageable>();
                
            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Player")) continue; 

                IDamageable target = col.GetComponentInParent<IDamageable>();
                
                if (target != null && !processedTargets.Contains(target))
                {
                    float distance = Vector3.Distance(transform.position, col.ClosestPoint(transform.position));
                    
                    if (distance <= innerAoeRadius)
                    {
                        target.TakeDamage(maxDamage);
                    }
                    else
                    {
                        target.TakeDamage(baseDamage);
                    }
                    
                    target.TakeRecurrentDamage(baseDamage, dotInterval, dotTicks);
                    processedTargets.Add(target);
                }
            }
        }
        else 
        {
            IDamageable directTarget = null;
            if (other != null)
            {
                directTarget = other.GetComponentInParent<IDamageable>();
            }
            
            if (directTarget != null) 
            {
                directTarget.TakeDamage(baseDamage);
                
                directTarget.TakeRecurrentDamage(baseDamage, dotInterval, dotTicks);
            }
        }
    }
}