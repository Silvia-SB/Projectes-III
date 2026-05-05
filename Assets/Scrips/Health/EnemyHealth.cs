using UnityEngine;

public class EnemyHealth : Health
{
    [Header("Enemy Rewards")]
    [SerializeField] private int soulsToDrop = 15;

    protected override void Die()
    {
        if (SoulManager.Instance != null)
        {
            SoulManager.Instance.AddSouls(soulsToDrop);
        }

        base.Die();
    }
}