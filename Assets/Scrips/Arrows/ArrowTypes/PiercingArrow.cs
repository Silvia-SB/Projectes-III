using UnityEngine;
using System.Collections.Generic;

public class PiercingArrow : Arrow 
{
    public override ArrowType type => ArrowType.Piercing;
    public override DamageType damageType => DamageType.Piercing;
    
    [SerializeField] private float damage = 40f;

    private HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    private void OnEnable()
    {
        hitTargets.Clear();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            StickToTarget(other);
            return;
        }

        if (other.CompareTag("Player")) return; 

        IDamageable target = other.GetComponentInParent<IDamageable>();
        
        if (target != null && !hitTargets.Contains(target))
        {
            hitTargets.Add(target);
            float multiplier = GetDamageMultiplier(other);
            target.TakeDamage(damage * multiplier, damageType);

            if (!isFullyCharged)
            {
                StickToTarget(other);
            }
        }
    }

    protected override void OnHit(Collider other) 
    {
    }
}