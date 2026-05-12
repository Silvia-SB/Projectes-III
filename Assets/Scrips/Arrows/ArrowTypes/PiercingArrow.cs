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

    protected override bool ProcessCollision(Collider other, Vector3 hitPoint)
    {
        if (other.CompareTag("Player") || other == col) return false; 

        IDamageable target = other.GetComponentInParent<IDamageable>();
        
        if (other.CompareTag("Wall") || (!other.isTrigger && target == null))
        {
            transform.position = hitPoint - transform.forward * (arrowLength - penetrationDepth);
            StickToTarget(other);
            return true;
        }

        if (target != null)
        {
            if (!hitTargets.Contains(target))
            {
                hitTargets.Add(target);
                float multiplier = GetDamageMultiplier(other);
                target.TakeDamage(damage * multiplier, damageType);
            }

            if (!isFullyCharged)
            {
                transform.position = hitPoint - transform.forward * (arrowLength - penetrationDepth);
                StickToTarget(other);
                return true;
            }
        }
        
        return false;
    }

    protected override void OnHit(Collider other) 
    {
    }
}