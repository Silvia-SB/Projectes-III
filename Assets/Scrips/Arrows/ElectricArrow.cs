using UnityEngine;

public class ElectricArrow : Arrow
{
    [Header("Electric Stats (Quick Shot)")]
    [SerializeField] private float quickDamage = 10f;
    
    [Header("Electric Stats (Charged AoE)")]
    [SerializeField] private float aoeRadius = 4f;
    [SerializeField] private float chargedDamage = 25f;

    [Header("Slow Effect")]
    [SerializeField] private float slowFactor = 0.5f; // Reduce la velocidad a la mitad
    [SerializeField] private float slowDuration = 2f;

    // Asegúrate de tener 'Electric' añadido en tu enum ArrowType
    public override ArrowType type => ArrowType.Electric;
    public override DamageType damageType => DamageType.Electric;

    protected override void OnHit(Collider other)
    {
        // Prioridad 1: Si la flecha impacta en una superficie conductora (agua).
        ConductiveSurface surface = other.GetComponent<ConductiveSurface>();
        if (surface != null)
        {
            // El disparo cargado electrifica el agua por más tiempo.
            float electrifyDuration = isFullyCharged ? slowDuration * 2.0f : slowDuration;
            surface.Electrify(electrifyDuration);
            // No hacemos nada más, la flecha se consume. La clase base se encarga de devolverla al pool.
            return;
        }

        // Prioridad 2: Lógica de impacto normal.
        if (isFullyCharged)
        {
            ApplyAoEDamage();
        }
        else
        {
            // Solo el disparo rápido (no cargado) genera contagio.
            ApplySingleTargetDamage(other);
        }
    }

    private void ApplySingleTargetDamage(Collider other)
    {
        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(quickDamage, damageType);
            ApplySlow(other.gameObject);

            // Aprovechamos StatusEffectManager para marcar al enemigo como electrificado.
            // Aplicamos un DoT de 0 de daño para que EnemyContagion lo detecte y lo propague.
            int ticks = Mathf.CeilToInt(slowDuration);
            float interval = 1f; // 1 tick por segundo
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
                // En el disparo cargado, el daño y el slow se aplican en área,
                // pero no se genera el efecto de contagio posterior.
                // El área de efecto ya es el "contagio" inicial.
            }
        }
    }

    private void ApplySlow(GameObject targetObj)
    {
        ISlowable slowable = targetObj.GetComponentInParent<ISlowable>();
        if (slowable != null)
        {
            slowable.ApplySlow(slowFactor, slowDuration);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}