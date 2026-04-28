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
            ReturnToPool();
            return;
        }

        if (other.CompareTag("Player")) return; 

        IDamageable target = other.GetComponentInParent<IDamageable>();
        
        if (target != null && !hitTargets.Contains(target))
        {
            hitTargets.Add(target);
            target.TakeDamage(damage, damageType);

            if (!isFullyCharged)
            {
                ReturnToPool();
            }
        }
    }

    protected override void OnHit(Collider other) 
    {
    }
}