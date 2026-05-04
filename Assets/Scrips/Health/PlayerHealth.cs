using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHealth : Health
{
    [Header("Healing")]
    [SerializeField] private int soulCostToHeal = 20;
    [SerializeField] private float healAmount = 30f;

    protected override void Awake()
    {
        base.Awake();
        
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            OnDeath.AddListener(gm.ReloadCurrentScene);
        }
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