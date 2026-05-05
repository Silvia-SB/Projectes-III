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
    private readonly Dictionary<DamageType, DoTInstance> activeStatuses = new Dictionary<DamageType, DoTInstance>();
    private readonly List<DamageType> activeKeys = new List<DamageType>();
    
    private IDamageable damageable;

    private void Awake()
    {
        damageable = GetComponentInParent<IDamageable>();
    }

    public bool HasStatus(DamageType type) => activeStatuses.TryGetValue(type, out var dot) && dot.TicksRemaining > 0;
    
    public DoTInstance GetStatus(DamageType type) => activeStatuses.GetValueOrDefault(type);

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
            activeKeys.Add(damageType);
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
        activeStatuses.Remove(damageType);
        activeKeys.Remove(damageType);
    }

    public void ClearAllStatuses()
    {
        activeStatuses.Clear();
        activeKeys.Clear();
    }

    private void Update()
    {
        for (int i = activeKeys.Count - 1; i >= 0; i--)
        {
            DamageType key = activeKeys[i];
            DoTInstance dot = activeStatuses[key];
            
            dot.Timer += Time.deltaTime;
            
            if (dot.Timer >= dot.Interval)
            {
                dot.Timer -= dot.Interval;
                
                damageable?.TakeDamage(dot.Amount, key); 
                
                if (!gameObject.activeInHierarchy) return;
                
                dot.TicksRemaining--;
                
                if (dot.TicksRemaining <= 0)
                {
                    activeStatuses.Remove(key);
                    activeKeys.RemoveAt(i);
                }
            }
        }
    }
}