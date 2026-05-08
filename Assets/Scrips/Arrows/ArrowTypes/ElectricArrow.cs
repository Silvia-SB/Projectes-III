using UnityEngine;
using System.Collections.Generic;

public class ElectricArrow : Arrow
{
    [Header("Electric Stats (Quick Shot)")]
    [SerializeField] private float quickDamage = 10f;
    
    [Header("Electric Stats (Charged AoE)")]
    [SerializeField] private float aoeRadius = 4f;
    [SerializeField] private float chargedDamage = 25f;

    public override ArrowType type => ArrowType.Electric;
    public override DamageType damageType => DamageType.Electric;

    protected override void OnHit(Collider other)
    {
        ConductiveSurface surface = other.GetComponent<ConductiveSurface>();
        if (surface != null)
        {
            surface.Electrify();
            return;
        }

        if (isFullyCharged)
        {
            ApplyAoEDamage();
        }
        else
        {
            ApplySingleTargetDamage(other);
        }
    }

    private void ApplySingleTargetDamage(Collider other)
    {
        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target != null)
        {
            float multiplier = GetDamageMultiplier(other);
            target.TakeDamage(quickDamage * multiplier, damageType);
            ApplySlow(other.gameObject);

            EnemyController enemy = other.GetComponentInParent<EnemyController>();
            float markerDuration = enemy != null && enemy.Config != null ? enemy.Config.timeStunned : 3f;
            target.TakeRecurrentDamage(0f, markerDuration, 1, damageType);
        }
    }

    private void ApplyAoEDamage()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, aoeRadius);
        HashSet<IDamageable> processedTargets = new HashSet<IDamageable>();

        foreach (Collider col in colliders)
        {
            IDamageable target = col.GetComponentInParent<IDamageable>();
            if (target != null && !processedTargets.Contains(target))
            {
                processedTargets.Add(target);
                target.TakeDamage(chargedDamage, damageType);
                ApplySlow(col.gameObject);

                EnemyController enemy = col.GetComponentInParent<EnemyController>();
                float markerDuration = enemy != null && enemy.Config != null ? enemy.Config.timeStunned : 3f;
                target.TakeRecurrentDamage(0f, markerDuration, 1, damageType);
            }
        }
    }

    private void ApplySlow(GameObject targetObj)
    {
        ISlowable slowable = targetObj.GetComponentInParent<ISlowable>();
        slowable?.ApplySlow();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}