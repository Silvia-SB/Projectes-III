using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public UnityEvent OnDeath;

    [Header("Visuales")]
    [SerializeField] private Renderer mainRenderer;
    [SerializeField] private Color damageColor = Color.red;

    private float recurrentDamageAmount;
    private float recurrentDamageInterval;
    private int recurrentDamageTicksRemaining;
    private float recurrentTimer;
    
    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (recurrentDamageTicksRemaining > 0)
        {
            recurrentTimer += Time.deltaTime;

            if (recurrentTimer >= recurrentDamageInterval)
            {
                recurrentTimer -= recurrentDamageInterval;
                TakeDamage(recurrentDamageAmount);
                recurrentDamageTicksRemaining--;
            }
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (mainRenderer != null)
        {
            mainRenderer.material.color = damageColor;
        }

        if (currentHealth <= 0)
        {
            Die();
        }

    }

    public void TakeRecurrentDamage(float amount, float interval, int ticks)
    {
        recurrentDamageAmount = amount;
        recurrentDamageInterval = interval;
        recurrentDamageTicksRemaining = ticks;
        recurrentTimer = 0f; 
    }

    private void Die()
    {
        recurrentDamageTicksRemaining = 0; 
        OnDeath?.Invoke();
        gameObject.SetActive(false);
    }
}