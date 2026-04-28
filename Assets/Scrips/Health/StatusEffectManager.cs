using UnityEngine;
using System.Collections.Generic;

public class DoTInstance
{
    public float Amount;
    public float Interval;
    public int TicksRemaining;
    public float Timer;
}

public class StatusEffectManager : MonoBehaviour
{
    private Dictionary<DamageType, DoTInstance> activeStatuses = new Dictionary<DamageType, DoTInstance>();
    private List<DamageType> keysToRemove = new List<DamageType>();
    
    private IDamageable damageable;

    private void Awake()
    {
        // Busca el IDamageable en este objeto o en sus padres para aplicarle el daño
        damageable = GetComponentInParent<IDamageable>();
    }

    public bool HasStatus(DamageType type) => activeStatuses.ContainsKey(type) && activeStatuses[type].TicksRemaining > 0;
    public DoTInstance GetStatus(DamageType type) => activeStatuses.TryGetValue(type, out var dot) ? dot : null;

    public void ApplyStatus(float amount, float interval, int ticks, DamageType damageType)
    {
        if (activeStatuses.TryGetValue(damageType, out DoTInstance dot))
        {
            dot.Amount = amount;
            dot.Interval = interval;
            dot.TicksRemaining = ticks;
            dot.Timer = 0f;
        }
        else
        {
            activeStatuses[damageType] = new DoTInstance 
            { 
                Amount = amount, 
                Interval = interval, 
                TicksRemaining = ticks, 
                Timer = 0f 
            };
        }
    }

    public void RemoveStatus(DamageType damageType)
    {
        if (activeStatuses.ContainsKey(damageType))
        {
            activeStatuses.Remove(damageType);
        }
    }

    public void ClearAllStatuses()
    {
        activeStatuses.Clear();
    }

    private void Update()
    {
        keysToRemove.Clear();
        foreach (var kvp in activeStatuses)
        {
            var dot = kvp.Value;
            dot.Timer += Time.deltaTime;
            if (dot.Timer >= dot.Interval)
            {
                dot.Timer -= dot.Interval;
                
                if (damageable != null)
                {
                    damageable.TakeDamage(dot.Amount, kvp.Key); 
                }
                
                dot.TicksRemaining--;
                
                if (dot.TicksRemaining <= 0)
                    keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            activeStatuses.Remove(key);
        }
    }
}