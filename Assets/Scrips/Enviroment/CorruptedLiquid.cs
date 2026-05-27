using UnityEngine;
using System.Collections.Generic;

public class CorruptedLiquid : MonoBehaviour, IDamageable
{
    [SerializeField] private float damage = 5f;
    [SerializeField] private float interval = 0.4f;
    [SerializeField] private int ticksOnExit = 5;
    [SerializeField] private DamageType damageType = DamageType.Blood;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem[] corruptedParticles;
    [SerializeField] private Light corruptedLight;

    [Header("Deactivation")]
    [SerializeField] private Collider deactivationCollider;
    [SerializeField] private string deactivatorTag = "Player";

    private Collider[] colliders; 
    private bool isActive;
    private float nextPulseTime;
    
    private readonly List<IDamageable> targets = new List<IDamageable>();

    private void Awake() 
    {
        colliders = GetComponents<Collider>();
        
        foreach (Collider c in colliders)
        {
            c.isTrigger = true;
        }
        
        if (deactivationCollider != null)
        {
            deactivationCollider.isTrigger = true;
            TriggerListener listener = deactivationCollider.gameObject.AddComponent<TriggerListener>();
            listener.OnTriggerEntered += HandleDeactivation;
        }
        
        DeactivateEffects();
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

        ActivateEffects();

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

    private void HandleDeactivation(Collider other)
    {
        if (string.IsNullOrEmpty(deactivatorTag) || other.CompareTag(deactivatorTag))
        {
            Deactivate();
        }
    }

    public void Deactivate()
    {
        if (!isActive) return;
        isActive = false;
        DeactivateEffects();
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
        IDamageable target = other.GetComponentInParent<IDamageable>();
        
        if (target != null && !targets.Contains(target)) 
        {
            targets.Add(target);
            if (isActive) 
            {
                target.TakeDamage(damage, damageType);
            }
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        IDamageable target = other.GetComponentInParent<IDamageable>();
        
        if (target != null && targets.Remove(target)) 
        {
            if (isActive) 
            {
                target.TakeRecurrentDamage(damage, interval, ticksOnExit, damageType);
            }
        }
    }

    private void ActivateEffects()
    {
        if (corruptedLight != null) corruptedLight.enabled = true;

        if (corruptedParticles != null)
        {
            foreach (var ps in corruptedParticles)
            {
                if (ps != null) ps.Play();
            }
        }
    }

    private void DeactivateEffects()
    {
        if (corruptedLight != null) corruptedLight.enabled = false;

        if (corruptedParticles != null)
        {
            foreach (var ps in corruptedParticles)
            {
                if (ps != null) ps.Stop();
            }
        }
    }
}