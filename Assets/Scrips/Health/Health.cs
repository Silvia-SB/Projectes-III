using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(StatusEffectManager))]
public abstract class Health : MonoBehaviour, IDamageable
{
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public DamageType LastDamageType { get; private set; }

    public UnityEvent OnDeath;

    protected StatusEffectManager statusManager;

    protected virtual void Awake() 
    {
        statusManager = GetComponent<StatusEffectManager>();
    }

    protected virtual void OnEnable()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float amount, DamageType damageType)
    {
        LastDamageType = damageType;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public virtual void TakeRecurrentDamage(float amount, float interval, int ticks, DamageType damageType)
    {
        statusManager?.ApplyStatus(amount, interval, ticks, damageType);
    }

    protected virtual void Die()
    {
        if(gameObject.CompareTag("Wall")) gameObject.SetActive(false);
        OnDeath?.Invoke();
        ClearStatuses();
    }

    public void ClearStatuses()
    {
        statusManager?.ClearAllStatuses();
    }
    
}