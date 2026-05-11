using UnityEngine;
using System.Collections.Generic;

public class CorruptedLiquid : MonoBehaviour, IDamageable
{
    [SerializeField] private float damage = 5f;
    [SerializeField] private float interval = 0.4f;
    [SerializeField] private int ticksOnExit = 5;
    [SerializeField] private DamageType damageType = DamageType.Blood;
    
    private Collider[] colliders; 
    private Renderer meshRenderer;
    private bool isActive;
    private float nextPulseTime;
    
    private readonly List<IDamageable> targets = new List<IDamageable>();

    private void Awake() 
    {
        colliders = GetComponents<Collider>();
        meshRenderer = GetComponent<Renderer>();
        
        foreach (Collider c in colliders)
        {
            c.isTrigger = false;
        }
    }

    public void TakeDamage(float amount, DamageType incomingDamageType)
    {
        if (incomingDamageType == damageType) 
        {
            Activate();
        }
    }

    public void TakeRecurrentDamage(float amount, float interval, int ticks, DamageType incomingDamageType)
    {
        if (incomingDamageType == damageType) 
        {
            Activate();
        }
    }

    public void Activate() 
    {
        if (isActive) return;
        
        isActive = true;
        nextPulseTime = Time.time + interval;

        if (meshRenderer != null) 
        {
            meshRenderer.material.color = Color.red;
        }
        
        foreach (Collider c in colliders)
        {
            c.isTrigger = true;
        }
    }

    private void Update() 
    {
        if (!isActive || Time.time < nextPulseTime) return;
        
        nextPulseTime = Time.time + interval;
        
        for (int i = targets.Count - 1; i >= 0; i--) 
        {
            IDamageable target = targets[i];
            
            if (target is not MonoBehaviour obj || !obj.gameObject.activeInHierarchy) 
            {
                targets.RemoveAt(i);
            } 
            else 
            {
                target.TakeDamage(damage, damageType);
            }
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (!isActive) return;
        
        IDamageable target = other.GetComponentInParent<IDamageable>();
        
        if (target != null && !targets.Contains(target)) 
        {
            targets.Add(target);
            target.TakeDamage(damage, damageType);
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (!isActive) return;
        
        IDamageable target = other.GetComponentInParent<IDamageable>();
        
        if (target != null && targets.Remove(target)) 
        {
            target.TakeRecurrentDamage(damage, interval, ticksOnExit, damageType);
        }
    }
}