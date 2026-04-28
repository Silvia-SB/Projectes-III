using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(StatusEffectManager))]
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public UnityEvent OnDeath;

    [Header("Visuales")]
    [SerializeField] private Renderer mainRenderer;
    [SerializeField] private Color damageColor = Color.red;

    protected StatusEffectManager statusManager;

    protected virtual void Awake() 
    {
        currentHealth = maxHealth;
        statusManager = GetComponent<StatusEffectManager>();
        if (statusManager == null)
        {
            statusManager = gameObject.AddComponent<StatusEffectManager>();
        }
    }

    public virtual void TakeDamage(float amount, DamageType damageType)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (mainRenderer != null) mainRenderer.material.SetColor("_BaseColor", damageColor);

        if (currentHealth <= 0) Die();
    }

    public virtual void TakeRecurrentDamage(float amount, float interval, int ticks, DamageType damageType)
    {
        if (statusManager != null)
        {
            statusManager.ApplyStatus(amount, interval, ticks, damageType);
        }

        if (mainRenderer != null) mainRenderer.material.SetColor("_BaseColor", damageColor);
    }

    protected virtual void Die()
    {
        if (statusManager != null) statusManager.ClearAllStatuses();
        gameObject.SetActive(false);
    }
}