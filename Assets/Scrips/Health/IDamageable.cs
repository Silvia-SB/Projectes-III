public interface IDamageable
{
    void TakeDamage(float amount, DamageType damageType);
    void TakeRecurrentDamage(float amount, float interval, int ticks, DamageType damageType);
}