using UnityEngine;

public class BaseArrow : Arrow 
{
    public override ArrowType type => ArrowType.Base;
    [SerializeField] private float baseDamage = 5f;
    [SerializeField] private float maxDamage = 15f;

    protected override void OnHit(Collider other) 
    {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null) 
        {
            float finalDamage = isFullyCharged ? maxDamage : baseDamage;
            damageable.TakeDamage(finalDamage);
        }
    }
}