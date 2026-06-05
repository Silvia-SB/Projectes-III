using UnityEngine;

public class BloodCircleIndicator : MonoBehaviour, IDamageable
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem[] bloodParticles;
    [SerializeField] private Light bloodLight;

    [Header("Settings")]
    [SerializeField] private float activeDuration = 5f;

    private float timer;
    private bool isActive = false;

    private void Awake()
    {
        DeactivateEffects();
    }

    private void Update()
    {
        if (isActive)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                DeactivateEffects();
            }
        }
    }

    public void TakeDamage(float amount, DamageType type)
    {
        if (type == DamageType.Blood)
        {
            ActivateEffects();
        }
    }

    public void TakeRecurrentDamage(float amount, float interval, int ticks, DamageType type)
    {
        if (type == DamageType.Blood)
        {
            ActivateEffects();
        }
    }

    private void ActivateEffects()
    {
        timer = activeDuration; // Reinicia el temporizador si vuelve a ser golpeado

        if (!isActive)
        {
            isActive = true;
            
            AchievementManager.UnlockAchievement("environment_light");

            if (bloodLight != null) bloodLight.enabled = true;

            if (bloodParticles != null)
            {
                foreach (var ps in bloodParticles)
                {
                    if (ps != null) ps.Play();
                }
            }
        }
    }

    private void DeactivateEffects()
    {
        isActive = false;
        
        if (bloodLight != null) bloodLight.enabled = false;

        if (bloodParticles != null)
        {
            foreach (var ps in bloodParticles)
            {
                if (ps != null) ps.Stop();
            }
        }
    }
}