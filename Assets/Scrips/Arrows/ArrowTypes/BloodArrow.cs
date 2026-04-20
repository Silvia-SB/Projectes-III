using UnityEngine;

public class BloodArrow : Arrow
{
    public override ArrowType type => ArrowType.Blood;

    protected override void OnHit(Collider other)
    {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }
}