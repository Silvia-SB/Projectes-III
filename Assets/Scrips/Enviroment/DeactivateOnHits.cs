using UnityEngine;
using System.Linq;

public class DeactivateOnHits : MonoBehaviour, IDamageable, IResettable
{
    [Header("Settings")]
    [SerializeField] private float maxHealth = 100f;
    
    [SerializeField] private DamageType[] validDamageTypes;

    private float currentHealth;

    private void OnEnable()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, DamageType incomingDamageType)
    {
        ProcessDamage(amount, incomingDamageType);
    }

    public void TakeRecurrentDamage(float amount, float interval, int ticks, DamageType incomingDamageType)
    {
        ProcessDamage(amount, incomingDamageType);
    }

    private void ProcessDamage(float amount, DamageType damageType)
    {
        if (validDamageTypes != null && validDamageTypes.Length > 0 && !validDamageTypes.Contains(damageType))
        {
            return;
        }

        currentHealth -= amount;
        
        if (currentHealth <= 0f)
        {
            gameObject.SetActive(false);
        }
    }

    public void CaptureInitialState()
    {
        //Dont need to capture initial state
    }
    public void ResetState()
    {
        Debug.Log("ResetState DeactivateOnHits: " + gameObject.name);
        currentHealth = maxHealth;
        gameObject.SetActive(true);
    }
}