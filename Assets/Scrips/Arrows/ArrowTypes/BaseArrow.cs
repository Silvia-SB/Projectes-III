using UnityEngine;

public class BaseArrow : Arrow 
{
    public override ArrowType type => ArrowType.Base;
    [SerializeField] private float baseDamage = 10f;

    protected override void OnHit(Collider other, float damageMultiplier) 
    {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null) 
        {
            damageable.TakeDamage(baseDamage * damageMultiplier);
        }
    }
}