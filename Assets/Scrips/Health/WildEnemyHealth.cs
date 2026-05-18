using System;
using UnityEngine;

public class WildEnemyHealth : Health
{
    [Header("Enemy Rewards")]
    [SerializeField] private int soulsToDrop = 15;
    private bool isDamaged;

    public event Action OnDamaged;

    public override void TakeDamage(float amount, DamageType damageType)
    {
        if(!isDamaged)
        {
            isDamaged = true;
            OnDamaged?.Invoke();
        }
        base.TakeDamage(amount, damageType);
    }

    public override void TakeRecurrentDamage(float amount, float interval, int ticks, DamageType damageType)
    {
        if(!isDamaged)
        {
            isDamaged = true;
            OnDamaged?.Invoke();
        }
        base.TakeRecurrentDamage(amount, interval,ticks, damageType);
    }


    protected override void Die()
    {
        if (SoulManager.Instance != null)
        {
            SoulManager.Instance.AddSouls(soulsToDrop);
        }

        base.Die();
    }
}