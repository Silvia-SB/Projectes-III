using UnityEngine;
using System.Collections.Generic;

public class ExplosiveObject : Health, IResettable
{
    private const int InfiniteTicks = 9999;
    private const float DefaultDotAmount = 5f;

    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float instantDamage = 50f;
    [SerializeField] private float dotAmount = 5f;
    [SerializeField] private float dotInterval = 0.4f;
    [SerializeField] private int dotTicks = 5;
    [SerializeField] private DamageType damageType = DamageType.Blood;

    [Header("Effects")]
    [SerializeField] private ParticleSystem[] idleParticles;
    [SerializeField] private ParticleSystem[] explosionParticles;
    [SerializeField] private Light explosionLight;

    private bool hasExploded;
    private bool isIgnited;

    protected override void OnEnable()
    {
        base.OnEnable();
        RestoreInitialState();
    }

    public override void TakeDamage(float amount, DamageType incomingDamageType)
    {
        if (incomingDamageType != damageType) return;
        
        Ignite();

        base.TakeDamage(amount, incomingDamageType);

        if (statusManager?.HasStatus(damageType) == false)
        {
            float initialDotAmount = amount > 0 ? amount : DefaultDotAmount;
            base.TakeRecurrentDamage(initialDotAmount, dotInterval, InfiniteTicks, incomingDamageType);
        }
    }

    public override void TakeRecurrentDamage(float amount, float interval, int ticks, DamageType incomingDamageType)
    {
        if (incomingDamageType != damageType) return;
        
        Ignite();

        base.TakeRecurrentDamage(amount, interval, InfiniteTicks, incomingDamageType);
    }

    private void Ignite()
    {
        if (isIgnited) return;
        isIgnited = true;

        if (idleParticles != null)
        {
            foreach (var ps in idleParticles)
            {
                if (ps != null) ps.Play();
            }
        }
    }

    protected override void Die()
    {
        if (!hasExploded)
        {
            hasExploded = true;
            Explode();
        }
        
        base.Die();

        foreach (var col in GetComponentsInChildren<Collider>()) col.enabled = false;
        foreach (var rend in GetComponentsInChildren<Renderer>())
        {
            if (rend is not ParticleSystemRenderer) rend.enabled = false;
        }
        Invoke(nameof(DeactivateObject), 5f);
    }

    private void DeactivateObject()
    {
        gameObject.SetActive(false);
    }

    private void Explode()
    {
        AchievementManager.UnlockAchievement("environment_barrel");

        ActivateEffects();

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        var damagedTargets = new HashSet<IDamageable>();

        foreach (Collider col in colliders)
        {
            if (col.gameObject == gameObject) continue;

            IDamageable target = col.GetComponentInParent<IDamageable>();
            
            if (target != null && damagedTargets.Add(target))
            {
                target.TakeRecurrentDamage(dotAmount, dotInterval, dotTicks, damageType);
                target.TakeDamage(instantDamage, damageType);

                HeadBobController headBob = col.GetComponentInParent<HeadBobController>();
                if (headBob != null)
                {
                    headBob.TriggerExplosionShake();
                }
            }
        }
    }

    private void ActivateEffects()
    {
        if (idleParticles != null)
        {
            foreach (var ps in idleParticles)
            {
                if (ps != null) ps.Stop();
            }
        }

        if (explosionLight != null)
        {
            explosionLight.enabled = true;
            Invoke(nameof(TurnOffLight), 1.5f); 
        }

        if (explosionParticles != null)
        {
            foreach (var ps in explosionParticles)
            {
                if (ps != null)
                {
                    ps.Play();
                }
            }
        }
    }

    private void TurnOffLight()
    {
        if (explosionLight != null) explosionLight.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    public void CaptureInitialState()
    {
        //Dont need to capture initial state
    }
    public void ResetState()
    {
        CancelInvoke();
        currentHealth = maxHealth;
        ClearStatuses();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            return;
        }

        RestoreInitialState();
    }

    private void RestoreInitialState()
    {
        hasExploded = false;
        isIgnited = false;

        foreach (var col in GetComponentsInChildren<Collider>(true)) col.enabled = true;
        foreach (var rend in GetComponentsInChildren<Renderer>(true))
        {
            if (rend is not ParticleSystemRenderer) rend.enabled = true;
        }

        if (explosionLight != null) explosionLight.enabled = false;
        if (explosionParticles != null)
        {
            foreach (var ps in explosionParticles)
            {
                if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (idleParticles != null)
        {
            foreach (var ps in idleParticles)
            {
                if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
