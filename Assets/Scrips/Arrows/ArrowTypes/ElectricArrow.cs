using UnityEngine;
using System.Collections.Generic;

public class ElectricArrow : Arrow
{
    [Header("Electric Stats (Quick Shot)")]
    [SerializeField] private float quickDamage = 10f;
    
    [Header("Electric Stats (Charged AoE)")]
    [SerializeField] private float aoeRadius = 4f;
    [SerializeField] private float chargedDamage = 25f;

    [Header("Knight Bonus")]
    [SerializeField] private float knightMultiplier = 1.5f;

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
            EnemyController enemy = other.GetComponentInParent<EnemyController>();
            float multiplier = GetDamageMultiplier(other);
            float bonus = GetKnightBonus(enemy);
            
            target.TakeDamage(quickDamage * multiplier * bonus, damageType);
            ApplySlow(other.gameObject);

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
                
                EnemyController enemy = col.GetComponentInParent<EnemyController>();
                float bonus = GetKnightBonus(enemy);
                
                target.TakeDamage(chargedDamage * bonus, damageType);
                ApplySlow(col.gameObject);

                float markerDuration = enemy != null && enemy.Config != null ? enemy.Config.timeStunned : 3f;
                target.TakeRecurrentDamage(0f, markerDuration, 1, damageType);
            }
        }
    }

    private float GetKnightBonus(EnemyController enemy)
    {
        if (enemy != null && enemy.Config != null)
        {
            string typeName = enemy.Config.type.ToString().ToLower();
            if (typeName.Contains("caballero") || typeName.Contains("knight")) return knightMultiplier;
        }
        return 1f;
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