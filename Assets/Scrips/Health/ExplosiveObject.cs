using UnityEngine;
using System.Collections.Generic;

public class ExplosiveObject : Health
{
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float instantDamage = 50f;
    [SerializeField] private float dotAmount = 5f;
    [SerializeField] private float dotInterval = 0.4f;
    [SerializeField] private int dotTicks = 5;

    private bool hasExploded = false;

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
                target.TakeDamage(instantDamage);
                target.TakeRecurrentDamage(dotAmount, dotInterval, dotTicks);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}