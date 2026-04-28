using UnityEngine;
using System.Collections.Generic;

public class ExplosiveObject : Health
{
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float instantDamage = 50f;
    [SerializeField] private float dotAmount = 5f;
    [SerializeField] private float dotInterval = 0.4f;
    [SerializeField] private int dotTicks = 5;
    [SerializeField] private DamageType damageType = DamageType.Blood;

    private bool hasExploded = false;

    public override void TakeDamage(float amount, DamageType incomingDamageType)
    {
        if (incomingDamageType != this.damageType) return;
        base.TakeDamage(amount, incomingDamageType);

        // Si recibe daño instantáneo, "encendemos la mecha" asegurando que empiece el daño recurrente infinito
        if (statusManager != null && !statusManager.HasStatus(this.damageType))
        {
            base.TakeRecurrentDamage(amount > 0 ? amount : 5f, dotInterval, 9999, incomingDamageType);
        }
    }

    public override void TakeRecurrentDamage(float amount, float interval, int ticks, DamageType incomingDamageType)
    {
        if (incomingDamageType != this.damageType) return;
        // Ignoramos los ticks que le pasa el atacante y le ponemos un número infinito para que no pare hasta detonar
        base.TakeRecurrentDamage(amount, interval, 9999, incomingDamageType);
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
        List<IDamageable> damagedTargets = new List<IDamageable>();

        foreach (Collider col in colliders)
        {
            if (col.gameObject == gameObject) continue;

            IDamageable target = col.GetComponentInParent<IDamageable>();
            if (target != null && !damagedTargets.Contains(target))
            {
                damagedTargets.Add(target);
                target.TakeDamage(instantDamage, this.damageType);
                target.TakeRecurrentDamage(dotAmount, dotInterval, dotTicks, this.damageType);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}