using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerHealth : Health
{
    [Header("Healing")]
    [SerializeField] private int soulCostToHeal = 20;
    [SerializeField] private float healAmount = 30f;

    public event Action<float, float> OnHealthChanged;

    protected override void Awake()
    {
        base.Awake();
        
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            OnDeath.AddListener(gm.ReloadCurrentScene);
        }
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public override void TakeDamage(float amount, DamageType damageType)
    {
        base.TakeDamage(amount, damageType);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public override void Heal(float amount)
    {
        base.Heal(amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void OnHeal(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TryHealWithSouls();
        }
    }

    private bool TryHealWithSouls()
    {
        if (currentHealth >= maxHealth) return false;

        if (SoulManager.Instance != null && SoulManager.Instance.TryConsumeSouls(soulCostToHeal))
        {
            Heal(healAmount);
            return true;
        }
        return false;
    }
}