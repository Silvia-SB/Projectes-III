using UnityEngine;

public class BloodArrow : Arrow 
{
    public override ArrowType type => ArrowType.Blood;
    [SerializeField] private float baseDamage = 15f;

    protected override void OnHit(Collider other, float damageMultiplier) 
    {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null) 
        {
            damageable.TakeDamage(baseDamage * damageMultiplier);
        }
    }
}