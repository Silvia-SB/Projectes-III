using UnityEngine;

public class BaseArrow : Arrow
{
    public override ArrowType type => ArrowType.Base;

    protected override void OnHit(Collider other)
    {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }
}