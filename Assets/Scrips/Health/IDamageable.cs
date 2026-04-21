public interface IDamageable
{
    void TakeDamage(float amount);
    void TakeRecurrentDamage(float amount, float interval, int ticks);
}