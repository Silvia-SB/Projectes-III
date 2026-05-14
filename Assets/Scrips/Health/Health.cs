using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(StatusEffectManager))]
public abstract class Health : MonoBehaviour, IDamageable
{
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public UnityEvent OnDeath;

    [Header("Visuals")]
    [SerializeField] private Renderer mainRenderer;
    [SerializeField] private Color damageColor = Color.red;

    protected StatusEffectManager statusManager;
    private MaterialPropertyBlock propBlock;

    protected virtual void Awake() 
    {
        statusManager = GetComponent<StatusEffectManager>();
        
        if (mainRenderer == null)
        {
            mainRenderer = GetComponentInChildren<Renderer>();
        }

        if (mainRenderer != null)
        {
            propBlock = new MaterialPropertyBlock();
        }
    }

    protected virtual void OnEnable()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float amount, DamageType damageType)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (amount > 0) ApplyDamageVisuals();

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
        if (amount > 0) ApplyDamageVisuals();
    }

    private void ApplyDamageVisuals()
    {
        if (mainRenderer != null && propBlock != null)
        {
            mainRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", damageColor);
            mainRenderer.SetPropertyBlock(propBlock);
        }
    }

    protected virtual void Die()
    {
        if(gameObject.CompareTag("Wall")) Destroy(gameObject);
        OnDeath?.Invoke();
        statusManager?.ClearAllStatuses();

    }
}