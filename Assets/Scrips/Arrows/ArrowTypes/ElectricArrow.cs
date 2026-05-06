using UnityEngine;

public class ElectricArrow : Arrow
{
    [Header("Electric Stats (Quick Shot)")]
    [SerializeField] private float quickDamage = 10f;
    
    [Header("Electric Stats (Charged AoE)")]
    [SerializeField] private float aoeRadius = 4f;
    [SerializeField] private float chargedDamage = 25f;

    [Header("Slow Effect")]
    [SerializeField] private float slowFactor = 0.5f;
    [SerializeField] private float slowDuration = 2f;

    public override ArrowType type => ArrowType.Electric;
    public override DamageType damageType => DamageType.Electric;

    protected override void OnHit(Collider other)
    {
        ConductiveSurface surface = other.GetComponent<ConductiveSurface>();
        if (surface != null)
        {
            float electrifyDuration = isFullyCharged ? slowDuration * 2.0f : slowDuration;
            surface.Electrify(electrifyDuration);
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
            target.TakeDamage(quickDamage, damageType);
            float appliedDuration = ApplySlow(other.gameObject);

            int ticks = Mathf.CeilToInt(appliedDuration);
            float interval = 1f;
            target.TakeRecurrentDamage(0f, interval, ticks, DamageType.Electric);
        }
    }

    private void ApplyAoEDamage()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, aoeRadius);
        foreach (Collider col in colliders)
        {
            IDamageable target = col.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(chargedDamage, damageType);
                ApplySlow(col.gameObject);
            }
        }
    }

    private float ApplySlow(GameObject targetObj)
    {
        ISlowable slowable = targetObj.GetComponentInParent<ISlowable>();
        EnemyController enemy = targetObj.GetComponentInParent<EnemyController>();
        
        float appliedDuration = slowDuration; // Valor por defecto (útil si golpeas objetos que no son enemigos)

        if (slowable != null)
        {
            if (enemy != null && enemy.Config != null)
            {
                // Calculamos el factor de ralentización necesario para que la velocidad final sea "stunnedSpeed"
                float calculatedSlowFactor = enemy.Config.stunnedSpeed / enemy.Config.speed;
                appliedDuration = enemy.Config.timeStunned;
                slowable.ApplySlow(calculatedSlowFactor, appliedDuration);
            }
            else
            {
                slowable.ApplySlow(slowFactor, appliedDuration);
            }
        }

        return appliedDuration;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}